// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.IO.Ports;
namespace Serial;

public sealed class CommsInfo : IDisposable //-V3073
{ //-V3073
    private static readonly Dictionary<SerialPort, List<CommsInfo>> Ports = new Dictionary<SerialPort, List<CommsInfo>>();
    private SerialPort _serialPort;
    public const int DefaultBaudRate = 9600;

    /// <summary>Creates a new CommsInfo object using the settings from the provided SerialPort object.</summary>
    /// <exception cref="IOException">The specified port could not be found or opened.</exception>
    public CommsInfo(SerialPort port) : this(port.PortName, port.BaudRate) { }

    /// <summary>Creates a new CommsInfo object using the settings the PortName provided and the DefaultBaudRate.</summary>
    /// <exception cref="IOException">The specified port could not be found or opened.</exception>
    public CommsInfo(string portName) : this(portName, DefaultBaudRate) { }

    /// <summary>Creates a new CommsInfo object using the settings provided.</summary>
    /// <exception cref="IOException">The specified port could not be found or opened.</exception>
    public CommsInfo(string portName, int baudRate)
    {
        BaudRate = baudRate;
        PortName = portName;

        _serialPort = new SerialPort(portName, baudRate);
    }

    /// <summary>Gets the BaseStream object being used by the underlying SerialPort</summary>
    public Stream BaseStream { get => _serialPort.BaseStream; }
        
    /// <summary>Gets or sets the serial port being used by this object.</summary>
    /// <exception cref="IOException">The specified port could not be found or opened.</exception>
    public string PortName {
        get => _serialPort.PortName;
        set {
            lock (Ports)
            {
                if (!(_serialPort is null) && Ports.ContainsKey(_serialPort))
                {
                    Ports[_serialPort].Remove(this);
                    if (Ports[_serialPort].Count == 0)
                    {
                        Ports.Remove(_serialPort);
                        _serialPort.Dispose();
                    }
                }
                foreach (var kvPair in Ports)
                {
                    if (kvPair.Key.PortName == value)
                    {
                        _serialPort = kvPair.Key;
                        Ports[_serialPort].Add(this);
                        return;
                    }
                }
                //_serialPort = new SerialPort(value, BaudRate);
                if (_serialPort == null) return;
                    
                _serialPort.PortName = value;
                _serialPort.BaudRate = BaudRate;

                Ports[_serialPort] = new List<CommsInfo> { this };
            }
        }
    }

    /// <summary>Gets or sets the BaudRate being used by the underlying SerialPort</summary>
    /// <exception cref="ArgumentOutOfRangeException">The baud rate is less than zero, or greater than the maximum allowable baud rate for this machine.</exception>
    /// <exception cref="IOException">The port is in an invalid state -OR- Attempt to set the state of the port failed, eg: parameters are invalid.</exception>
    public int BaudRate { get; set; } = DefaultBaudRate;

    /// <summary>Discards data from the serial driver's receive buffer</summary>
    /// <exception cref="IOException">The port is in an invalid state -OR- Attempt to set the state of the port failed, eg: parameters are invalid.</exception>
    public void DiscardInBuffer()
    {
        lock (Ports)
        {
            if (_serialPort.IsOpen)
                _serialPort.DiscardInBuffer();
        }
    }

    /// <summary>Discards data from the serial driver's transmit buffer</summary>
    /// <exception cref="IOException">The port is in an invalid state -OR- Attempt to set the state of the port failed, eg: parameters are invalid.</exception>
    public void DiscardOutBuffer()
    {
        lock (Ports)
        {
            if (_serialPort.IsOpen)
                _serialPort.DiscardOutBuffer();
        }
    }

    /// <summary>Discards data from the serial driver's transmit and receive buffers</summary>
    /// <exception cref="IOException">The port is in an invalid state -OR- Attempt to set the state of the port failed, eg: parameters are invalid.</exception>
    public void DiscardBuffers()
    {
        DiscardInBuffer();
        DiscardOutBuffer();
    }

    /// <summary>Sets the baud rate of the serial port and opens the port if it is not already open.</summary>
    /// <exception cref="UnauthorizedAccessException">Access is denied to the port.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Port settings are out of range.</exception>
    /// <exception cref="ArgumentException">This type of serial port is not supported.</exception>
    /// <exception cref="IOException">The port is in an invalid state -OR- Attempt to set the state of the port failed, eg: parameters are invalid.</exception>
    public void SetConfig()
    {
        lock (Ports)
        {
            _serialPort.BaudRate = BaudRate;
            if (!_serialPort.IsOpen)
                _serialPort.Open();
        }
    }
    public void SetConfig(int ReadTimeout, int WriteTimeout)
    {
        SetConfig();
        _serialPort.ReadTimeout = ReadTimeout;
        _serialPort.WriteTimeout = WriteTimeout;
    }

    /// <summary>Closes the underlying serial port. The port is shared so care should be taken in multithreaded environments.</summary>
    /// <exception cref="IOException">The port is in an invalid state -OR- Attempt to set the state of the port failed, eg: parameters are invalid.</exception>
    public void ClosePort()
    {
        lock (Ports)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.DiscardOutBuffer();
                _serialPort.DiscardInBuffer();
                _serialPort.Close();
            }
        }
    }

    public void Dispose()
    {
        lock (Ports)
        {
            if (Ports.ContainsKey(_serialPort))
            {
                Ports[_serialPort].Remove(this);
                if (Ports[_serialPort].Count == 0)
                {
                    Ports.Remove(_serialPort);
                    _serialPort.Dispose();
                }
            }
            else
                _serialPort.Dispose();
        }
    }

    public override string ToString()
    {
        return $"{PortName} @{BaudRate}";
    }
}