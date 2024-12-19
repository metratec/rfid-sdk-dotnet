using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The status event arguments
  /// </summary>
  public class StatusEventArgs : EventArgs
  {
    /// <summary>
    /// Status event
    /// </summary>
    /// <param name="status">The status as integer</param>
    /// <param name="message">The status message</param>
    /// <param name="timestamp">The change timestamp</param>
    public StatusEventArgs(int status, string message, DateTime timestamp)
    {
      Status = status;
      Message = message;
      Timestamp = timestamp;
    }
    /// <summary>
    /// The new status
    /// </summary>
    /// <value></value>
    public int Status { get; }
    /// <summary>
    /// The new status message
    /// </summary>
    /// <value></value>
    public string Message { get; }
    /// <summary>
    /// The change timestamp
    /// </summary>
    /// <value></value>
    public DateTime Timestamp { get; }
  }

  /// <summary>
  /// The input change event arguments
  /// </summary>
  public class InputChangedEventArgs : EventArgs
  {
    /// <summary>
    /// Input change event
    /// </summary>
    /// <param name="inputPin">changed input pin</param>
    /// <param name="isHigh">current value</param>
    /// <param name="timestamp">The change timestamp</param>
    public InputChangedEventArgs(int inputPin, bool isHigh, DateTime timestamp)
    {
      Pin = inputPin;
      IsHigh = isHigh;
      Timestamp = timestamp;
    }
    /// <summary>
    /// The input pin number
    /// </summary>
    /// <value></value>
    public int Pin { get; }
    /// <summary>
    /// The current value
    /// </summary>
    /// <value></value>
    public bool IsHigh { get; }
    /// <summary>
    /// The change timestamp
    /// </summary>
    /// <value></value>
    public DateTime Timestamp { get; }
  }

  /// <summary>
  /// The new inventory event arguments
  /// </summary>
  public class NewInventoryEventArgs<T> : EventArgs where T : RfidTag
  {
    /// <summary>
    /// Inventory event
    /// </summary>
    /// <param name="tags">the founded transponder</param>
    /// <param name="timestamp">The change timestamp</param>
    public NewInventoryEventArgs(List<T> tags, DateTime timestamp)
    {
      Tags = tags;
      Timestamp = timestamp;
    }
    /// <summary>
    /// The new status
    /// </summary>
    /// <value></value>
    public List<T> Tags { get; }
    /// <summary>
    /// The change timestamp
    /// </summary>
    /// <value></value>
    public DateTime Timestamp { get; }
  }

  /// <summary>
  /// new inventory event arguments
  /// </summary>
  public class NewRequestResponseArgs : EventArgs
  {
    /// <summary>
    /// Inventory event
    /// </summary>
    /// <param name="tag">the transponder with the response</param>
    /// <param name="timestamp">The change timestamp</param>
    public NewRequestResponseArgs(HfTag tag, DateTime timestamp)
    {
      Tag = tag;
      Timestamp = timestamp;
    }
    /// <summary>
    /// The new status
    /// </summary>
    /// <value></value>
    public HfTag Tag { get; }
    /// <summary>
    /// The change timestamp
    /// </summary>
    /// <value></value>
    public DateTime Timestamp { get; }
  }
}