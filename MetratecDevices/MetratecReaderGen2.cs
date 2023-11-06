using CommunicationInterfaces;
using Microsoft.Extensions.Logging;

namespace MetraTecDevices
{
  /// <summary>
  /// The reader class for the ASCII based metratec reader
  /// </summary>
  public abstract class MetratecReaderGen2<T> : MetratecReader<T> where T : RfidTag
  {
    /// <summary>
    /// Input change event handler
    /// </summary>
    public event EventHandler<InputChangedEventArgs>? InputChanged;

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

    /// <summary>
    /// The reader class for all Metratec reader
    /// </summary>
    /// <param name="connection">The connection interface</param>
    public MetratecReaderGen2(ICommunicationInterface connection) : this(connection, null!, null!)
    {
    }

    /// <summary>
    /// The reader class for all Metratec reader
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    public MetratecReaderGen2(ICommunicationInterface connection, ILogger logger) : this(connection, null!, logger)
    {

    }

    /// <summary>
    /// The reader class for all Metratec reader
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// /// <param name="id">The reader id</param>

    public MetratecReaderGen2(ICommunicationInterface connection, string id) : this(connection, id, null!)
    {
    }

    /// <summary>
    /// The reader class for all Metratec reader
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="id">The reader id</param>
    /// <param name="logger">The connection interface</param>
    public MetratecReaderGen2(ICommunicationInterface connection, string id, ILogger logger) : base(connection, id, logger)
    {
      connection.NewlineString = "\r\n";
    }
    /// <summary>
    /// Send a command and check if the response contains "OK"
    /// </summary>
    /// <param name="command">the command to send</param>
    protected void SetCommand(String command)
    {
      ExecuteCommand(command);
    }

    /// <summary>
    /// Send a command and check if the response contains "OK"
    /// </summary>
    /// <param name="command">the command to send</param>
    /// <returns>the command response</returns>
    protected String GetCommand(String command)
    {
      return ExecuteCommand(command);
    }

    /// <summary>
    /// Send a command and returns the response
    /// </summary>
    /// <param name="command">the command</param>
    /// <param name="timeout">the response timeout, defaults to 2000ms</param>
    /// <returns></returns>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    public override string ExecuteCommand(string command, int timeout = 10000)
    {
      SendCommand(command);
      try
      {
        string resp = GetResponse();
        if (!command.StartsWith(resp))
        {
          throw new InvalidOperationException($"Wrong response to '{command}' - {resp}");
        }
        string? data = null;
        while (true)
        {
          resp = GetResponse();
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
                throw new InvalidOperationException(data);
              }
              throw new InvalidOperationException($"Reader Error '{command}' - {data}");
          }
          data = resp;
        }
      }
      catch (TimeoutException e)
      {
        throw new TimeoutException($"Response timeout ({command})", e);
      }
      catch (Exception)
      {
        throw;
      }
    }

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
      InputChanged(this, args);
    }

    private bool _commandReceived = false;

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
        case 'A': // AT command
          _commandReceived = true;
          break;
        case 'O': // OK
          _commandReceived = false;
          break;
        case 'E': // Error
          _commandReceived = false;
          break;
        case '+':
          if (_commandReceived)
          {
            break;
          }
          else
          {
            // Handle Event
            switch (response[1])
            {
              case 'H': // Heartbeat
                return;
              case 'C':
                // Inventory event
                HandleInventoryEvent(response);
                break;
              case 'I':
                // input changed
                if (response.StartsWith("+IEV: "))
                {
                  // +IEV: 1,HIGH
                  // +IEV: 2,LOW
                  string[] split = SplitLine(response[6..]);
                  FireInputChangeEvent(int.Parse(split[0]), split[1].ToUpper().Equals("HIGH"));
                }
                break;
            }
            return;
          }

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
    protected override void PrepareReader()
    {
      StopInventory();
      base.PrepareReader();
    }

    /// <summary>
    /// Configure the reader.
    /// The base implementation must be called after success.
    /// </summary>
    protected override void ConfigureReader()
    {
      SetHeartBeatInterval(10);
      EnableInputEvents();
    }

    /// <summary>
    /// Parse the error response and throw a InvalidOperationException with a detailed message
    /// </summary>
    /// <param name="response"></param>
    /// <returns>the InvalidOperationException</returns>
    protected InvalidOperationException ParseErrorResponse(String response)
    {
      return new InvalidOperationException($"Unhandled Error: {response}");
    }
    /// <summary>
    /// Set the HeartBeatInterval ... override for send the heartbeat command.
    /// The base implementation must be called after success.
    /// </summary>
    /// <param name="intervalInSec"></param>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    protected override void SetHeartBeatInterval(int intervalInSec)
    {
      if (0 > intervalInSec || intervalInSec > 60)
      {
        throw new InvalidOperationException("Number out of range ([0,60])");
      }
      SetCommand($"AT+HBT={intervalInSec}");
      base.SetHeartBeatInterval(intervalInSec);
    }

    /// <summary>
    /// Returns the firmware revision ({firmware} {version})
    /// </summary>
    /// <returns>The firmware revision ({firmware} {version})</returns>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    protected override void UpdateDeviceRevisions()
    {
      string[] responses = SplitResponse(GetCommand("ATI"));
      // +SW: PULSAR_LR 0100<CR>
      // +HW: PULSAR_LR 0103<CR>
      // +SERIAL: 2020090817420000
      FirmwareName = responses[0][5..^5];
      FirmwareVersion = responses[0][^4..];
      FirmwareMajorVersion = int.Parse(FirmwareVersion[..2]);
      FirmwareMinorVersion = int.Parse(FirmwareVersion[2..]);
      if (responses[1].Length > 6)
      {
        HardwareName = responses[1][5..^5];
        HardwareVersion = responses[1][^4..];
      }
      else
      {
        HardwareName = FirmwareName;
        HardwareVersion = "0100";
      }
      SerialNumber = responses[2][9..];
    }

    /// <summary>
    /// Returns true if the input pin is high, otherwise false
    /// </summary>
    /// <param name="pin">The requested input pin number</param>
    /// <returns>True if the input pin is high, otherwise false</returns>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    public override bool GetInput(int pin)
    {
      if (1 > pin || pin > 2)
      {
        throw new InvalidOperationException("Number out of range ([1,2])");
      }
      string[] responses = SplitResponse(GetCommand("AT+IN?"));
      return responses[pin - 1].Contains("HIGH");
    }
    /// <summary>
    /// Enable or disable input events
    /// </summary>
    /// <param name="enable">enable/disable</param>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
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
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    public override void SetOutput(int pin, bool value)
    {
      if (1 > pin || pin > 4)
      {
        throw new InvalidOperationException("Number out of range ([1,4])");
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
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    public bool GetOutput(int pin)
    {
      if (1 > pin || pin > 4)
      {
        throw new InvalidOperationException("Number out of range ([1,4])");
      }
      string[] responses = SplitResponse(GetCommand("AT+OUT?"));
      return responses[pin - 1].Contains("HIGH");
    }

    /// <summary>
    /// Sets the current antenna to use
    /// </summary>
    /// <param name="antennaPort">the antenna to use</param>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
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
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
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
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
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
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
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
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    public virtual List<int> GetAntennaMultiplex()
    {
      String[] split = SplitLine(GetCommand("AT+MUX?")[6..]);
      List<int> sequence = new();
      if (split.Length == 1)
      {
        // throw new InvalidOperationException("No multiplex sequence activated, please use 'GetAntennaMultiplex' command");
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
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// /// If the reader is not connected or the connection is lost
    /// </exception>
    public override void StopInventory()
    {
      try
      {
        SetCommand("AT+BINV");
      }
      catch (InvalidOperationException e)
      {
        if (!e.ToString().Contains("is not running"))
        {
          throw e;
        }
      }
    }

  }

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
}