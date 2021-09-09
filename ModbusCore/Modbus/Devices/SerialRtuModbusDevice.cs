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
    public class SerialRtuModbusDevice : ModbusDeviceBase
    {
        private readonly MessagingContext _context;
        private readonly ICollection<IMessageParser> _parsers;
        private readonly ILogger<SerialRtuModbusDevice>? _logger;

        private readonly SerialPort _port;
        private readonly SemaphoreSlim _sendLock = new(1);
        private readonly int _timer1_5;
        private readonly int _timer3_5;
        private long _lineIdleFrom;
        private State _state;

        public SerialRtuModbusDevice(SerialRtuModbusDeviceConfiguration configuration, MessagingContext context, ICollection<IMessageParser> parsers, ILogger<SerialRtuModbusDevice>? logger)
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
            return Task.Factory.StartNew(() =>
            {
                byte[] buffer = new byte[4096];
                int bufferIndex = 0;

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        void ReadData(int count)
                        {
                            while (bufferIndex < count)
                            {
                                stoppingToken.ThrowIfCancellationRequested();
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

                        ReadData(4);

                        _state = State.Receiving;

                        // Read information necessary to obtain the length of the frame
                        Transaction transaction = new(buffer[0], buffer[1]);
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

                        if (ModbusUtility.CalculateCrc16(frame[..^2]) != ModbusUtility.ReadUInt16(frame[^2..]))
                            throw new IOException("Checksum of the received frame is not valid");

                        IModbusMessage message = parser.Parse(frame, messageType);

                        OnMessageReceived(message, messageType);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger?.LogInformation("Cancellation was requested -> stopping receiver");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Unhandled receiver exception");
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

        public override async Task Send(ReadOnlyMemory<byte> message, CancellationToken cancellationToken)
        {
            // Prepare the frame
            byte[] frame = new byte[message.Length + 2];
            message.CopyTo(frame.AsMemory(0, message.Length));

            ModbusUtility.Write(frame.AsSpan(^2..), ModbusUtility.CalculateCrc16(message.Span));

            await _sendLock.WaitAsync(1, cancellationToken).ConfigureAwait(false);
            try
            {
                {
                    SpinWait w = new();

                    while (true)
                    {
                        long mustWait = _lineIdleFrom - Stopwatch.GetTimestamp();
                        if (mustWait < 0 && _state == State.Idle)
                            break;

                        cancellationToken.ThrowIfCancellationRequested();

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

                _port.Write(frame, 0, frame.Length);

                _lineIdleFrom = Stopwatch.GetTimestamp() + _timer3_5;
                _state = State.Idle;
            }
            finally
            {
                _sendLock.Release();
            }
        }

        private enum State
        {
            Idle,
            Sending,
            Receiving,
        }
    }
}
