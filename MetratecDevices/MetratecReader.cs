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
  /// The base reader class for all Metratec reader
  /// </summary>
  public abstract class MetratecReader<T> where T : RfidTag
  {
    /// <summary>
    /// Reader id
    /// </summary>
    protected string id;
    /// <summary>
    /// Logger
    /// </summary>
    protected readonly ILogger Logger;

    private readonly ICommunicationInterface _connection;
    private Thread _t_worker;
    private Thread? _t_configure;
    private bool _running = false;

    private int _status = -1;
    private string _status_message = "";
    private MetratecCommunicationException? _status_exception;

    private readonly ConcurrentQueue<string> _responses = new();
    private readonly ConcurrentQueue<string> _commands = new();

    private int _connectionReceiveTimeout = 10000;
    private DateTime _lastResponseTime = DateTime.Now;

    /// <summary>
    /// Status change event handler
    /// </summary>
    public event EventHandler<StatusEventArgs>? StatusChanged;

    /// <summary>
    /// new inventory event handler
    /// </summary>
    public event EventHandler<NewInventoryEventArgs<T>>? NewInventory;
    private readonly Dictionary<string, T> _inventory = new ();
    /// <summary>
    /// If the event handler for new inventory is set and this value is true, 
    /// an empty inventory also triggers the event handler
    /// </summary>
    /// <value>Fire also empty inventories. Defaults to false</value>
    public bool FireEmptyInventories { get; set; }
    /// <summary>
    /// Reader default response timeout
    /// </summary>
    /// <value>Response timeout in millisecond. Defaults to 2000</value>
    public int ResponseTimeout { get; set; }
    /// <summary>
    /// Reader firmware name
    /// </summary>
    /// <value></value>
    public string? FirmwareName { get; protected set; }
    /// <summary>
    /// Reader hardware name
    /// </summary>
    /// <value></value>
    public string? HardwareName { get; protected set; }
    /// <summary>
    /// Reader firmware version
    /// </summary>
    /// <value></value>
    public string? FirmwareVersion { get; protected set; }
    /// <summary>
    /// Reader major firmware version
    /// </summary>
    protected int? FirmwareMajorVersion { get; set; }
    /// <summary>
    /// Firmware minor firmware version
    /// </summary>
    protected int? FirmwareMinorVersion { get; set; }
    /// <summary>
    /// Reader hardware version
    /// </summary>
    /// <value></value>
    public string? HardwareVersion { get; protected set; }
    /// <summary>
    /// Reader serial number
    /// </summary>
    /// <value></value>
    public string? SerialNumber { get; protected set; }

    /// <summary>
    /// Create a new instance of the MetratecReader class.
    /// </summary>
    /// <param name="connection">The connection interface</param>
    public MetratecReader(ICommunicationInterface connection) : this(connection, null!, null!) { }

    /// <summary>
    ///Create a new instance of the MetratecReader class.
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    public MetratecReader(ICommunicationInterface connection, ILogger logger) : this(connection, null!, logger) { }

    /// <summary>
    /// Create a new instance of the MetratecReader class.
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// /// <param name="id">The reader id</param>

    public MetratecReader(ICommunicationInterface connection, string id) : this(connection, id, null!) { }

    /// <summary>
    /// Create a new instance of the MetratecReader class.
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="id">The reader id</param>
    /// <param name="logger">The connection interface</param>
    public MetratecReader(ICommunicationInterface connection, string id, ILogger logger)
    {
      if (connection is null)
      {
        throw new NullReferenceException("Connection must be not null");
      }
      ResponseTimeout = 2000;
      FireEmptyInventories = false;
      _connection = connection;
      this.id = id ?? _connection.ToString() ?? "";
      Logger = logger ?? NullLogger.Instance;
      _t_worker = new Thread(new ThreadStart(Work));
    }
    /// <summary>
    /// Finalize-Methode
    /// </summary>
    ~MetratecReader()
    {
      if (Connected)
      {
        Disconnect();
      }
    }

    /// <summary>
    /// Connect the reader, this method does not wait for the connection
    /// </summary>
    public void Connect()
    {
      if (_running)
      {
        return;
      }
      OnStatusChanged(0, "Connecting...", DateTime.Now);
      _running = true;
      _t_worker = new Thread(new ThreadStart(Work));
      _t_worker.Start();
    }


    /// <summary>
    /// Connect the reader and wait for the connection established
    /// </summary>
    /// <param name="timeout">the connection timeout</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void Connect(int timeout)
    {
      Connect();
      DateTime start = DateTime.Now;
      while (DateTime.Now.Subtract(start).TotalMilliseconds < timeout && _status == 0)
      {
        Thread.Sleep(1);
      }
      if (_status == 0)
      {
        Disconnect();
        throw new MetratecReaderException("Connection timeout");
      }
      if (!Connected)
        throw new MetratecReaderException(_status_message);
    }

    /// <summary>
    /// Disconnect the reader
    /// </summary>
    public void Disconnect()
    {
      if(_status >= 1){
        try
        {
          StopInventory();
        }
        catch (MetratecReaderException)
        {
          // ignore
        }
      }
      Disconnect("Disconnected");
    }

    /// <summary>
    /// Stop the receive thread and disconnect the reader. Update the status message
    /// </summary>
    /// <param name="statusMessage">the reader status message</param>
    protected virtual void Disconnect(string statusMessage)
    {
      // this._connectionHandler.Disconnect();
      _running = false;
      while (_t_worker.IsAlive)
      {
        Thread.Sleep(20);
      }
      _connection.Disconnect();
      OnStatusChanged(-1, statusMessage, DateTime.Now);
    }

    /// <summary>
    /// The reader connection state
    /// </summary>
    public bool Connected { get => _status >= 0; }

    /// <summary>
    /// Set the connection end of frame string
    /// </summary>
    /// <param name="endOfFrame">the new end of frame string</param>
    protected void SetEndOfFrame(String endOfFrame)
    {
      _connection.NewlineString = endOfFrame;
    }

    /// <summary>
    /// Send a command
    /// </summary>
    /// <param name="command">the command</param>
    /// <exception cref="MetratecReaderException">
    /// if the reader is not connected
    /// </exception>
    protected virtual void SendCommand(string command)
    {
      if (Connected)
        _commands.Enqueue(command);
      else
        throw new MetratecReaderException("not connected");
    }

    /// <summary>
    /// Returns the next response
    /// </summary>
    /// <param name="timeout">the response timeout in ms, if not explicitly specified, the default response timeout is used</param>
    /// <returns></returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or the timeout has expired
    /// </exception>
    protected string GetResponse(int timeout = 0)
    {
      string response;
      int responseTimeout = 0 != timeout ? timeout : ResponseTimeout;
      DateTime start = DateTime.Now;
      while (DateTime.Now.Subtract(start).TotalMilliseconds < responseTimeout)
      {
        if (_responses.IsEmpty)
        {
          Thread.Sleep(20);
        }
        else
        {
          if (_responses.TryDequeue(out response!))
          {
            return response;
          }
        }
      }
      if (!Connected)
        throw new MetratecReaderException("Not connected");
      throw new MetratecReaderException("Response timeout");
    }

    /// <summary>
    /// Send a command and returns the response
    /// </summary>
    /// <param name="command">the command</param>
    /// <param name="timeout">the response timeout, defaults to 2000ms</param>
    /// <returns></returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public abstract string ExecuteCommand(string command, int timeout = 10000);

    /// <summary>
    /// Deletes the currently waiting responses from the buffer and returns them
    /// </summary>
    /// <returns>The currently waiting responses</returns>
    protected List<string> ClearResponseBuffer()
    {
      List<String> responses = new();
      string response;
      while (!_responses.IsEmpty)
      {
        if (_responses.TryDequeue(out response!))
        {
          responses.Add(response);
        }
      }
      return responses;
    }

    /// <summary>
    /// Set the HeartBeatInterval ... override for send the heartbeat command.
    /// The base implementation must be called after success.
    /// </summary>
    /// <param name="intervalInSec">Heartbeat interval in seconds. 0 for disable</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected virtual void SetHeartBeatInterval(int intervalInSec)
    {
      _connectionReceiveTimeout = (int)(intervalInSec * 1000 * 2.5);
    }

    /// <summary>
    /// Configure the reader.
    /// The base implementation must be called after success.
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected virtual void PrepareReader()
    {
      UpdateDeviceRevisions();
      ConfigureReader();
      _connection.ReceiveTimeout = ResponseTimeout - 100;
      OnStatusChanged(1, "Connected", DateTime.Now);
    }

    /// <summary>
    /// Configure the reader.
    /// The base implementation must be called after success.
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected abstract void ConfigureReader();

    /// <summary>
    /// Update the the firmware name and version ({firmware} {version})
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected abstract void UpdateDeviceRevisions();

    /// <summary>
    /// Set the reader power
    /// </summary>
    /// <param name="power">the reader power</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public abstract void SetPower(int power);

    /// <summary>
    /// Sets the current antenna to use
    /// </summary>
    /// <param name="antennaPort">the antenna to use</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public abstract void SetAntenna(int antennaPort);

    /// <summary>
    /// Sets the number of antennas to be multiplexed
    /// </summary>
    /// <param name="antennasToUse">the antenna count to use</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public abstract void SetAntennaMultiplex(int antennasToUse);

    /// <summary>
    /// Scan for the current inventory
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public abstract List<T> GetInventory();

    /// <summary>
    /// Starts the continuous inventory scan.
    /// If the inventory <see cref="NewInventory">event handler</see> is set,
    /// any transponder found will be delivered via it. 
    /// If the event handler is not set, the found transponders can be fetched
    /// via the method <see cref="FetchInventory">FetchInventory</see>
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public abstract void StartInventory();

    /// <summary>
    /// Stops the continuous inventory scan.
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public abstract void StopInventory();

    /// <summary>
    /// Returns true if the input pin is high, otherwise false
    /// </summary>
    /// <param name="pin">The requested input pin number</param>
    /// <returns>True if the input pin is high, otherwise false</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public abstract bool GetInput(int pin);

    /// <summary>
    /// Sets a output pin
    /// </summary>
    /// <param name="pin">The output pin number</param>
    /// <param name="value">True for set the pin high</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public abstract void SetOutput(int pin, bool value);

    private void PrepareReaderRunner()
    {
      try
      {
        PrepareReader();
      }
      catch (Exception ex)
      {
        Logger.LogDebug("Error during preparing reader - {}", ex.Message);
        Disconnect($"Error during preparing reader - Maybe wrong reader? ({FirmwareName ?? ""} {FirmwareVersion ?? ""})");
      }
    }

    internal void Work()
    {
      int waitTime = 2000;
      int retryCount = 0;
      while (_running)
      {
        if (!_connection.IsConnected)
        {
          try
          {
            _connection.Connect();
            _connection.ReceiveTimeout = 250;
            _lastResponseTime = DateTime.Now;
            if (_status != 0)
              OnStatusChanged(0, "Connecting...", DateTime.Now);
            _t_configure = new Thread(new ThreadStart(PrepareReaderRunner));
            _t_configure.Start();
          }
          catch (MetratecCommunicationException e)
          {
            Logger.LogTrace(e, "{} Connection exception", _connection.ToString());
            OnStatusChanged(-1, e.Message, DateTime.Now, e);
            _connection.Disconnect();
            retryCount++;
            Thread.Sleep(waitTime * retryCount);
            continue;
          }
        }
        
        while (_running)
        {
          //_logger.LogTrace("{} Next round", _id);
          try
          {
            if (_commands.IsEmpty && !_connection.DataAvailable)
            {
              if (_connectionReceiveTimeout > 0 && DateTime.Now.Subtract(_lastResponseTime).TotalMilliseconds > _connectionReceiveTimeout)
              {
                OnStatusChanged(-1, "Connection lost (timeout)", DateTime.Now);
                _connection.Disconnect();
                break;
              }
              Thread.Sleep(5);
              continue;
            }
            if (!_commands.IsEmpty)
            {
              string command;
              if (_commands.TryDequeue(out command!))
              {
                Logger.LogTrace("{} Send command {}", id, command);
                _connection.SendCommand(command);
              }
            }
            while (_connection.DataAvailable && _commands.IsEmpty)
            {
              string response = _connection.ReadResponse();
              _lastResponseTime = DateTime.Now;
              if (string.IsNullOrEmpty(response))
              {
                continue;
              }
              Logger.LogTrace("{} Recv response {}", id, response);
              HandleResponse(response);
            }
          }
          catch (MetratecCommunicationException e)
          {
            Logger.LogTrace(e, "{} Connection exception {}", id, _connection.ToString());
            OnStatusChanged(-1, e.Message, DateTime.Now, e);
            break;
          }
          catch (TimeoutException)
          {
            // Logger.LogTrace(e, "{} Timeout exception", _connection.ToString());
            continue;
          }
          catch (Exception e)
          {
            Logger.LogTrace(e, "{} {}", _connection.ToString(), e.Message);
            // Console.WriteLine($"Unhandled Exception - {e.Message} {e}");
          }
          // wait for new command or new data
        }
      }
    }
    /// <summary>
    /// Process the reader response...override for event check
    /// The base implementation adds the response to the responses queue.
    /// </summary>
    /// <param name="response">a reader response</param>
    protected virtual void HandleResponse(string response)
    {
      _responses.Enqueue(response);
    }

    /// <summary>
    /// Fire a inventory event
    /// </summary>
    /// <param name="tags">the founded tags</param>
    /// <param name="continuous">set to true if it came from a continuous scan</param>
    protected void FireInventoryEvent(List<T> tags, bool continuous)
    {
      if (null == NewInventory) {
        if (tags.Count > 0 && continuous)
          UpdateInventory(tags);
        return;
      }
      if (!FireEmptyInventories && tags.Count == 0)
        return;
      NewInventoryEventArgs<T> args = new(tags, new DateTime());
      ThreadPool.QueueUserWorkItem(o => NewInventory.Invoke(this, args));
    }

    private void UpdateInventory(List<T> tags){
      foreach (T tag in tags){
        if(_inventory.ContainsKey(tag.ID)){
          T current = _inventory[tag.ID];
          current.SeenCount += tag.SeenCount;
          current.LastSeen = tag.LastSeen;
        } else {
          _inventory[tag.ID] = tag;
        }
      }
    }
    /// <summary>
    /// Can be called when an inventory has been started and no inventory callback is set.
    /// Returns a list with the currently found transponders
    /// </summary>
    /// <returns>A list with the currently found transponders</returns>
    public List<T> FetchInventory() {
      List<T> inventory = new (_inventory.Values);
      _inventory.Clear();
      return inventory;
    }
    private void OnStatusChanged(int status, string message, DateTime timestamp, MetratecCommunicationException? exception = null)
    {
      _status = status;
      _status_message = message;
      _status_exception = exception;
      Logger.LogDebug("{} Status changed to {} ({})", id, message, status);
      if (null == StatusChanged)
        return;
      StatusEventArgs args = new(status, message, timestamp);
      ThreadPool.QueueUserWorkItem(o => StatusChanged.Invoke(this, args));
    }

  }


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
}