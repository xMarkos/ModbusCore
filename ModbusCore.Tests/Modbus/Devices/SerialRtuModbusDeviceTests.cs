using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using ModbusCore.Messages;
using ModbusCore.Parsers;
using Xunit;

namespace ModbusCore.Devices
{
    public class SerialRtuModbusDeviceTests
    {
        [SkippableFact(typeof(FileNotFoundException), Timeout = 5000)]
        [Trait("Execution", "Manual")]
        public async Task SerialRtuModbusDevice_SentMessageIsReceived()
        {
            // Arrange
            var context = new MessagingContext();

            var parsers = new IMessageParser[] {
                new ReadRegistersRequestParser(),
            };

            IModbusMessage? actualMessage = null;
            ModbusMessageType actualMessageType = ModbusMessageType.Unknown;

            {
                using var sender = new SerialRtuModbusDevice(
                    new SerialRtuModbusDeviceConfiguration
                    {
                        BaudRate = 9600,
                        Parity = Parity.Even,
                        PortName = "COM4",
                    }, context, parsers, null);

                using var target = new SerialRtuModbusDevice(
                    new SerialRtuModbusDeviceConfiguration
                    {
                        BaudRate = 9600,
                        Parity = Parity.Even,
                        PortName = "COM3",
                    }, context, parsers, null);

                using CancellationTokenSource cts = new();

                target.MessageReceived += (object? sender, ModbusMessageReceivedEventArgs e) =>
                {
                    actualMessage = e.Message;
                    actualMessageType = e.Type;
                    cts.Cancel();
                };

                // Act
                Task tLoop = target.ReceiverLoop(cts.Token);

                ReadRegistersRequestMessage input = new(new byte[] { 0x11, 0x03, 0x00, 0x6B, 0x00, 0x03 });
                await sender.Send(input, default).ConfigureAwait(false);

                await tLoop.ConfigureAwait(false);
            }

            // Verify
            Assert.Equal(ModbusMessageType.Request, actualMessageType);
            var actual = Assert.IsType<ReadRegistersRequestMessage>(actualMessage);

            Assert.Equal(0x11, actual.Address);
            Assert.Equal(0x03, actual.Function);
            Assert.Equal(0x6B, actual.Register);
            Assert.Equal(0x03, actual.Count);
        }
    }
}
