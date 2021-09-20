﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore.Devices
{
    public abstract class ModbusDeviceBase : IModbusDevice
    {
        private readonly ConcurrentExclusiveSchedulerPair _scheduler = new();

        public event EventHandler<ModbusMessageReceivedEventArgs>? MessageReceived;

        public abstract Task ReceiverLoop(CancellationToken stoppingToken);

        public abstract Task Send(IModbusMessage message, CancellationToken cancellationToken);

        protected virtual void OnMessageReceived(IModbusMessage message, ModbusMessageType type, ModbusMessagePriority priority = ModbusMessagePriority.Normal)
        {
            // Run asynchronously to not block the reader thread. Scheduler is used to prevent race condition causing
            //  the events to be delivered out of order.
            Task.Factory.StartNew(() =>
            {
                MessageReceived?.Invoke(this, new(message, type, priority));
            }, default, TaskCreationOptions.HideScheduler | TaskCreationOptions.DenyChildAttach, _scheduler.ExclusiveScheduler);
        }
    }
}
