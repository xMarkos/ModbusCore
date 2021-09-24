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

namespace ModbusCore.Monitor
{
    public class Program
    {
        private class Arguments
        {
            [Value(0, Required = true, MetaName = "Port", HelpText = "The name of the device used to listen for messages.")]
            public string? Port { get; set; }

            [Option('r', "baudrate", Default = 9600, HelpText = "Baud rate (speed) of the serial port.")]
            public int BaudRate { get; set; }

            [Option('p', "parity", Default = Parity.None, HelpText = "Parity of the serial port (None, Odd, Even, Mark, Space).")]
            public Parity Parity { get; set; }

            [Option('s', "stop-bits", HelpText = "(Default: infer from parity) Stop bits of the serial port (None, One, Two, OnePointFive).")]
            public StopBits? StopBits { get; set; }

            [Option('o', "output", HelpText = "File path to write JSON formatted messages (1 per line). Value \"-\" will write messages to console.")]
            public string? OutputPath { get; set; }

            [Option('e', "stderr", HelpText = "Writes messages to standard error instead of standard output.", SetName = "logging-1")]
            public bool WriteToStdErr { get; set; }

            [Option('q', "quiet", HelpText = "Suppresses message logging.", SetName = "logging-2")]
            public bool Quiet { get; set; }
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

                config.MinimumLevel.Debug();

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

            using TextWriter? output =
                options.OutputPath switch
                {
                    null => null,
                    "-" => Console.Out,
                    _ => new StreamWriter(File.Open(options.OutputPath, FileMode.Append, FileAccess.Write)),
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

            using var context = new ExpiringMessagingContext(loggerFactory);

            using SerialRtuModbusDevice? device = OpenDevice();

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
                        context,
                        ParserCollection.Default,
                        loggerFactory.CreateLogger<SerialRtuModbusDevice>());
                }
                catch (IOException ex)
                {
                    Log.Logger.Error(ex, "Could not open the port");
                    return null;
                }
            }

            if (device == null)
                return 2;

            using (device)
            {
                int messageWidth = 0;
                string messageTemplate = "Received {Type,-8} {Class} {Message}";

                device.MessageReceived += (object? sender, ModbusMessageReceivedEventArgs e) =>
                {
                    if (e.Type == ModbusMessageType.Request)
                    {
                        context.AddTransaction(Transaction.From(e.Message));
                    }
                    else if (e.Type == ModbusMessageType.Response)
                    {
                        context.RemoveTransaction(Transaction.From(e.Message));
                    }

                    string[] parts = e.Message.ToString()!.Split(new[] { ' ' }, 2);
                    if (messageWidth < parts[0].Length)
                    {
                        messageWidth = parts[0].Length;
                        messageTemplate = $"Received {{Type,-8}} {{Class,-{messageWidth}}} {{Message}}";
                    }

                    Log.Logger.Information(messageTemplate, e.Type, parts[0], parts[1]);

                    if (output != null)
                    {
                        serializer.Serialize(output, new PorcelainOutput(e.Type == ModbusMessageType.Request, e.Message));
                        output.WriteLine();
                    }
                };

                await device.ReceiverLoop(cts.Token).ConfigureAwait(false);
            }

            Log.Logger.Information("The monitor has ended");

            return 0;
        }
    }
}
