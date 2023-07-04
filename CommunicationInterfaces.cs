using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
[assembly: CLSCompliant(true)]

namespace CommunicationInterfaces
{
  /// <summary>
  /// The common interface definition for different hardware communication options
  /// </summary>
  public interface ICommunicationInterface
  {

    /// <summary>
    /// The communication receive timeout
    /// </summary>
    /// <exception cref="T:System.InvalidOperationException">
    /// Thrown in case baud rate setting did not work
    /// </exception>
    int ReceiveTimeout
    {
      get;
      set;
    }

    /// <summary>
    /// The communication baud rate
    /// </summary>
    /// <exception cref="T:System.InvalidOperationException">
    /// Thrown in case baud rate setting did not work
    /// </exception>
    int BaudRate
    {
      get;
      set;
    }

    /// <summary>
    /// The communication new line string
    /// </summary>
    string NewlineString
    {
      get;
      set;
    }

    /// <summary>
    /// Indicates whether data is available for reading
    /// </summary>
    /// <exception cref="T:System.ObjectDisposedException">
    /// Thrown when the underlying function reports a broken stream
    /// </exception>
    bool DataAvailable
    {
      get;
    }

    /// <summary>
    /// The method to connect the communication interface
    /// </summary>
    /// <exception cref="T:System.InvalidOperationException">
    /// Thrown when the  serial port could not be set up (e.g. wrong parameters, insufficient permissions, invalid port state).
    /// </exception>
    void Connect();

    /// <summary>
    /// The method to close the communication interface
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Connection flag
    /// </summary>
    public bool IsConnected { get; }

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
    void Send(byte[] data, int offset, int count);

    /// <summary>
    /// Method to write a byte-array to the device (e.g. a binary file)
    /// </summary>
    /// <param name="data">
    /// The overall byte-array of data
    /// </param>
    void Send(byte[] data)
    {
      Send(data, 0, data.Length);
    }

    /// <summary>
    /// Method to write a string to the device
    /// </summary>
    /// <param name="data">
    /// The overall byte-array of data
    /// </param>
    void Send(string data)
    {
      Send(System.Text.Encoding.ASCII.GetBytes(data));
    }

    /// <summary>
    /// The method used to send a command to the reader
    /// </summary>
    /// <param name="command">
    /// The data / command sent to the reader
    /// </param>
    /// <exception cref="T:System.ArgumentNullException">
    /// Thrown when the specified <paramref name="command"/>  is  <see langword="null"/>.
    /// </exception>
    /// <exception cref="T:System.InvalidOperationException">
    /// Thrown when an exception occurs when trying to access the port (e.g. port closed, timeout).
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// Thrown when the underlying function reports a broken stream
    /// </exception>

    void SendCommand(string command);

    /// <summary>
    /// Method to read a stream of bytes from device
    /// </summary>
    /// <param name="count">
    /// Number of bytes to read
    /// </param>
    /// <returns>
    /// The bytes read
    /// </returns>
    byte[] Read(int count);

    /// <summary>
    /// The method used to synchronously read a reader response
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
    string ReadResponse();

  }

}
