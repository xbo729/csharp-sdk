using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Protocol;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace ModelContextProtocol.Client;

/// <summary>
/// Provides a <see cref="IClientTransport"/> implemented via "stdio" (standard input/output).
/// </summary>
/// <remarks>
/// <para>
/// This transport launches an external process and communicates with it through standard input and output streams.
/// It's used to connect to MCP servers launched and hosted in child processes.
/// </para>
/// <para>
/// The transport manages the entire lifecycle of the process: starting it with specified command-line arguments
/// and environment variables, handling output, and properly terminating the process when the transport is closed.
/// </para>
/// </remarks>
public sealed partial class StdioClientTransport : IClientTransport
{
    private readonly StdioClientTransportOptions _options;
    private readonly ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="StdioClientTransport"/> class.
    /// </summary>
    /// <param name="options">Configuration options for the transport, including the command to execute, arguments, working directory, and environment variables.</param>
    /// <param name="loggerFactory">Logger factory for creating loggers used for diagnostic output during transport operations.</param>
    public StdioClientTransport(StdioClientTransportOptions options, ILoggerFactory? loggerFactory = null)
    {
        Throw.IfNull(options);

        _options = options;
        _loggerFactory = loggerFactory;
        Name = options.Name ?? $"stdio-{Regex.Replace(Path.GetFileName(options.Command), @"[\s\.]+", "-")}";
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public async Task<ITransport> ConnectAsync(CancellationToken cancellationToken = default)
    {
        string endpointName = Name;

        Process? process = null;
        bool processStarted = false;

        string command = _options.Command;
        IList<string>? arguments = _options.Arguments;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
            !string.Equals(Path.GetFileName(command), "cmd.exe", StringComparison.OrdinalIgnoreCase))
        {
            // On Windows, for stdio, we need to wrap non-shell commands with cmd.exe /c {command} (usually npx or uvicorn).
            // The stdio transport will not work correctly if the command is not run in a shell.
            arguments = arguments is null or [] ? ["/c", command] : ["/c", command, ..arguments];
            command = "cmd.exe";
        }

        ILogger logger = (ILogger?)_loggerFactory?.CreateLogger<StdioClientTransport>() ?? NullLogger.Instance;
        try
        {
            LogTransportConnecting(logger, endpointName);

            ProcessStartInfo startInfo = new()
            {
                FileName = command,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = _options.WorkingDirectory ?? Environment.CurrentDirectory,
                StandardOutputEncoding = StreamClientSessionTransport.NoBomUtf8Encoding,
                StandardErrorEncoding = StreamClientSessionTransport.NoBomUtf8Encoding,
#if NET
                StandardInputEncoding = StreamClientSessionTransport.NoBomUtf8Encoding,
#endif
            };

            if (arguments is not null) 
            {
#if NET
                foreach (string arg in arguments)
                {
                    startInfo.ArgumentList.Add(arg);
                }
#else
                StringBuilder argsBuilder = new();
                foreach (string arg in arguments)
                {
                    PasteArguments.AppendArgument(argsBuilder, arg);
                }

                startInfo.Arguments = argsBuilder.ToString();
#endif
            }

            if (_options.EnvironmentVariables != null)
            {
                foreach (var entry in _options.EnvironmentVariables)
                {
                    startInfo.Environment[entry.Key] = entry.Value;
                }
            }

            if (logger.IsEnabled(LogLevel.Trace))
            {
                LogCreateProcessForTransportSensitive(logger, endpointName, _options.Command,
                    startInfo.Arguments,
                    string.Join(", ", startInfo.Environment.Select(kvp => $"{kvp.Key}={kvp.Value}")),
                    startInfo.WorkingDirectory);
            }
            else
            {
                LogCreateProcessForTransport(logger, endpointName, _options.Command);
            }

            process = new() { StartInfo = startInfo };

            // Set up stderr handling. Log all stderr output, and keep the last
            // few lines in a rolling log for use in exceptions.
            const int MaxStderrLength = 10; // keep the last 10 lines of stderr
            Queue<string> stderrRollingLog = new(MaxStderrLength);
            process.ErrorDataReceived += (sender, args) =>
            {
                string? data = args.Data;
                if (data is not null)
                {
                    lock (stderrRollingLog)
                    {
                        if (stderrRollingLog.Count >= MaxStderrLength)
                        {
                            stderrRollingLog.Dequeue();
                        }

                        stderrRollingLog.Enqueue(data);
                    }

                    _options.StandardErrorLines?.Invoke(data);

                    LogReadStderr(logger, endpointName, data);
                }
            };

            // We need both stdin and stdout to use a no-BOM UTF-8 encoding. On .NET Core,
            // we can use ProcessStartInfo.StandardOutputEncoding/StandardInputEncoding, but
            // StandardInputEncoding doesn't exist on .NET Framework; instead, it always picks
            // up the encoding from Console.InputEncoding. As such, when not targeting .NET Core,
            // we temporarily change Console.InputEncoding to no-BOM UTF-8 around the Process.Start
            // call, to ensure it picks up the correct encoding.
#if !NET
            Encoding originalInputEncoding = Console.InputEncoding;
            if (HasConsole())
            {
                try
                {
                    Console.InputEncoding = StreamClientSessionTransport.NoBomUtf8Encoding;
                }
                finally
                {
                    Console.InputEncoding = originalInputEncoding;
                }
            }
#endif
            processStarted = process.Start();

            if (!processStarted)
            {
                LogTransportProcessStartFailed(logger, endpointName);
                throw new IOException("Failed to start MCP server process.");
            }

            LogTransportProcessStarted(logger, endpointName, process.Id);

            process.BeginErrorReadLine();

            return new StdioClientSessionTransport(_options, process, endpointName, stderrRollingLog, _loggerFactory);
        }
        catch (Exception ex)
        {
            LogTransportConnectFailed(logger, endpointName, ex);

            try
            {
                DisposeProcess(process, processStarted, _options.ShutdownTimeout, endpointName);
            }
            catch (Exception ex2)
            {
                LogTransportShutdownFailed(logger, endpointName, ex2);
            }

            throw new IOException("Failed to connect transport.", ex);
        }
    }
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    private static bool HasConsole()
    {
        try { return GetConsoleWindow() != IntPtr.Zero; } catch { return false; }
    }
    internal static void DisposeProcess(
        Process? process, bool processRunning, TimeSpan shutdownTimeout, string endpointName)
    {
        if (process is not null)
        {
            try
            {
                processRunning = processRunning && !HasExited(process);
                if (processRunning)
                {
                    // Wait for the process to exit.
                    // Kill the while process tree because the process may spawn child processes
                    // and Node.js does not kill its children when it exits properly.
                    process.KillTree(shutdownTimeout);
                }
            }
            finally
            {
                process.Dispose();
            }
        }
    }

    /// <summary>Gets whether <paramref name="process"/> has exited.</summary>
    internal static bool HasExited(Process process)
    {
        try
        {
            return process.HasExited;
        }
        catch
        {
            return true;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{EndpointName} connecting.")]
    private static partial void LogTransportConnecting(ILogger logger, string endpointName);

    [LoggerMessage(Level = LogLevel.Information, Message = "{EndpointName} starting server process. Command: '{Command}'.")]
    private static partial void LogCreateProcessForTransport(ILogger logger, string endpointName, string command);

    [LoggerMessage(Level = LogLevel.Trace, Message = "{EndpointName} starting server process. Command: '{Command}', Arguments: {Arguments}, Environment: {Environment}, Working directory: {WorkingDirectory}.")]
    private static partial void LogCreateProcessForTransportSensitive(ILogger logger, string endpointName, string command, string? arguments, string environment, string workingDirectory);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{EndpointName} failed to start server process.")]
    private static partial void LogTransportProcessStartFailed(ILogger logger, string endpointName);

    [LoggerMessage(Level = LogLevel.Information, Message = "{EndpointName} received stderr log: '{Data}'.")]
    private static partial void LogReadStderr(ILogger logger, string endpointName, string data);

    [LoggerMessage(Level = LogLevel.Information, Message = "{EndpointName} started server process with PID {ProcessId}.")]
    private static partial void LogTransportProcessStarted(ILogger logger, string endpointName, int processId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{EndpointName} connect failed.")]
    private static partial void LogTransportConnectFailed(ILogger logger, string endpointName, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{EndpointName} shutdown failed.")]
    private static partial void LogTransportShutdownFailed(ILogger logger, string endpointName, Exception exception);
}
