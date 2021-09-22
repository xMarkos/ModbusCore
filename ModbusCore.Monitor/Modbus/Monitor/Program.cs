using System;
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

            var context = new MessagingContext();

            IMessageParser[] parsers = {
                new ExceptionMessageParser(),
                new ReadCoilsResponseMessageParser(),
                new ReadRegistersMessageParser(),
                new ReadExceptionStatusMessageParser(),
                new WriteSingleValueMessageParser(),
            };

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
                    parsers,
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
                device.MessageReceived += (object? sender, ModbusMessageReceivedEventArgs e) =>
                {
                    Log.Logger.Information("Received {Type} {Message}", e.Type, e.Message);
                };

                await device.ReceiverLoop(cts.Token).ConfigureAwait(false);
            }

            Log.Logger.Information("The monitor has ended");
        }
    }
}
