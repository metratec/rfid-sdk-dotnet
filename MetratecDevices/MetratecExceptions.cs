using System;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The exception that is thrown when an communication error with Metratec devices occurs.
  /// </summary>
  public class MetratecReaderException : MetratecCommunicationException
  {
    /// <summary>
    /// Initializes a new instance of the MetratecReaderException class.
    /// </summary>
    public MetratecReaderException() : base() { }
    /// <summary>
    /// Initializes a new instance of the MetratecReaderException class with a specified
    /// error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public MetratecReaderException(string? message) : base(message) { }
    /// <summary>
    /// Initializes a new instance of the MetratecReaderException class with a specified
    /// error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception. If the innerException
    /// parameter is not null, the current exception is raised in a catch block that
    /// handles the inner exception.</param>
    /// <returns></returns>
    public MetratecReaderException(string? message, Exception? innerException) : base(message, innerException) { }
  }

  /// <summary>
  /// The exception that is triggered when a transponder returns an error
  /// </summary>
  public class TransponderException : MetratecReaderException
  {
    /// <summary>
    /// Initializes a new instance of the TransponderException class.
    /// </summary>
    public TransponderException() : base() { }
    /// <summary>
    /// Initializes a new instance of the TransponderException class with a specified
    /// error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public TransponderException(string? message) : base(message) { }
    /// <summary>
    /// Initializes a new instance of the TransponderException class with a specified
    /// error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception. If the innerException
    /// parameter is not null, the current exception is raised in a catch block that
    /// handles the inner exception.</param>
    /// <returns></returns>
    public TransponderException(string? message, Exception? innerException) : base(message, innerException) { }
  }
}