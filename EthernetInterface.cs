using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace CommunicationInterfaces
{
  /// <summary>
  /// The Ethernet version of the interface used for communication
  /// </summary>
  public class EthernetInterface : ICommunicationInterface
  {
    /// <summary>
    /// String variable to remember newline character
    /// </summary>
    private string _newlineString = "\u000D";
    private readonly IPAddress _ipAddress;
    private readonly int _port;
    /// <summary>
    /// The socket and network stream the reader is connected to
    /// </summary>
    private Socket? _clientSocket;
    private NetworkStream? _socketStream;
    private bool _isConnected = false;

    /// <summary>
    /// The constructor
    /// </summary>
    /// <param name="address">
    /// The IP address of the reader
    /// </param>
    /// <param name="port">
    /// The TCP port the reader is communicating on (default is 10,001)
    /// </param>
    /// <exception cref="T:System.ArgumentNullException">
    /// Thrown when the specified <paramref name="address"/>  is  <see langword="null"/>.
    /// </exception>
    /// <exception cref="T:System.ArgumentException">
    /// Thrown when the specified <paramref name="address"/> cannot be parsed into an IP address.
    /// </exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// Thrown when the specified <paramref name="port"/> is outside of the range of valid TCP ports (1-65535).
    /// </exception>
    /// <exception cref="T:System.InvalidOperationException">
    /// Thrown when the socket could not be connected to the IPEndpoint (socket closed or insufficient permissions).
    /// </exception>
    public EthernetInterface(string address, int port)
    {
      if (address == null)
      {
        ArgumentNullException _exception = new(nameof(address), "Setting up IPBased connection to the device failed - no IP address given!");
        throw _exception;
      }

      if (!IPAddress.TryParse(address, out _ipAddress!))
      {
        ArgumentException _exception = new("Setting up IPBased connection to the device failed (wrong IP address / TCP port given?)!");
        throw _exception;
      }
      if ((port < 1) || (port > 65535))
      {
        throw new ArgumentOutOfRangeException("TCPPort", "The TCP Port number has to be between 1 and 65535!");
      }
      this._port = port;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
      return $"{_ipAddress}:{_port}";
    }

    /// <summary>
    /// The communication baud rate
    /// </summary>
    /// <exception cref="T:System.InvalidOperationException">
    /// Thrown in case baud rate setting did not work
    /// </exception>
    public int BaudRate
    {
      get => throw new InvalidOperationException("Bau rate setting not possible for Ethernet connections");
      set => throw new InvalidOperationException("Bau rate setting not possible for Ethernet connections");

    }
    private int _receiveTimeout = 2000;
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
        {
          _clientSocket!.ReceiveTimeout = value;
        }
        _receiveTimeout = value;
      }
    }
    /// <summary>
    /// The communication new line string
    /// </summary>
    public string NewlineString
    {
      get => _newlineString;
      set
      {
        _newlineString = value ?? throw new ArgumentNullException("newlineString", "No newline string was supplied"); ;
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
          return _socketStream!.DataAvailable;
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
      IPEndPoint endPoint = new(_ipAddress!, _port);
      _clientSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
      try
      {
        _clientSocket.Connect(endPoint);
      }
      catch (Exception exc)
      {
        throw new InvalidOperationException("Connecting to the remote device failed!", exc);
      }
      //Check if socket is connected
      if (_clientSocket.Connected == true)
      {
        _clientSocket.SendTimeout = 1000;
        _clientSocket.ReceiveTimeout = _receiveTimeout;
        _clientSocket.Blocking = true;
        //Create Reader and Writer objects to perform higher level calls.
        _socketStream = new NetworkStream(_clientSocket) { ReadTimeout = _receiveTimeout, WriteTimeout = 1000 };
      }
      _isConnected = _clientSocket.Connected;
    }


    /// <summary>
    /// The method to close the communication interface
    /// </summary>
    public void Disconnect()
    {
      if (!IsConnected)
      {
        return;
      }
      _socketStream?.Close();
      _socketStream?.Dispose();
      _clientSocket?.Close();
      _isConnected = false;
    }

    /// <summary>
    /// Returns if the connection is established
    /// </summary>
    /// <returns>True if the connection is established</returns>
    public bool IsConnected
    {
      get => _isConnected;
    }

    /// <summary>
    /// Method to write a byte-array to the device (e.g. a binary file)
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
        ArgumentNullException _exc = new(nameof(offset), "Offset cannot be less than 0 or larger than the length of data");
        throw _exc;
      }
      if ((count < 0) || (count + offset > data.Length))
      {
        ArgumentNullException _exc = new(nameof(count), "Count cannot be less than zero or larger than the length of Data when counting from offset");
        throw _exc;
      }
      try
      {
        _socketStream!.Write(data, offset, count);
        //                Console.WriteLine("Sending " + count.ToString() + " characters starting at " + offset.ToString());
      }
      catch (IOException exc)
      {
        throw new InvalidOperationException("Writing to the device failed!", exc);
      }
      catch (ObjectDisposedException exc)
      {
        throw new ObjectDisposedException("Connection to device broken", exc);
      }
      catch (NullReferenceException exc)
      {
        throw new ObjectDisposedException("Not connected", exc);
      }
    }

    /// <summary>
    /// The method used to send data to the reader
    /// </summary>
    /// <param name="outputBuffer">
    /// The data / command sent to the reader
    /// </param>
    /// <exception cref="T:System.ArgumentNullException">
    /// Thrown when the specified <paramref name="outputBuffer"/>  is  <see langword="null"/>.
    /// </exception>
    /// <exception cref="T:System.InvalidOperationException">
    /// Thrown when an exception occurs when trying to access the port (e.g. port closed, timeout).
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// Thrown when the underlying function reports a broken stream
    /// </exception>
    public void SendCommand(string outputBuffer)
    {
      if (outputBuffer == null)
      {
        ArgumentNullException _exception = new(nameof(outputBuffer), "No string was passed to the send command");
        throw _exception;
      }
      try
      {
        byte[] help = System.Text.Encoding.ASCII.GetBytes(outputBuffer + "\u000D");
        _socketStream!.Write(help, 0, help.Length);
      }
      catch (IOException exc)
      {
        throw new InvalidOperationException("Writing to the device failed!", exc);
      }
      catch (ObjectDisposedException exc)
      {
        throw new ObjectDisposedException("Connection to device broken", exc);
      }
      catch (NullReferenceException exc)
      {
        throw new ObjectDisposedException("Not connected", exc);
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
      byte[] tempBytes = new byte[count];
      DateTime start;
      start = DateTime.Now;
      for (int i = 0; i < count; i++)
      {
        try
        {
          TimeSpan ts = DateTime.Now.Subtract(start);
          if ((!_socketStream!.DataAvailable) && (ts.TotalMilliseconds < _clientSocket!.ReceiveTimeout))
            continue;
          if (ts.TotalMilliseconds >= _clientSocket!.ReceiveTimeout)
            throw new TimeoutException("Reading from the port timed out");
          if (_socketStream.Read(tempBytes, i, 1) < 1)
            throw new InvalidOperationException("Reading from network stream didn't return anything");
          start = DateTime.Now;
        }
        catch (IOException e)
        {
          throw new TimeoutException("Reading from the port timed out", e);
        }
        catch (ObjectDisposedException e)
        {
          throw new ObjectDisposedException("Connection to device lost", e);
        }
        catch (NullReferenceException exc)
        {
          throw new ObjectDisposedException("Not connected", exc);
        }
      }
      return tempBytes;
    }

    /// <summary>
    /// The method used to synchronously read data from the reader
    /// </summary>
    /// <returns>
    /// The string read - without the newline character
    /// </returns>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown when no answer is received for more than 400ms.
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// Thrown when the underlying function reports a broken stream
    /// </exception>
    public string Read(string endLineString)
    {
      StringBuilder sb = new();
      DateTime start = DateTime.Now;
      while (!sb.ToString().EndsWith(endLineString))
      {
        try
        {
          TimeSpan ts = DateTime.Now.Subtract(start);
          if ((!_socketStream!.DataAvailable) && (ts.TotalMilliseconds < _clientSocket!.ReceiveTimeout))
            continue;
          if (ts.TotalMilliseconds >= _clientSocket!.ReceiveTimeout)
          {
            throw new TimeoutException($"Response Timeout - {sb}");
          }
          int c = _socketStream.ReadByte();
          if (c < 0)
          {
            throw new TimeoutException($"Response Timeout - {sb}");
          }
          sb.Append(Convert.ToChar(c));
          start = DateTime.Now;
        }
        catch (IOException e)
        {
          throw new TimeoutException($"Reading from the port timed out - {sb}", e);
        }
        catch (ObjectDisposedException e)
        {
          throw new ObjectDisposedException("Connection to device lost", e);
        }
        catch (NullReferenceException exc)
        {
          throw new ObjectDisposedException("Not connected", exc);
        }
      }
      sb.Remove(sb.Length - endLineString.Length, endLineString.Length);
      return sb.ToString();
    }

    /// <summary>
    /// The method used to read a complete answer from the reader, check it for error messages and crc mismatch
    /// </summary>
    /// <returns>
    /// The answer read - nicely parsed
    /// </returns>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown when no answer is received for more than 400ms.
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// Thrown when the underlying function reports a broken stream
    /// </exception>
    /// <exception cref="T:System.InvalidOperationException">
    /// Thrown when the answer from the reader contained errors
    /// </exception>
    public string ReadResponse()
    {
      return Read(_newlineString);
    }

  }
}
