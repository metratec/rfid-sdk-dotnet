using CommunicationInterfaces;
using Microsoft.Extensions.Logging;

namespace MetraTecDevices
{
  /// <summary>
  /// The reader class for the ASCII based metratec reader
  /// </summary>
  public abstract class MetratecReaderGen1<T> : MetratecReader<T> where T : RfidTag
  {
    private bool _isCRC = false;
    /// <summary>
    /// Current antenna port
    /// </summary>
    /// <value></value>
    protected int CurrentAntennaPort { get; set; }

    /// <summary>
    /// The reader class for all Metratec reader
    /// </summary>
    /// <param name="connection">The connection interface</param>
    public MetratecReaderGen1(ICommunicationInterface connection) : this(connection, null!, null!)
    {
    }

    /// <summary>
    /// The reader class for all Metratec reader
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    public MetratecReaderGen1(ICommunicationInterface connection, ILogger logger) : this(connection, null!, logger)
    {
    }

    /// <summary>
    /// The reader class for all Metratec reader
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// /// <param name="id">The reader id</param>

    public MetratecReaderGen1(ICommunicationInterface connection, string id) : this(connection, id, null!)
    {
    }

    /// <summary>
    /// The reader class for all Metratec reader
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="id">The reader id</param>
    /// <param name="logger">The connection interface</param>
    public MetratecReaderGen1(ICommunicationInterface connection, string id, ILogger logger) : base(connection, id, logger)
    {
    }

    /// <summary>
    /// Process the reader response...override for event check
    /// The base implementation must be called after success.
    /// </summary>
    /// <param name="response">a reader response</param>
    protected override void HandleResponse(string response)
    {
      // check for events
      switch (response[0])
      {
        case 'H':
          if (response.StartsWith("HBT"))
          {
            return;
          }
          break;
        case 'I':
          switch (response[1])
          {
            case 'V': // Inventory: IVF 00
              if (response[2] == 'F')
              {
                HandleInventoryResponse(response);
                return;
              }
              break;
          }
          break;
      }
      if (response.Length >= 14 && response[(response.Length - 14)..].Contains("IVF"))
      {
        HandleInventoryResponse(response);
        return;
      }
      base.HandleResponse(response);
    }

    /// <summary>
    /// Send a command
    /// </summary>
    /// <param name="command">the command</param>
    /// <exception cref="T:System.ObjectDisposedException">
    /// Thrown if the reader is not connected
    /// </exception>
    protected override void SendCommand(string command)
    {
      if (_isCRC)
      {
        base.SendCommand($"{command} {ComputeCRC(command + " ")}");
      }
      else
      {
        base.SendCommand(command);
      }
    }

    /// <summary>
    /// Method to compute CRC (starting value 0xFFFF, 8408 polynomial)
    /// </summary>
    /// <param name="toCompute">
    /// The string over which the CRC is to be computed
    /// </param>
    /// <returns>
    /// A string containing the CRC as 4 hex digits
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// Thrown when the specified <paramref name="toCompute"/>  is  <see langword="null"/>.
    /// </exception>
    internal static string ComputeCRC(string toCompute)
    {
      if (toCompute == null)
        throw new ArgumentNullException(nameof(toCompute), "The string passed to the CRC computation function was null");
      string result;
      byte[] _konvertierteDaten = System.Text.Encoding.ASCII.GetBytes(toCompute);
      int i, j;
      UInt16 _CRC;
      _CRC = 0xFFFF;
      for (i = 0; i < _konvertierteDaten.Length; i++)
      {
        _CRC ^= _konvertierteDaten[i];
        for (j = 0; j < 8; j++)
        {
          if ((_CRC & 0x0001) == 0) _CRC >>= 1;
          else _CRC = (UInt16)((_CRC >> 1) ^ 0x8408);
        }
        //if ((_CRC == 1) && (_CRC == 2))
        //    break;
      }
      result = _CRC.ToString("X4");
      return result;
    }
    /// <summary>
    /// Method to check reader answers for correct CRCs
    /// </summary>
    /// <param name="toCheck">
    /// The string to be checked - contains the CRC as the last 4 characters
    /// </param>
    /// <returns>
    /// True if CRC is correct
    /// </returns>
    internal static bool CheckCRC(string toCheck)
    {
      if (toCheck.Length < 4)
        return false;
      return toCheck.Substring(toCheck.Length - 4, 4) == ComputeCRC(toCheck[..^4]);
    }

    /// <summary>
    /// Configure the reader.
    /// The base implementation must be called after success.
    /// </summary>
    protected override void PrepareReader()
    {
      bool next = true;
      bool checkSleeping = false;
      ClearResponseBuffer();
      SendCommand("BRK");
      string receive;
      while (next)
      {
        try
        {
          receive = GetResponse();
          // System.out.println("Receive: "+receive);
          if (receive.Contains("BRA"))
          {
            next = false;
          }
          else if (receive.Contains("NCM"))
          {
            next = false;
          }
          else if (receive.Contains("CCE"))
          {
            _isCRC = true;
            if (checkSleeping)
              SendCommand("WAK");
            else
              SendCommand("BRK");
          }
          else if (receive.Contains("GMO"))
          {
            next = false;
          }
          else if (receive.Contains("DNS"))
          {
            next = false;
          }
          else if (receive.Contains("UCO"))
          {
            throw new InvalidOperationException("device is not a metraTec rfid reader");
          }
        }
        catch (TimeoutException e)
        {
          if (!checkSleeping)
          {
            checkSleeping = true;
            SendCommand("WAK");
          }
          else
          {
            throw e;
          }
        }
      }
      SetHeartBeatInterval(0);
      EnableEndOfFrame(true);
      EnableCrcCheck(true);
      SetHeartBeatInterval(10);
      base.PrepareReader();
    }

    /// <summary>
    /// Enable or Disable end of frame
    /// </summary>
    /// <param name="enable"></param>
    protected void EnableEndOfFrame(bool enable = true)
    {
      if (enable)
      {
        SetEndOfFrame("\r\n");
        SetCommand("EOF");
      }
      else
      {
        SetEndOfFrame("\r");
        SetCommand("NEF");
      }
    }
    /// <summary>
    /// Enable or Disable the antenna report. 
    /// If the reader is used with an antenna multiplexer, you can enable this to get the antenna information in the inventory response.
    /// </summary>
    /// <param name="enable"></param>
    public void EnableAntennaReport(bool enable = true)
    {
      SetCommand($"SAP ARP {(enable ? "ON" : "OFF")}");
    }
    /// <summary>
    /// Enable the Cyclic Redundancy Check (CRC) of the computer to reader communication
    /// </summary>
    /// <param name="enable">true for enable</param>
    public void EnableCrcCheck(bool enable = true)
    {
      if (enable)
      {
        SetCommand("CON");
        _isCRC = true;
      }
      else
      {
        SetCommand("COF");
        _isCRC = false;
      }
    }

    /// <summary>
    /// Send a command and check if the response contains "OK"
    /// </summary>
    /// <param name="command">the command to send</param>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    protected void SetCommand(String command)
    {
      string response = ExecuteCommand(command);
      if (!response.Contains("OK"))
      {
        throw ParseErrorResponse(response);
      }
    }

    /// <summary>u
    /// Send a command and return the response
    /// </summary>
    /// <param name="command">the command to send</param>
    /// <returns>the command response</returns>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    public String GetCommand(String command)
    {
      string response = ExecuteCommand(command);
      if (_isCRC)
      {
        if (CheckCRC(response))
        {
          return response[..^5];
        }
        else
        {
          //multiline response?
          string[] array = SplitResponse(response);
          string responseWithoutCRC = "";
          for (int i = 0; i < array.Length; i++)
          {
            responseWithoutCRC += array[i];
            responseWithoutCRC += "\u000D";
          }
          return responseWithoutCRC[..^1];
        }
      }
      else
      {
        return response;
      }
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
        return GetResponse();
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
    /// <param name="response"></param>
    /// <returns></returns>
    protected string[] SplitResponse(string response)
    {
      string[] array = response.Split("\u000D", 128, StringSplitOptions.RemoveEmptyEntries);
      if (_isCRC)
      {
        if (response.StartsWith("CCE"))
          throw new InvalidOperationException($"CRC error - {response}");
        for (int i = 0; i < array.Length; i++)
        {
          if (CheckCRC(array[i]))
          {
            array[i] = array[i][..^5];
          }
          else
          {
            throw new InvalidOperationException($"CRC error - {response}");
          }
        }
      }
      return array;
    }

    /// <summary>
    /// Called if a new inventory response is received
    /// </summary>
    /// <param name="response">inventory response</param>
    protected abstract void HandleInventoryResponse(string response);

    /// <summary>
    /// Parse the error response and throw a InvalidOperationException with a detailed message
    /// </summary>
    /// <param name="response"></param>
    /// <returns>the InvalidOperationException</returns>
    protected InvalidOperationException ParseErrorResponse(String response)
    {
      switch (response[0..3])
      {
        // class hardware errors - produce exceptions / events in async cases
        case "ARH":
          return new InvalidOperationException("Hardware error detected: Antenna Reflectivity High. Please check hardware - especially antenna connection and tuning - or call support");
        case "BOD":
          return new InvalidOperationException("Hardware error detected: Brownout detected. Please check hardware or call support");
        case "BOF":
          return new InvalidOperationException("Hardware error detected: Buffer overflow. Please check hardware or call support");
        case "CRT":
          return new InvalidOperationException("Hardware error detected: Command Receive Timeout. Please check hardware or call support");
        case "EHF":
          return new InvalidOperationException("Hardware error detected: Hardware Failure. Please check hardware or call support");
        case "PLE":
          return new InvalidOperationException("Hardware error detected: PLL Error. Please check hardware or call support");
        case "SRT":
          return new InvalidOperationException("Hardware error detected: Hardware Reset. Please check hardware or call support");
        case "UER":
          return new InvalidOperationException($"Hardware error detected: Unknown Error. Please check hardware or call support. Full error string: {response}");
        case "URE":
          return new InvalidOperationException("Hardware error detected: UART Receive Error. Please check hardware or call support");
        // class parser / dll error - produce exceptions / events in async cases
        case "UCO":
          return new InvalidOperationException("Command not supported");
        case "CCE":
        case "DNS":
        case "EDX":
        case "EHX":
        case "NCM":
        case "NOR":
        case "NOS":
        case "NRF":
        case "NSS":
        case "UPA":
        case "WDL":
          return new InvalidOperationException($"Parser error detected - if using direct mode please check string sent, otherwise contact support. Error message: {response}");
        // class tag answer / communication problems - are reported as is
        case "ACE":
        case "CER":
        case "FLE":
        case "HBE":
        case "PDE":
        case "RDL":
        case "RXE":
        case "TCE":
        case "TMT":
        case "TOE":
        case "TOR":
        default:
          //PLE and SRT can be found anywhere in the string - the others are to be reported as they are
          if (response.Contains("PLE"))
            return new InvalidOperationException("Hardware error detected: PLL Error. Please check hardware or call support");
          if (response.Contains("SRT"))
            return new InvalidOperationException("Hardware error detected: Hardware Reset. Please check hardware or call support");
          break;
      }
      return new InvalidOperationException($"Unhandled Error: {response}");
    }
    /// <summary>
    /// Set the HeartBeatInterval ... override for send the heartbeat command.
    /// The base implementation must be called after success.
    /// </summary>
    /// <param name="intervalInSec">Heartbeat interval in seconds</param>
    protected override void SetHeartBeatInterval(int intervalInSec)
    {
      try
      {
        SetCommand($"HBT {(intervalInSec > 0 ? intervalInSec : "OFF")}");
        base.SetHeartBeatInterval(intervalInSec);
      }
      catch (Exception e)
      {
        if (e is InvalidOperationException)
        {
          // disable heartbeat
          base.SetHeartBeatInterval(0);
        }
        else
        {
          throw;
        }
      }
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
      string response = GetCommand("RFW");
      if (response.Length > 3)
      {
        FirmwareName = response[..^4].Replace(" ", "");
        FirmwareMajorVersion = int.Parse(response.Substring(response.Length - 4, 2));
        FirmwareMinorVersion = int.Parse(response.Substring(response.Length - 2, 2));
        FirmwareVersion = $"{FirmwareMajorVersion}.{FirmwareMinorVersion}";

        response = GetCommand("RHW");
        HardwareName = response[..^4];
        HardwareVersion = int.Parse(response.Substring(response.Length - 4, 2)) + "." +
                          int.Parse(response.Substring(response.Length - 2, 2));
      }
      else
      {
        response = GetCommand("REV");
        if (response.Length > 3)
        {
          // return response.Substring(0, response.Length - 8) + response.Substring(response.Length - 4, 4);
          FirmwareName = response[..^8].Replace(" ", "");
          FirmwareMajorVersion = int.Parse(response.Substring(response.Length - 4, 2));
          FirmwareMinorVersion = int.Parse(response.Substring(response.Length - 2, 2));
          FirmwareVersion = $"{FirmwareMajorVersion}.{FirmwareMinorVersion}";
          HardwareName = response[..^8];
          HardwareVersion = $"{response.Substring(response.Length - 8, 2)}.{response.Substring(response.Length - 6, 2)}";
        }
        else
        {
          FirmwareName = "Unknown";
          FirmwareVersion = "0.0";
          HardwareName = "Unknown";
          HardwareVersion = "0.0";
        }
      }
      SerialNumber = GetCommand("RSN");
    }

    /// <summary>
    /// Returns true if the input pin is high, otherwise false
    /// </summary>
    /// <param name="pin">The requested input pin number</param>
    /// <returns>True if the input pin is high, otherwise false</returns>
    /// <exception cref="T:System.InvalidOperationException">
    /// Thrown in case exceptions occur during PC/reader communication (port busy, reading / writing error, wrong answer from reader) or if reader type does not support the command
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
      String response = GetCommand($"RIP {pin}");
      if (response.Contains("HI"))
      {
        return true;
      }
      else if (response.Contains("LOW"))
      {
        return false;
      }
      else
      {
        throw ParseErrorResponse(response);
      }
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
      SetCommand($"WOP {pin} {(value ? "HI" : "LOW")}");
    }

    /// <summary>
    /// Set the verbosity level
    /// </summary>
    /// <param name="level">
    /// 0.. Only necessary data (EPC, User Data, etc.) is returned. 
    /// 1..Default, most tag communication errors added.
    /// 2: All tag communication errors including RXE and CRE normally indicating a collision are send.
    /// </param>
    protected void SetVerbosityLevel(int level)
    {
      SetCommand($"VBL {level}");
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
      SetCommand($"SAP {antennaPort}");
      CurrentAntennaPort = antennaPort;
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
      SetCommand($"SAP AUT {antennasToUse}");
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
      string response = GetCommand("BRK");
      if (response.Contains("BRA") || response.Contains("NCM"))
        return;
      else
        throw ParseErrorResponse(response);
    }
  }
}