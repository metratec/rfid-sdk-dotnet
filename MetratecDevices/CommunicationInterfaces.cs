using System;
using System.Collections.Generic;
using System.IO;
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
    /// <exception cref="MetratecCommunicationException">
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
    /// <exception cref="MetratecCommunicationException">
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
    /// <exception cref="MetratecCommunicationException">
    /// Thrown when the underlying function reports a broken stream
    /// </exception>
    bool DataAvailable
    {
      get;
    }

    /// <summary>
    /// The method to connect the communication interface
    /// </summary>
    /// <exception cref="MetratecCommunicationException">
    /// Thrown when the connection could not set up (e.g. wrong parameters, insufficient permissions, invalid state).
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
    /// <exception cref="MetratecCommunicationException">
    /// Thrown when the data cannot be sent
    /// </exception>
    void Send(byte[] data, int offset, int count);

    /// <summary>
    /// Method to write a byte-array to the device (e.g. a binary file)
    /// </summary>
    /// <param name="data">
    /// The overall byte-array of data
    /// </param>
    /// <exception cref="MetratecCommunicationException">
    /// Thrown when the data cannot be sent
    /// </exception>
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
    /// <exception cref="MetratecCommunicationException">
    /// Thrown when the data cannot be sent
    /// </exception>
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
    /// <exception cref="MetratecCommunicationException">
    /// Thrown when the data cannot be sent
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
    /// <exception cref="MetratecCommunicationException">
    /// Thrown when the data cannot be read
    /// </exception>
    /// <exception cref="System.TimeoutException">
    /// If the data could not be read in time
    /// </exception>
    byte[] Read(int count);

    /// <summary>
    /// The method used to synchronously read a reader response
    /// </summary>
    /// <returns>
    /// The string read - without the newline character
    /// </returns>
    /// <exception cref="MetratecCommunicationException">
    /// Thrown when the data cannot be read
    /// </exception>
    /// <exception cref="System.TimeoutException">
    /// If the data could not be read in time
    /// </exception>
    string ReadResponse();

  }

  /// <summary>
  /// The exception that is thrown when an communication error with Metratec devices occurs.
  /// Derived from the System.IO.IOException exception
  /// </summary>
  public class MetratecCommunicationException : IOException
  {
    /// <summary>
    /// Initializes a new instance of the MetratecCommunicationException class.
    /// </summary>
    public MetratecCommunicationException() : base() { }
    /// <summary>
    /// Initializes a new instance of the MetratecCommunicationException class with a specified
    /// error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public MetratecCommunicationException(string? message) : base(message) { }
    /// <summary>
    /// Initializes a new instance of the MetratecCommunicationException class with a specified
    /// error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception. If the innerException
    /// parameter is not null, the current exception is raised in a catch block that
    /// handles the inner exception.</param>
    /// <returns></returns>
    public MetratecCommunicationException(string? message, Exception? innerException) : base(message, innerException) { }
  }

}
