using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace CommunicationInterfaces
{
  /// <summary>
  /// The Serial / COM version of the interface used for communication
  /// </summary>
  public class SerialInterface : ICommunicationInterface
  {
    private SerialPort _SerialSocket = new();
    private int _baudRate;
    private int _dataBits;
    private StopBits _stopBits;
    private Handshake _handshake;
    private Parity _parity;
    private int _receiveTimeout = 2000;
    private string _newLine = "\u000D";
    private readonly string _port;

    /// <summary>
    /// The constructor
    /// </summary>
    /// <param name="COMPort">
    /// The name of the COM Port that the device is attached to - e.g. "COM5"
    /// </param>
    /// <param name="baudrate">
    /// The baud rate that the device uses
    /// </param>
    /// <param name="dataBits"></param>
    /// <param name="stopBits"></param>
    /// <param name="handshake"></param>
    /// <param name="parity"></param>
    public SerialInterface(string COMPort, int baudrate = 115200, int dataBits = 8, StopBits stopBits = StopBits.OnePointFive, Handshake handshake = Handshake.None, Parity parity = Parity.None)
    {
      _baudRate = baudrate;
      _port = COMPort;
      _dataBits = dataBits;
      _stopBits = stopBits;
      _handshake = handshake;
      _parity = parity;
    }
  
    /// <summary>
    /// The constructor
    /// </summary>
    /// <param name="baudrate">
    /// The baud rate that the device uses
    /// </param>
    /// <param name="COMPort">
    /// The name of the COM Port that the device is attached to - e.g. "COM5"
    /// </param>
    /// <param name="dataBits"></param>
    /// <param name="stopBits"></param>
    /// <param name="handshake"></param>
    /// <param name="parity"></param>
    [Obsolete("Constructor is deprecated, please use SerialInterface(string COMPort, int baudrate) instead")]
    public SerialInterface(int baudrate, string COMPort, int dataBits = 8, StopBits stopBits = StopBits.OnePointFive, Handshake handshake = Handshake.None, Parity parity = Parity.None)
    {
      _baudRate = baudrate;
      _port = COMPort;
      _dataBits = dataBits;
      _stopBits = stopBits;
      _handshake = handshake;
      _parity = parity;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
      return $"{_port}";
    }

    /// <summary>
    /// The communication baud rate
    /// </summary>
    /// <exception cref="MetratecCommunicationException">
    /// Thrown in case baud rate setting did not work
    /// </exception>
    public int BaudRate
    {
      get => _baudRate;
      set
      {
        if (IsConnected)
          try
          {
            _SerialSocket.BaudRate = value;
          }
          catch
          {
            throw new MetratecCommunicationException("Couldn't set baud rate");
          }
        _baudRate = value;
      }
    }

    /// <summary>
    /// The communication receive timeout
    /// </summary>
    /// <exception cref="MetratecCommunicationException">
    /// Thrown in case the receive timeout can not be set
    /// </exception>
    public int ReceiveTimeout
    {
      get => _receiveTimeout;
      set
      {
        if (IsConnected)
          try
          {
            _SerialSocket.ReadTimeout = value;
          }
          catch
          {
            throw new MetratecCommunicationException("Couldn't set baud rate");
          }
        _receiveTimeout = value;
      }
    }

    /// <summary>
    /// The communication new line string
    /// </summary>
    public string NewlineString
    {
      get => _newLine;
      set
      {
        _newLine = value ?? throw new ArgumentNullException("newlineString", "No newline string was supplied");
        if (IsConnected)
          _SerialSocket.NewLine = value;
      }
    }

    /// <summary>
    /// Indicates whether data is available for reading
    /// </summary>
    /// <exception cref="MetratecCommunicationException">
    /// Thrown when the underlying function reports a broken stream
    /// </exception>
    public bool DataAvailable
    {
      get
      {
        if (IsConnected)
        {
          return _SerialSocket!.BytesToRead > 0;
        }
        else
        {
          throw new MetratecCommunicationException("Not connected");
        }
      }
    }

    /// <summary>
    /// The method to connect the communication interface
    /// </summary>
    /// <exception cref="MetratecCommunicationException">
    /// Thrown when the serial port could not be set up (e.g. wrong parameters, insufficient permissions, invalid port state).
    /// </exception>
    public void Connect()
    {
      if (IsConnected)
      {
        return;
      }
      _SerialSocket = new SerialPort()
      {
        BaudRate = _baudRate,
        DataBits = 8,
        DtrEnable = true,
        Handshake = Handshake.None,
        Parity = Parity.None,
        PortName = _port,
        ReadTimeout = _receiveTimeout,
        RtsEnable = false,
        StopBits = StopBits.Two,
        WriteTimeout = 400,
        ReadBufferSize = 1024,
        NewLine = _newLine,
      };
      try
      {
        _SerialSocket.Open();
      }
      catch (Exception e)
      {
        throw new MetratecCommunicationException($"Serial port could not be set up - {e.Message}", e);
      }
    }

    /// <summary>
    /// The method used to close the connection to the reader
    /// </summary>
    public void Disconnect()
    {
      if (null != _SerialSocket && _SerialSocket.IsOpen)
      {
        _SerialSocket.Close();
        _SerialSocket.Dispose();
      }

    }

    /// <summary>
    /// Returns if the connection is established
    /// </summary>
    /// <returns>True if the connection is established</returns>
    public bool IsConnected
    {
      get
      {
        if (null != _SerialSocket && _SerialSocket.IsOpen)
        {
          if (_SerialSocket!.BytesToWrite >= 0)
          {
            return true;
          }
          else
          {
            Disconnect();
          }
        }
        return false;
      }
    }

    /// <summary>
    /// Method to write a byte-array to the device (e.g. a converted binary file)
    /// </summary>
    /// <param name="data">
    /// The overall byte-array of data
    /// </param>
    /// <param name="offset">
    /// The starting address in the array
    /// </param>
    /// <param name="count">
    /// The number of characters to write
    /// </param>
    /// <exception cref="MetratecCommunicationException">
    /// Thrown when the data cannot be sent
    /// </exception>
    public void Send(byte[] data, int offset, int count)
    {
      try
      {
        _SerialSocket.DiscardOutBuffer();
        _SerialSocket.Write(data, offset, count);
        //                Console.WriteLine("Sending " + count.ToString() + " characters starting at " + offset.ToString());
      }
      catch (TimeoutException e)
      {
        throw new MetratecCommunicationException("Writing to port failed!", e);
      }
      catch (InvalidOperationException e)
      {
        throw new MetratecCommunicationException("Connection to device broken!", e);
      }
      catch (Exception e){
        throw new MetratecCommunicationException(e.Message, e);
      }
    }

    /// <summary>
    /// The method used to send data to the reader
    /// </summary>
    /// <param name="data">
    /// The data / command sent to the reader
    /// </param>
    /// <exception cref="MetratecCommunicationException">
    /// Thrown when the data cannot be sent
    /// </exception>
    public void SendCommand(string data)
    {
      try
      {
        _SerialSocket.DiscardOutBuffer();
        //_SerialSocket.WriteLine(data);
        _SerialSocket.Write(data + "\u000D");
      }
      catch (TimeoutException e)
      {
        throw new MetratecCommunicationException("Writing to port failed!", e);
      }
      catch (InvalidOperationException e)
      {
        throw new MetratecCommunicationException("Connection to device broken!", e);
      }
      catch (Exception e){
        throw new MetratecCommunicationException(e.Message, e);
      }
    }

    /// <summary>
    /// Method to read a stream of bytes from device
    /// </summary>
    /// <param name="count">
    /// Number of bytes to read
    /// </param>
    /// <returns>
    /// The bytes read
    /// </returns>
    /// <exception cref="MetratecCommunicationException">
    /// Thrown when the data cannot be read
    /// </exception>
    /// <exception cref="System.TimeoutException">
    /// If the data could not be read in time
    /// </exception>
    public byte[] Read(int count)
    {
      byte[] tempBuffer = new byte[count];
      for (int i = 0; i < count; i++)
      {
        try
        {
          tempBuffer[i] = (byte)_SerialSocket.ReadByte();
        }
        catch (TimeoutException e)
        {
          throw new TimeoutException("Reading from port timed out / was interrupted", e);
        }
        catch (InvalidOperationException e)
        {
          throw new MetratecCommunicationException("Connection to device lost", e);
        }
      }
      return tempBuffer;
    }

    /// <summary>
    /// The method used to synchronously read data from the reader
    /// </summary>
    /// <returns>
    /// The string read - without the trailing newline character(s)
    /// </returns>
    /// <exception cref="MetratecCommunicationException">
    /// Thrown when the data cannot be read
    /// </exception>
    /// <exception cref="System.TimeoutException">
    /// If the data could not be read in time
    /// </exception>
    public string ReadResponse()
    {
      try
      {
        return _SerialSocket.ReadLine();
      }
      catch (ObjectDisposedException e)
      {
        throw new MetratecCommunicationException(e.Message, e);
      }
    }

  }
}
