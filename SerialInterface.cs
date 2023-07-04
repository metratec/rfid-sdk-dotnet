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
    private int _receiveTimeout = 2000;
    private string _newLine = "\u000D";
    private readonly string _port;
    /// <summary>
    /// The constructor
    /// </summary>
    /// <param name="baudrate">
    /// The baud rate that the device uses
    /// </param>
    /// <param name="COMPort">
    /// The name of the COM Port that the device is attached to - e.g. "COM5"
    /// </param>
    public SerialInterface(int baudrate, string COMPort)
    {
      _baudRate = baudrate;
      _port = COMPort;
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
    /// <exception cref="T:System.InvalidOperationException">
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
            throw new InvalidOperationException("Couldn't set baud rate");
          }
        _baudRate = value;
      }
    }

    /// <summary>
    /// The communication receive timeout
    /// </summary>
    /// <exception cref="T:System.InvalidOperationException">
    /// Thrown in case baud rate setting did not work
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
            throw new InvalidOperationException("Couldn't set baud rate");
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
    /// <exception cref="T:System.ObjectDisposedException">
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
          throw new ObjectDisposedException("Not connected");
        }
      }
    }

    /// <summary>
    /// The method to connect the communication interface
    /// </summary>
    /// <exception cref="T:System.InvalidOperationException">
    /// Thrown when the  serial port could not be set up (e.g. wrong parameters, insufficient permissions, invalid port state).
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
        DtrEnable = false,
        Handshake = Handshake.None,
        Parity = Parity.None,
        PortName = _port,
        ReadTimeout = _receiveTimeout,
        RtsEnable = false,
        StopBits = StopBits.One,
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
        throw new InvalidOperationException("Serial port could not be set up", e);
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
    public void Send(byte[] data, int offset, int count)
    {
      if (data == null)
      {
        ArgumentNullException _exc = new(nameof(data), "No data array was passed to send");
        throw _exc;
      }
      if ((offset < 0) || (offset > data.Length))
      {
        ArgumentNullException _exc = new(nameof(offset), "Offset cannot be less than 0 or larger than the length of DataBytes");
        throw _exc;
      }
      if ((count < 0) || (count + offset > data.Length))
      {
        ArgumentNullException _exc = new(nameof(count), "Count cannot be less than zero ot larger than the length of DataBytes when counting from offset");
        throw _exc;
      }
      try
      {
        _SerialSocket.DiscardOutBuffer();
        _SerialSocket.Write(data, offset, count);
        //                Console.WriteLine("Sending " + count.ToString() + " characters starting at " + offset.ToString());
      }
      catch (TimeoutException e)
      {
        throw new InvalidOperationException("Writing to port failed!", e);
      }
      catch (InvalidOperationException e)
      {
        throw new ObjectDisposedException("Connection to device broken!", e);
      }
    }

    /// <summary>
    /// The method used to send data to the reader
    /// </summary>
    /// <param name="data">
    /// The data / command sent to the reader
    /// </param>
    /// <exception cref="T:System.ArgumentNullException">
    /// Thrown when the specified <paramref name="data"/>  is  <see langword="null"/>.
    /// </exception>
    /// <exception cref="T:System.InvalidOperationException">
    /// Thrown when an exception occurs when trying to access the port (e.g. port closed, timeout).
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// Thrown when the underlying function reports a broken stream
    /// </exception>
    public void SendCommand(string data)
    {
      if (data == null)
      {
        ArgumentNullException _exc = new(nameof(data), "No string was given to send");
        throw _exc;
      }
      try
      {
        _SerialSocket.DiscardOutBuffer();
        //_SerialSocket.WriteLine(data);
        _SerialSocket.Write(data + "\u000D");
      }
      catch (TimeoutException e)
      {
        throw new InvalidOperationException("Writing to port failed!", e);
      }
      catch (InvalidOperationException e)
      {
        throw new ObjectDisposedException("Connection to device broken!", e);
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
          throw new ObjectDisposedException("Connection to device lost", e);
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
    /// <exception cref="T:System.TimeoutException">
    /// Thrown when no answer is received for more than 400ms.
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// Thrown when the underlying function reports a broken stream
    /// </exception>
    public string ReadResponse()
    {
      try
      {
        return _SerialSocket.ReadLine();
      }
      catch (ObjectDisposedException e)
      {
        Disconnect();
        throw e;
      }
    }


  }
}
