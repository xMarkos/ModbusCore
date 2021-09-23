using System;
using System.Linq;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using ModbusCore.Devices;
using ModbusCore.Parsers;
using Microsoft.Extensions.Logging;
using Serilog;
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
        }

        public static async Task Main(string[] args)
        {
            Arguments? options =
                Parser.Default.ParseArguments<Arguments>(args)
                    .MapResult(x => x, x => null!);

            if (options is null)
                return;

            Log.Logger =
                new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                .CreateLogger();

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

            SerialRtuModbusDevice device;
            try
            {
#pragma warning disable CA2000
                device = new SerialRtuModbusDevice(
                    new()
                    {
                        PortName = options.Port,
                        BaudRate = options.BaudRate,
                        Parity = options.Parity,
                    },
                    context,
                    ParserCollection.Default,
                    loggerFactory.CreateLogger<SerialRtuModbusDevice>());
#pragma warning restore CA2000
            }
            catch (IOException ex)
            {
                Log.Logger.Error(ex, "Could not open the port");
                return;
            }

            using (device)
            {
                int messageWidth = 0;

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
                        messageWidth = parts[0].Length;

                    Log.Logger.Information($"Received {{Type,-8}} {{Class,-{messageWidth}}} {{Message}}", e.Type, parts[0], parts[1]);
                };

                await device.ReceiverLoop(cts.Token).ConfigureAwait(false);
            }

            Log.Logger.Information("The monitor has ended");
        }
    }
}
