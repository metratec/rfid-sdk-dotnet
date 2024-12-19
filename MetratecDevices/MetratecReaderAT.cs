using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using CommunicationInterfaces;
using System.Threading;

namespace MetraTecDevices
{
  /// <summary>
  /// The base reader class for the metratec readers based on the AT protocol
  /// </summary>
  public abstract class MetratecReaderAT<T> : MetratecReader<T> where T : RfidTag
  {
    #region Properties

    /// <summary>
    /// Current antenna port
    /// </summary>
    /// <value></value>
    protected int CurrentAntennaPort { get; set; }
    /// <summary>
    /// True, if a single antenna is in use
    /// </summary>
    /// <value></value>
    protected bool SingleAntennaInUse { get; set; }

    #endregion Properties

    #region Event Handlers

    /// <summary>
    /// Input change event handler
    /// </summary>
    public event EventHandler<InputChangedEventArgs>? InputChanged;

    #endregion Event Handlers

    #region Constructor

    /// <summary>
    /// Create a new instance of the MetratecReaderAT class.
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public MetratecReaderAT(ICommunicationInterface connection, ILogger logger = null!, string id = null!) : base(connection, logger, id)
    {
      connection.NewlineString = "\r\n";
    }

    #endregion Constructor

    #region Public Methods

    /// <summary>
    /// Send a command and check if the response contains "OK"
    /// </summary>
    /// <param name="command">the command to send</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetCommand(String command)
    {
      ExecuteCommand(command);
    }
    /// <summary>
    /// Send a command and check if the response contains "OK"
    /// </summary>
    /// <param name="command">the command to send</param>
    /// <returns>the command response</returns>
    /// <param name="timeout">the response timeout, defaults to 2000ms</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public String GetCommand(String command, int timeout = 0)
    {
      return ExecuteCommand(command, timeout);
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
    public override string ExecuteCommand(string command, int timeout = 0)
    {
      SendCommand(command);
      try
      {
        string resp;
        string? data = null;
        while (true)
        {
          resp = GetResponse(timeout);
          switch (resp[0])
          {
            case 'O': // OK
              return data ?? "";
            case 'E': // ERROR
              if (null != data && data.Contains('<') && data.Contains('>'))
              {
                int indexFrom = data.IndexOf('<') + 1;
                int indexTo = data.LastIndexOf('>');
                data = data[indexFrom..indexTo];
                throw ParseErrorResponse(data);
              }
              throw new MetratecReaderException($"Error: {data}");
            case 'A': // Command Echo
              if (!command.StartsWith(resp))
              {
                Logger.LogWarning("Reader Response warning ({}) - {}", command, resp);
              }
              break;
          }
          data = resp;
        }
      }
      catch (MetratecReaderException e)
      {
        if (e.Message.Contains("Response timeout"))
        {
          throw new MetratecReaderException($"Command ({command}) malformed response", e);
        }
        throw;
      }
      catch (Exception)
      {
        throw;
      }
    }
    /// <summary>
    /// Returns true if the input pin is high, otherwise false
    /// </summary>
    /// <param name="pin">The requested input pin number</param>
    /// <returns>True if the input pin is high, otherwise false</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public override bool GetInput(int pin)
    {
      if (1 > pin || pin > 2)
      {
        throw new MetratecReaderException("Number out of range ([1,2])");
      }
      string[] responses = SplitResponse(GetCommand("AT+IN?"));
      return responses[pin - 1].Contains("HIGH");
    }
    /// <summary>
    /// Enable or disable input events
    /// </summary>
    /// <param name="enable">enable/disable</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void EnableInputEvents(bool enable = true)
    {
      SetCommand($"AT+IEV={(enable ? '1' : '0')}");
    }
    /// <summary>
    /// Sets a output pin
    /// </summary>
    /// <param name="pin">The output pin number</param>
    /// <param name="value">True for set the pin high</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public override void SetOutput(int pin, bool value)
    {
      if (1 > pin || pin > 4)
      {
        throw new MetratecReaderException("Number out of range ([1,4])");
      }
      string command = "AT+OUT=";
      for (int i = 1; i < 5; i++)
      {
        command += i != pin ? "," : value ? "1" : "0";
      }
      SetCommand(command);
    }
    /// <summary>
    /// Returns true if the output pin is high, otherwise false
    /// </summary>
    /// <param name="pin">The requested input pin number</param>
    /// <returns>True if the output pin is high, otherwise false</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public bool GetOutput(int pin)
    {
      if (1 > pin || pin > 4)
      {
        throw new MetratecReaderException("Number out of range ([1,4])");
      }
      string[] responses = SplitResponse(GetCommand("AT+OUT?"));
      return responses[pin - 1].Contains("HIGH");
    }
    /// <summary>
    /// Sets the current antenna to use
    /// </summary>
    /// <param name="antennaPort">the antenna to use</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public override void SetAntenna(int antennaPort)
    {
      SetCommand($"AT+ANT={antennaPort}");
      CurrentAntennaPort = antennaPort;
      SingleAntennaInUse = true;
    }
    /// <summary>
    /// Gets the current used single antenna
    /// </summary>
    /// <returns>the current used single antenna</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public int GetAntenna()
    {
      CurrentAntennaPort = int.Parse(GetCommand("AT+ANT?").Substring(6, 1));
      return CurrentAntennaPort;
    }
    /// <summary>
    /// Sets the number of antennas to be multiplexed
    /// </summary>
    /// <param name="antennasToUse">the antenna count to use</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public override void SetAntennaMultiplex(int antennasToUse)
    {
      SetCommand($"AT+MUX={antennasToUse}");
      SingleAntennaInUse = false;
    }
    /// <summary>
    /// Sets the antenna multiplex sequence. Set the order in which the antennas are activated.
    /// </summary>
    /// <param name="antennaSequence">the antenna sequence</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetAntennaMultiplex(List<int> antennaSequence)
    {
      SetCommand("AT+MUX=" + string.Join(",", antennaSequence.Select(s => $"{s}")));
      SingleAntennaInUse = false;
    }
    /// <summary>
    /// Gets the number of antennas to be multiplexed
    /// </summary>
    /// <returns>the number of antennas to be multiplexed</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public virtual List<int> GetAntennaMultiplex()
    {
      String[] split = SplitLine(GetCommand("AT+MUX?")[6..]);
      List<int> sequence = new();
      if (split.Length == 1)
      {
        // throw new MetratecReaderException("No multiplex sequence activated, please use 'GetAntennaMultiplex' command");
        int antennas = int.Parse(split[0]);
        for (int i = 1; i <= antennas; i++)
        {
          sequence.Add(i);
        }
      }
      else
      {
        foreach (String value in split)
        {
          sequence.Add(int.Parse(value));
        }
      }
      return sequence;
    }
    /// <summary>
    /// Stops the continuous inventory scan.
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public override void StopInventory()
    {
      try
      {
        SetCommand("AT+BINV");
      }
      catch (MetratecReaderException e)
      {
        if (!e.ToString().Contains("is not running"))
        {
          throw;
        }
      }
    }
    /// <inheritdoc/>
    public override void Reset()
    {
      SetCommand("AT+RST");
      base.Reset();
    }

    #endregion Public Methods

    #region Protected Methods

    /// <summary>
    /// Parse the error message and return the reader or transponder exception
    /// </summary>
    /// <param name="response">error response</param>
    /// <returns></returns>
    protected abstract MetratecReaderException ParseErrorResponse(String response);
    /// <summary>
    /// Split a multiline response (and check the crc)
    /// </summary>
    /// <param name="response">the reader response</param>
    /// <returns></returns>
    protected string[] SplitResponse(string response)
    {
      string[] array = response.Split("\u000D", StringSplitOptions.RemoveEmptyEntries);
      return array;
    }
    /// <summary>
    /// Split a line response (and check the crc)
    /// </summary>
    /// <param name="responseLine">a line in the reader response</param>
    /// <returns></returns>
    protected string[] SplitLine(string responseLine)
    {
      string[] array = responseLine.Split(",", StringSplitOptions.RemoveEmptyEntries);
      return array;
    }
    /// <summary>
    /// Fire a inventory event
    /// </summary>
    /// <param name="inputPin">the changed input pin</param>
    /// <param name="isHigh">the new value</param>
    protected void FireInputChangeEvent(int inputPin, bool isHigh)
    {
      if (null == InputChanged)
        return;
      InputChangedEventArgs args = new(inputPin, isHigh, new DateTime());
      ThreadPool.QueueUserWorkItem(o => InputChanged.Invoke(this, args));
    }
    /// <summary>
    /// Process the reader response...override for event check
    /// The base implementation must be called after success.
    /// </summary>
    /// <param name="response">a reader response</param>
    protected override void HandleResponse(string response)
    {
      switch (response[0])
      {
        case '\r':
        case '\n':
          return;
        case '+':
          // Handle Event
          switch (response[1])
          {
            case 'H': // Heartbeat
              return;
            case 'C':
              // Inventory event
              if (response[2] == 'I' || response[2] == 'M') // +CINV +CINVR +CMINV
              {
                HandleInventoryEvent(response);
                return;
              }
              break;
            case 'I':
              if (response[2] == 'E') // +IEV:
              {
                // input changed
                // +IEV: 1,HIGH
                // +IEV: 2,LOW
                string[] split = SplitLine(response[6..]);
                FireInputChangeEvent(int.Parse(split[0]), split[1].ToUpper().Equals("HIGH"));
                return;
              }
              break;
          }
          break;
      }
      base.HandleResponse(response);
    }
    /// <summary>
    /// Parse the inventory event (+CINV, +CMINV, +CINVR)
    /// </summary>
    /// <param name="response"></param>
    protected abstract void HandleInventoryEvent(string response);
    /// <summary>
    /// Configure the reader.
    /// The base implementation must be called after success.
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected override void PrepareReader()
    {
      StopInventory();
      base.PrepareReader();
    }
    /// <summary>
    /// Configure the reader.
    /// The base implementation must be called after success.
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected override void ConfigureReader()
    {
      SetHeartBeatInterval(IsSerialConnection() ? 0 : 10);
    }
    /// <summary>
    /// Set the HeartBeatInterval ... override for send the heartbeat command.
    /// The base implementation must be called after success.
    /// </summary>
    /// <param name="intervalInSec">Heartbeat interval in seconds. 0 for disable</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected override void SetHeartBeatInterval(int intervalInSec)
    {
      if (0 > intervalInSec || intervalInSec > 60)
      {
        throw new MetratecReaderException("Number out of range ([0,60])");
      }
      SetCommand($"AT+HBT={intervalInSec}");
      base.SetHeartBeatInterval(intervalInSec);
    }
    /// <summary>
    /// Update the the firmware name and version ({firmware} {version})
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected override void UpdateDeviceRevisions()
    {
      string[] responses = SplitResponse(GetCommand("ATI"));
      // +SW: PULSAR_LR 0100<CR>
      // +HW: PULSAR_LR 0103<CR>
      // +SERIAL: 2020090817420000
      string[] Firmware = responses[0].Split(" ");
      FirmwareName = Firmware[1];
      FirmwareVersion = Firmware[^1];
      // FirmwareName = responses[0][5..^5];
      // FirmwareVersion = responses[0][^4..];
      FirmwareMajorVersion = int.Parse(FirmwareVersion[..2]);
      FirmwareMinorVersion = int.Parse(FirmwareVersion[2..]);
      if (responses[1].Length > 6)
      {
        string[] Hardware = responses[1].Split(" ");
        HardwareName = Hardware[1];
        HardwareVersion = Hardware[^1];
        // HardwareName = responses[1][5..^5];
        // HardwareVersion = responses[1][^4..];
      }
      else
      {
        HardwareName = FirmwareName;
        HardwareVersion = "0100";
      }
      SerialNumber = responses[2][9..];
    }

    #endregion Protected Methods

  }

  #region Configuration Classes

  /// <summary>
  /// Class for the high on tag setting
  /// </summary>
  public class HighOnTagSetting
  {
    /// <summary>
    /// Create a new instance
    /// </summary>
    /// <param name="enable">Sets to false for disable the high on tag feature</param>
    public HighOnTagSetting(bool enable)
    {
      Enable = enable;
    }
    /// <summary>
    /// Create a new instance
    /// </summary>
    /// <param name="outputPin">Output pin signaling a found tag</param>
    public HighOnTagSetting(int outputPin)
    {
      Enable = true;
      OutputPin = outputPin;
    }
    /// <summary>
    /// Create a new instance
    /// </summary>
    /// <param name="outputPin">Output pin signaling a found tag</param>
    /// <param name="duration">pin high duration in milliseconds</param>
    public HighOnTagSetting(int outputPin, int duration) : this(outputPin)
    {
      Duration = duration;
    }
    /// <summary>
    /// True if high on tag is enables
    /// </summary>
    /// <value>True if high on tag is enables</value>
    public bool Enable { get; set; }
    /// <summary>
    /// Output pin signaling a found tag
    /// </summary>
    /// <value>output pin number</value>
    public int? OutputPin { get; set; }
    /// <summary>
    /// Pin high duration in milliseconds
    /// </summary>
    /// <value>high duration in milliseconds</value>
    public int? Duration { get; set; }
  }

  #endregion Configuration Classes

}