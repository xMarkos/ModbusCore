using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ModbusCore.Devices
{
    public class SerialRtuModbusDevice : ModbusDeviceBase, IDisposable
    {
        private readonly IMessagingContext _context;
        private readonly ICollection<IMessageParser> _parsers;
        private readonly ILogger<SerialRtuModbusDevice>? _logger;

        private readonly SerialPort _port;
        private readonly SemaphoreSlim _sendLock = new(1);
        private readonly CancellationTokenSource _cts = new();
        private readonly byte[] _sendBuffer = new byte[256];
        private readonly int _timer1_5;
        private readonly int _timer3_5;
        private long _lineIdleFrom;
        private int _state;
        private bool _disposed;

        public SerialRtuModbusDevice(SerialRtuModbusDeviceConfiguration configuration, IMessagingContext context, ICollection<IMessageParser> parsers, ILogger<SerialRtuModbusDevice>? logger)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            _context = context ?? throw new ArgumentNullException(nameof(context));
            _parsers = parsers ?? throw new ArgumentNullException(nameof(parsers));
            _logger = logger;

            //const int baudrate = 300,600,115200;
            int baudrate = configuration.BaudRate;
            Parity parity = configuration.Parity;
            StopBits stopBits = parity == Parity.None ? StopBits.Two : StopBits.One;

            (_timer1_5, _timer3_5) = GetSilenceTimers(baudrate);

            _port = new SerialPort(configuration.PortName, baudrate, parity, 8, stopBits)
            {
                // We need the read timeouts to ensure that we don't block on incomplete messages
                // Note: the timing is tricky, especially with UART device, so the exact timer 3.5 chars can't be used reliably
                //  therefore we wait a little bit longer.
                ReadTimeout = (int)Math.Ceiling(_timer3_5 * 3 / 10000.0),
            };

            _state = State.Idle;

            _port.Open();
        }

        ~SerialRtuModbusDevice()
            => Dispose(false);

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;

            if (disposing)
            {
                _port.Dispose();
                _sendLock.Dispose();
            }

            _cts.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static (int T1_5, int T3_5) GetSilenceTimers(int baudrate)
        {
            // Times are returned in ticks; 1 tick = 10000ms = 10us
            if (baudrate > 19200)
            {
                // The specification says the time is fixed for faster rates
                return (7500, 17500);
            }
            else
            {
                return (150_000_000 / baudrate, 350_000_000 / baudrate);
            }
        }

        public override Task ReceiverLoop(CancellationToken stoppingToken)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SerialRtuModbusDevice));

            return Task.Factory.StartNew(() =>
            {
                byte[] buffer = new byte[4096];
                int bufferIndex = 0;

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, stoppingToken);

                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        void ReadData(int count)
                        {
                            while (bufferIndex < count)
                            {
                                cts.Token.ThrowIfCancellationRequested();
                                bufferIndex += _port.Read(buffer, bufferIndex, count - bufferIndex);
                            }
                        }

                        // Read only header of the frame
                        while (true)
                        {
                            try
                            {
                                ReadData(1);
                                break;
                            }
                            catch (TimeoutException)
                            {
                                // It is acceptable to timeout for the 1st byte
                            }
                        }

                        _state = State.Receiving;

                        ReadData(4);

                        // Read information necessary to obtain the length of the frame
                        Transaction transaction = new(buffer[0], (ModbusFunctionCode)buffer[1]);
                        ModbusMessageType messageType = _context.IsRequestActive(transaction) ? ModbusMessageType.Response : ModbusMessageType.Request;

                        IMessageParser parser = _parsers.GetParser(buffer.AsSpan(..bufferIndex), messageType);

                        // Some messages types might need more data to determine the length
                        int frameLength;
                        while (!parser.TryGetFrameLength(buffer.AsSpan(..bufferIndex), messageType, out frameLength))
                            ReadData(frameLength);

                        // Read the remaining frame bytes +2 bytes for CRC16
                        ReadData(frameLength + 2);

                        Span<byte> frame = buffer.AsSpan(..bufferIndex);

                        // The frame is complete now
                        _lineIdleFrom = Stopwatch.GetTimestamp() + _timer3_5;

                        ReadOnlySpan<byte> pdu = frame[..^2];
                        if (ModbusUtility.CalculateCrc16(pdu) != ModbusUtility.ReadUInt16(frame[^2..]))
                            throw new IOException("Checksum of the received frame is not valid");

                        IModbusMessage message = parser.Parse(pdu, messageType);

                        OnMessageReceived(message, messageType);
                    }
                    catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
                    {
                        _logger?.LogInformation("Cancellation was requested -> stopping receiver");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Unhandled receiver exception -> discarding buffer and attempting to recover");
                        _port.DiscardInBuffer();
                    }
                    finally
                    {
                        bufferIndex = 0;
                        _state = State.Idle;
                    }
                }
            }, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private static void BuildFrame(IModbusMessage message, Span<byte> buffer, out int length)
        {
            // [Unit][PDU][CRC16]
            // Unit is already included in the message, CRC is 2 bytes

            if (!message.TryWriteTo(buffer, out length) || length > 254)
                throw new ArgumentException("The message exceeds the maximum allowed length of 256 bytes", nameof(message));

            ModbusUtility.Write(buffer[length..], ModbusUtility.CalculateCrc16(buffer[..length]));
            length += 2;
        }

        public override async Task Send(IModbusMessage message, CancellationToken cancellationToken)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            if (_disposed)
                throw new ObjectDisposedException(nameof(SerialRtuModbusDevice));

            await _sendLock.WaitAsync(1, cancellationToken).ConfigureAwait(false);
            try
            {
                // It is enough to use shared buffer of fixed length 256, because we are in a lock,
                //  and the spec does not allow longer frames than 256 bytes.
                BuildFrame(message, _sendBuffer, out int length);

                {
                    SpinWait w = new();

                    long mustWait;
                    while ((mustWait = _lineIdleFrom - Stopwatch.GetTimestamp()) > 0 || _state != State.Idle)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // Only spin if we need to wait less than 1ms
                        if (mustWait > 10_000)
                        {
                            await Task.Delay((int)(mustWait / 10_000), cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            w.SpinOnce();
                        }
                    }
                }

                _state = State.Sending;

                _port.Write(_sendBuffer, 0, length);
            }
            finally
            {
                _lineIdleFrom = Stopwatch.GetTimestamp() + _timer3_5;
                if (Interlocked.CompareExchange(ref _state, State.Idle, State.Sending) == State.Receiving)
                    _logger?.LogWarning("Collision detected while sending a frame");

                _sendLock.Release();
            }
        }

        private class State
        {
            public const int Idle = 0;
            public const int Sending = 1;
            public const int Receiving = 2;
        }
    }
}
