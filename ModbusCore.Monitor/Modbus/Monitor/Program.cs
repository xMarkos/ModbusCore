using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
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
        public static async Task Main()
        {
            string portName = "COM10";
            int baudRate = 9600;
            Parity parity = Parity.None;

            // TODO command line config

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

            var context = new MessagingContext();

            IMessageParser[] parsers = {
                new ExceptionMessageParser(),
                new ReadCoilsResponseMessageParser(),
                new ReadRegistersMessageParser(),
                new ReadExceptionStatusMessageParser(),
                new WriteSingleValueMessageParser(),
            };

            using var device = new SerialRtuModbusDevice(
                new()
                {
                    PortName = portName,
                    BaudRate = baudRate,
                    Parity = parity,
                },
                context,
                parsers,
                loggerFactory.CreateLogger<SerialRtuModbusDevice>());

            device.MessageReceived += Device_MessageReceived;

            await device.ReceiverLoop(cts.Token).ConfigureAwait(false);

            Log.Logger.Information("The monitor has ended");
        }

        private static void Device_MessageReceived(object? sender, ModbusMessageReceivedEventArgs e)
        {
            Log.Logger.Information("Received {Type} {Message}", e.Type, e.Message);
        }
    }
}
