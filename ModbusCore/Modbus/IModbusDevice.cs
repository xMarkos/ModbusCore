using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore
{
    public interface IModbusDevice
    {
        event EventHandler<ModbusMessageReceivedEventArgs>? MessageReceived;

        Task Send(IModbusMessage message, CancellationToken cancellationToken);
        Task ReceiverLoop(CancellationToken stoppingToken);
    }
}
