namespace SX1268Library;

public enum OperationMode
{
    /// <summary>
    /// Module is in data transmission/reception mode.
    /// </summary>
    Transmission,
    
    /// <summary>
    /// Module is in configuration mode.
    /// </summary>
    Configuration,
    
    /// <summary>
    /// Module is in wake-on-radio mode, which the module sleeps until a message is received.
    /// </summary>
    WakeOnRadio,
    
    /// <summary>
    /// Module is in deep sleep mode.
    /// </summary>
    DeepSleep
}

public class LoraMessageReceivedEventArgs(byte[] payload, int? packetRssi = null) : EventArgs
{
    /// <summary>
    /// Received payload.
    /// </summary>
    public byte[] Payload { get; } = payload;
    
    /// <summary>
    /// RSSI value of the packet, if enabled.
    /// </summary>
    public int? PacketRssi { get; } = packetRssi;
}

public class SX1268 : IDisposable
{
    private int M0Pin = 22;
    private int M1Pin = 27;

    private static readonly byte[] VersionRequestMagic = [0xC1, 0x80, 0x07];
    private static readonly byte[] GetParametersMagic  = [0xC1, 0x00, 0x09];

    /// <summary>
    /// Configuration of the module.
    /// </summary>
    public SX1268Configuration Configuration { get; } = new();

    /// <summary>
    /// Raises when a message is received. The event arguments contain the payload and, if enabled, the packet RSSI value.
    /// </summary>
    public event EventHandler<LoraMessageReceivedEventArgs>? DataReceived;

    private readonly GpioController _gpioController;
    private readonly SerialPort _serialPort;
    private readonly SemaphoreWrapper _semaphore;

    private OperationMode? _lastMode;
    private CancellationTokenSource? _cts;
    private Task? _receiveTask;

    /// <summary>
    /// Creates SX1268 instance with default GPIO pins for Raspberry Pi 4.
    /// </summary>
    /// <param name="portName">Name of the port.</param>
    /// <returns>A <see cref="SX1268"/> instance.</returns>
    public static SX1268 CreateRaspberryPi(string portName) => new(portName, 22, 27);

    /// <summary>
    /// Creates SX1268 instance with default GPIO pins for Jetson Orin/Nano.
    /// </summary>
    /// <param name="portName">Name of the port.</param>
    /// <returns>A <see cref="SX1268"/> instance.</returns>
    public static SX1268 CreateJetsonOrinNano(string portName) => new(portName, 15, 13);

    /// <summary>
    /// Creates SX1268 instance with alternative GPIO pins for Jetson Orin/Nano.
    /// </summary>
    /// <param name="portName">Name of the port.</param>
    /// <returns>A <see cref="SX1268"/> instance.</returns>
    public static SX1268 CreateJetsonAlternative(string portName) => new(portName, 85, 122);

    /// <summary>
    /// Creates an instance of SX126x module controller.
    /// </summary>
    /// <param name="portName">Name of the port which the device is exposed to.</param>
    /// <param name="m0Pin">GPIO pin which M0 is connected to.</param>
    /// <param name="m1Pin">GPIO pin which M1 is connected to.</param>
    public SX1268(string portName, int m0Pin, int m1Pin)
    {
        M0Pin = m0Pin;
        M1Pin = m1Pin;

        _semaphore = new SemaphoreWrapper(1, 1);
        _gpioController = new GpioController();
        _gpioController.OpenPin(M0Pin, PinMode.Output);
        _gpioController.OpenPin(M1Pin, PinMode.Output);

        SetOperationMode(OperationMode.DeepSleep);

        _serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One)
        {
            ReadTimeout = 1000,
            WriteTimeout = 1000
        };

        _serialPort.Open();
        _serialPort.DiscardInBuffer();

        GetModuleVersion();
        GetConfiguration();
    }

    /// <summary>
    /// Creates an instance of SX126x module controller.
    /// </summary>
    /// <param name="portName">Name of the port which the device is exposed to.</param>
    /// <param name="m0Pin">GPIO pin which M0 is connected to.</param>
    /// <param name="m1Pin">GPIO pin which M1 is connected to.</param>
    /// <param name="configuration">Initial module configuration.</param>
    public SX1268(string portName, int m0Pin, int m1Pin, SX1268Configuration configuration) : this(portName, m0Pin, m1Pin)
    {
        SetConfiguration(configuration);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Port Name: {_serialPort.PortName}");
        sb.AppendLine($"Operation Mode: {_lastMode}");
        sb.Append(Configuration);

        return sb.ToString();
    }

    /// <summary>
    /// Changes the operation mode of the module.
    /// </summary>
    /// <param name="mode">New operation mode.</param>
    /// <exception cref="NotImplementedException">Thrown when the given mode is not recognized.</exception>
    public void SetOperationMode(OperationMode mode)
    {
        if (_lastMode == mode)
        {
            return;
        }

        _lastMode = mode;

        switch (mode)
        {
            case OperationMode.Transmission:
                _gpioController.Write(M0Pin, PinValue.Low);
                _gpioController.Write(M1Pin, PinValue.Low);
                break;

            case OperationMode.Configuration:
                _gpioController.Write(M0Pin, PinValue.Low);
                _gpioController.Write(M1Pin, PinValue.High);
                break;

            case OperationMode.WakeOnRadio:
                _gpioController.Write(M0Pin, PinValue.High);
                _gpioController.Write(M1Pin, PinValue.Low);
                break;

            case OperationMode.DeepSleep:
                _gpioController.Write(M0Pin, PinValue.High);
                _gpioController.Write(M1Pin, PinValue.High);
                break;

            default: 
                throw new NotImplementedException("Unrecognized operation mode.");
        }

        Thread.Sleep(100);
    }

    /// <summary>
    /// Reads the module version.
    /// </summary>
    /// <exception cref="TimeoutException">Module failed to respond in 5 seconds.</exception>
    /// <exception cref="NotImplementedException">Module sent unrecognized data.</exception>
    public void GetModuleVersion()
    {
        using var _ = _semaphore.LockOne();

        SetOperationMode(OperationMode.Configuration);

        try
        {
            _serialPort.DiscardInBuffer();
            _serialPort.Write(VersionRequestMagic, 0, VersionRequestMagic.Length);
            Thread.Sleep(200);

            var counter = 0;
            while (_serialPort.BytesToRead < 10)
            {
                if (++counter == 50)
                {
                    throw new TimeoutException("Module version could not be fetched after 5 seconds.");
                }

                Thread.Sleep(100);
            }

            var bytes = new byte[10];
            _serialPort.Read(bytes, 0, bytes.Length);

            if (bytes is [0xC1, 0x80, 0x07, ..])
            {
                Configuration.DetermineModuleType(
                    (ushort)((bytes[3] << 8) | bytes[4]),
                    bytes[5],
                    bytes[7],
                    bytes[6]
                );
            }
            else
            {
                throw new NotImplementedException("Invalid byte sequence received while reading version information.");
            }
        }
        finally
        {
            SetOperationMode(OperationMode.Transmission);
        }
    }

    /// <summary>
    /// Reads module configuration.
    /// </summary>
    /// <exception cref="TimeoutException">Module failed to respond in 5 seconds.</exception>
    /// <exception cref="NotImplementedException">Module sent unrecognized data.</exception>
    public void GetConfiguration()
    {
        using var _ = _semaphore.LockOne();

        SetOperationMode(OperationMode.Configuration);

        try
        {
            _serialPort.DiscardInBuffer();
            _serialPort.Write(GetParametersMagic, 0, GetParametersMagic.Length);
            Thread.Sleep(200);

            var counter = 0;
            while (_serialPort.BytesToRead < 12)
            {
                if (++counter == 50)
                {
                    throw new TimeoutException("Module configuration could not be fetched after 5 seconds.");
                }

                Thread.Sleep(100);
            }

            var bytes = new byte[12];
            _serialPort.Read(bytes, 0, bytes.Length);

            if (bytes is [0xC1, 0x00, 0x09, ..])
            {
                Configuration.FromByteArray(bytes);
            }
            else
            {
                throw new NotImplementedException("Invalid byte sequence received while reading module configuration.");
            }
        }
        finally
        {
            SetOperationMode(OperationMode.Transmission);
        }
    }

    /// <summary>
    /// Changes the module configuration.
    /// </summary>
    /// <param name="configuration">New configuration. Unspecified properties are unaffected.</param>
    public void SetConfiguration(SX1268Configuration configuration)
    {
        using var _ = _semaphore.LockOne();

        SetOperationMode(OperationMode.Configuration);

        try
        {
            Configuration.ApplyChanges(configuration);

            var bytes = Configuration.ToByteArray();

            _serialPort.DiscardInBuffer();

            for (var i = 0; i < 3; i++)
            {
                _serialPort.Write(bytes, 0, bytes.Length);
                Thread.Sleep(200);

                if (_serialPort.BytesToRead > 0)
                {
                    var response = new byte[_serialPort.BytesToRead];
                    _serialPort.Read(response, 0, response.Length);

                    if (response.Length > 0 && response[0] == 0xC1)
                        break;
                }
                else
                {
                    _serialPort.DiscardInBuffer();
                    Thread.Sleep(200);
                }
            }
        }
        finally
        {
            SetOperationMode(OperationMode.Transmission);
        }
    }

    /// <summary>
    /// Writes given payload to the module.
    /// </summary>
    /// <param name="payload"></param>
    public void SendData(byte[] payload)
    {
        using var _ = _semaphore.LockOne();

        SetOperationMode(OperationMode.Transmission); // Ensure we're transmitting.

        _serialPort.Write(payload, 0, payload.Length);
    }

    /// <summary>
    /// Reads the RSSI value of the channel.
    /// </summary>
    /// <returns></returns>
    public int? ReadChannelRssi()
    {
        using var _ = _semaphore.LockOne();

        SetOperationMode(OperationMode.Transmission);

        _serialPort.DiscardInBuffer();
        _serialPort.Write([0xC0, 0xC1, 0xC2, 0xC3, 0x00, 0x02], 0, 6);
        Thread.Sleep(500);

        while (_serialPort.BytesToRead == 0)
        {
            Thread.Sleep(100);
        }

        var resp = new byte[_serialPort.BytesToRead];
        _serialPort.Read(resp, 0, resp.Length);

        if (resp is [0xC1, 0x00, 0x02, _, ..])
        {
            return 256 - resp[3];
        }

        return null;
    }

    /// <summary>
    /// Starts the listener for incoming messages and event handling.
    /// </summary>
    public void StartListening()
    {
        if (_receiveTask is { IsCompleted: false })
            return;

        _cts = new CancellationTokenSource();
        _receiveTask = Task.Run((Func<Task>)(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    if (_serialPort.BytesToRead > 0)
                    {
                        await Task.Delay(500);

                        byte[] buffer;
                        using (_semaphore.LockOne())
                        {
                            buffer = new byte[_serialPort.BytesToRead];
                            _ = _serialPort.Read(buffer, 0, buffer.Length);
                        }

                        int bufferLength = buffer.Length;
                        if (Configuration.PacketRssiEnabled.HasValue && Configuration.PacketRssiEnabled.Value)
                            bufferLength -= 1;
                        
                        var payload = new byte[bufferLength];
                        Array.Copy(buffer, payload, bufferLength);

                        int? rssi = null;
                        if (Configuration.PacketRssiEnabled.HasValue && Configuration.PacketRssiEnabled.Value)
                            rssi = 256 - buffer[^1];

                        DataReceived?.Invoke(this, new LoraMessageReceivedEventArgs(
                            payload, rssi));

                        if (Configuration.PacketRssiEnabled.HasValue && Configuration.PacketRssiEnabled.Value)
                            ReadChannelRssi();
                    }

                    await Task.Yield();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while reading data: {ex.Message}");
                }
            }
        }), _cts.Token);
    }

    /// <summary>
    /// Stops incoming message listener.
    /// </summary>
    public void StopListening()
    {
        _cts?.Cancel();

        try
        {
            _receiveTask?.Wait();
        }
        catch (AggregateException agg)
        {
            // Ignore all OperationCanceledException and TaskCanceledException inside AggregateException
            agg.Handle(ex =>
                ex is OperationCanceledException or TaskCanceledException
            );
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }

        _cts?.Dispose();
        _receiveTask?.Dispose();

        _cts = null;
        _receiveTask = null;
    }

    public void Dispose()
    {
        StopListening();
        _serialPort.Close();
        _serialPort.Dispose();
        _gpioController.Dispose();
    }

    /// <summary>
    /// Stops listener, sets module configuration and restarts listener.
    /// </summary>
    /// <param name="configuration">New configuration. Unchanged properties are unaffected.</param>
    public void SafeSetConfiguration(SX1268Configuration configuration)
    {
        StopListening();
        SetConfiguration(configuration);
        StartListening();
    }
}