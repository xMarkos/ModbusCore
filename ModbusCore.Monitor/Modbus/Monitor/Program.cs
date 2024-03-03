using System;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using ModbusCore.Devices;
using ModbusCore.Parsers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;

namespace ModbusCore.Monitor;

public class Program
{
    private class Arguments
    {
        [Value(0, Required = true, MetaName = "Port", HelpText = "The name of the device used to listen for messages.")]
        public string? Port { get; set; }

        [Option('s', "baudrate", Default = 9600, HelpText = "Baud rate (speed) of the serial port.")]
        public int BaudRate { get; set; }

        [Option('p', "parity", Default = Parity.None, HelpText = "Parity of the serial port (None, Odd, Even, Mark, Space).")]
        public Parity Parity { get; set; }

        [Option("stop-bits", HelpText = "(Default: infer from parity) Stop bits of the serial port (None, One, Two, OnePointFive).")]
        public StopBits? StopBits { get; set; }

        [Option('o', "output", HelpText = "File path to write JSON formatted messages (1 per line). Value \"-\" will write messages to console.")]
        public string? OutputPath { get; set; }

        [Option('r', "raw-output", HelpText = "Writes frames in raw format (as received from the bus). Useful if the reader does not have dependency on our message types.")]
        public bool WriteRawFrames { get; set; }

        [Option('e', "stderr", HelpText = "Writes messages to standard error instead of standard output.", SetName = "logging-1")]
        public bool WriteToStdErr { get; set; }

        [Option('q', "quiet", HelpText = "Suppresses message logging.", SetName = "logging-2")]
        public bool Quiet { get; set; }

        [Option('v', "verbosity", Default = LogEventLevel.Information, HelpText = "Verbosity of logged messages (Verbose, Debug, Information, Warning, Error, Fatal).", SetName = "logging-2")]
        public LogEventLevel LogLevel { get; set; }
    }

    public static async Task<int> Main(string[] args)
    {
        Arguments? options;
        using (Parser parser = new Parser(c =>
        {
            c.CaseInsensitiveEnumValues = true;
            c.EnableDashDash = true;
            c.AutoHelp = true;
            c.AutoVersion = true;
            c.CaseSensitive = true;
            c.ParsingCulture = CultureInfo.InvariantCulture;
            c.HelpWriter = Console.Out;
        }))
        {
            options = parser
                .ParseArguments<Arguments>(args)
                .MapResult(x => x, x => null!);

            if (options is null)
                return 1;
        }

        {   // Initialize logger
            var config = new LoggerConfiguration();

            config.MinimumLevel.Is(options.LogLevel);

            if (!options.Quiet)
            {
                config.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Code,
                    standardErrorFromLevel: options.WriteToStdErr ? LogEventLevel.Verbose : null);
            }

            Log.Logger = config.CreateLogger();
        }

        var serializer = new JsonSerializer
        {
            Formatting = Formatting.None,
        };

        using TextWriter? outputWriter =
            options.OutputPath switch
            {
                null => null,
                "-" => Console.Out,
                _ => new StreamWriter(File.Open(options.OutputPath, FileMode.Append, FileAccess.Write, FileShare.Read)),
            };

        using CancellationTokenSource cts = new();

        Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs e) =>
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlBreak)
            {
                Log.Logger.Information("Ctrl+Break pressed -> terminating abruptly");
                Environment.Exit(-1);
            }
            else
            {
                e.Cancel = true;
                Log.Logger.Information("Ctrl+C pressed -> terminating gracefully");
                cts.Cancel();
            }
        };

        using SerilogLoggerFactory loggerFactory = new SerilogLoggerFactory();

        ExpiringMessagingContext context = new();

        SerialRtuModbusDevice? OpenDevice()
        {
            try
            {
                return new SerialRtuModbusDevice(
                    new()
                    {
                        PortName = options.Port,
                        BaudRate = options.BaudRate,
                        Parity = options.Parity,
                        StopBits = options.StopBits,
                    },
                    ParserCollection.Default,
                    loggerFactory.CreateLogger<SerialRtuModbusDevice>());
            }
            catch (IOException ex)
            {
                Log.Logger.Error(ex, "Could not open the port");
                return null;
            }
        }

        using (SerialRtuModbusDevice? device = OpenDevice())
        {
            if (device == null)
                return 2;

            int messageWidth = 0;
            string messageTemplate = "Received {Type,-8} {Class} {Message}";

            device.MessageReceived += (object? sender, ModbusMessageReceivedEventArgs e) =>
            {
                bool isRequest = e.Message.Type == ModbusMessageType.Request;

                if (isRequest)
                {
                    context.AddTransaction(Transaction.From(e.Message));
                }
                else if (e.Message.Type == ModbusMessageType.Response)
                {
                    context.RemoveTransaction(Transaction.From(e.Message));
                }

                string[] parts = e.Message.ToString()!.Split(' ', 2);
                if (messageWidth < parts[0].Length)
                {
                    messageWidth = parts[0].Length;
                    messageTemplate = $"Received {{Type,-8}} {{Class,-{messageWidth}}} {{Message}}";
                }

                Log.Logger.Information(messageTemplate, e.Message.Type, parts[0], parts[1]);

                if (outputWriter != null)
                {
                    PorcelainOutputBase output;
                    if (options.WriteRawFrames)
                    {
                        e.Message.TryWriteTo(null, out int length);
                        byte[] buffer = new byte[length];
                        e.Message.TryWriteTo(buffer, out _);

                        output = new RawPorcelainOutput(isRequest, buffer);
                    }
                    else
                    {
                        output = new PorcelainOutput(isRequest, e.Message);
                    }

                    serializer.Serialize(outputWriter, output);
                    outputWriter.WriteLine();
                    outputWriter.Flush();
                }
            };

            await device.Run(context, cts.Token).ConfigureAwait(false);
        }

        Log.Logger.Information("The monitor has ended");

        return 0;
    }
}
