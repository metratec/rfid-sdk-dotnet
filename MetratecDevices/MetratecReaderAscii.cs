using System;
using System.Threading;
using CommunicationInterfaces;
using Microsoft.Extensions.Logging;

namespace MetraTecDevices
{
  /// <summary>
  /// The base reader class for the metratec readers based on the Ascii protocol
  /// </summary>
  public abstract class MetratecReaderAscii<T> : MetratecReader<T> where T : RfidTag
  {
    #region Properties

    /// <summary>
    /// Current antenna port
    /// </summary>
    /// <value></value>
    protected int CurrentAntennaPort { get; set; }
    /// <summary>
    /// communication crc check
    /// </summary>
    /// <value></value>
    protected bool IsCRC { get; set; } = false;

    #endregion Properties

    #region Event Handlers

    /// <summary>
    /// Input change event handler
    /// 
    /// Note: The input trigger does not work during a continuous inventory.
    /// Therefore, do not use the ‘StartInventory()’ method with the input listener.
    /// If you need an inventory after an input has been changed, then use single inventories
    /// with the ‘getInventory()’ method
    /// </summary>
    public event EventHandler<InputChangedEventArgs>? InputChanged;

    #endregion Event Handlers

    #region Constructor

    /// <summary>
    /// Create a new instance of the MetratecReaderAscii class.
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public MetratecReaderAscii(ICommunicationInterface connection, ILogger? logger = null, string? id = null) : base(connection, logger, id) { }

    #endregion Constructor

    #region Public Methods

    /// <summary>
    /// Enable or Disable the antenna report. 
    /// If the reader is used with an antenna multiplexer, you can enable this to get the antenna information in the inventory response.
    /// </summary>
    /// <param name="enable"></param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void EnableAntennaReport(bool enable = true)
    {
      SetCommand($"SAP ARP {(enable ? "ON" : "OFF")}");
    }
    /// <summary>
    /// Enable the Cyclic Redundancy Check (CRC) of the computer to reader communication
    /// </summary>
    /// <param name="enable">true for enable</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public virtual void EnableCrcCheck(bool enable = true)
    {
      if (enable)
      {
        SetCommand("CON");
        IsCRC = true;
      }
      else
      {
        SetCommand("COF");
        IsCRC = false;
      }
    }
    /// <summary>
    /// Send a command and check if the response contains "OK"
    /// </summary>
    /// <param name="command">the command to send</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetCommand(String command)
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
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public String GetCommand(String command)
    {
      string response = ExecuteCommand(command);
      if (IsCRC)
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
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public override string ExecuteCommand(string command, int timeout = 0)
    {
      SendCommand(command);
      try
      {
        return GetResponse(timeout);
      }
      catch (MetratecReaderException e)
      {
        if (e.Message.Contains("timeout"))
        {
          throw new MetratecReaderException($"Response timeout ({command})", e);
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
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public override void SetOutput(int pin, bool value)
    {
      SetCommand($"WOP {pin} {(value ? "HI" : "LOW")}");
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
      SetCommand($"SAP {antennaPort}");
      CurrentAntennaPort = antennaPort;
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
      SetCommand($"SAP AUT {antennasToUse}");
    }
    /// <summary>
    /// Stops the continuous inventory scan.
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public override void StopInventory()
    {
      string response = GetCommand("BRK");
      if (response.Contains("BRA") || response.Contains("NCM"))
        return;
      else
        throw ParseErrorResponse(response);
    }
    /// <inheritdoc/>
    public override void Reset()
    {
      SetCommand("RST");
      base.Reset();
    }

    #endregion Public Methods

    #region Protected Method

    /// <summary>
    /// Process the reader response...override for event check
    /// The base implementation must be called after success.
    /// </summary>
    /// <param name="response">a reader response</param>
    protected override void HandleResponse(string response)
    {
      // check for events
      if (response.Length == 0)
      {
        base.HandleResponse(response);
        return;
      }
      
      switch (response[0])
      {
        case 'H':
          if (response.StartsWith("HBT"))
          {
            return;
          }
          break;
        case 'I':
          if (response.Length > 1)
          {
            switch (response[1])
            {
              case 'V': // Inventory: IVF 00
                if (response.Length > 2 && response[2] == 'F')
                {
                  HandleInventoryResponse(response);
                  return;
                }
                break;
              case 'N':
                // input event: IN0 IN1 
                bool state = response.Contains("HI!");
                if (response.Length > 2 && int.TryParse(response[2].ToString(), out var inputPin))
                {
                  FireInputChangeEvent(inputPin, state);
                }
                else
                {
                  Logger.LogWarning("Invalid input pin format in response: {}", response);
                }
                return;
            }
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
    /// <exception cref="MetratecReaderException">
    /// if the reader is not connected
    /// </exception>
    protected override void SendCommand(string command)
    {
      if (IsCRC)
      {
        base.SendCommand($"{command} {ComputeCRC(command + " ")}");
      }
      else
      {
        base.SendCommand(command);
      }
    }
    /// <summary>
    /// Configure the reader.
    /// The base implementation must be called after success.
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected override void PrepareReader()
    {
      bool next = true;
      bool checkSleeping = false;
      ClearResponseBuffer();
      SetEndOfFrame("\r");
      IsCRC = false;
      SendCommand("BRK");
      string receive;
      while (next)
      {
        try
        {
          receive = GetResponse();
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
            IsCRC = true;
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
            throw new MetratecReaderException("device is not a metraTec rfid reader");
          }
        }
        catch (MetratecReaderException)
        {
          if (!checkSleeping)
          {
            checkSleeping = true;
            SendCommand("WAK");
          }
          else
          {
            throw;
          }
        }
      }
      SetHeartBeatInterval(0);
      EnableEndOfFrame(true);
      EnableCrcCheck(true);
      if (!IsSerialConnection())
      {
        SetHeartBeatInterval(10);
      }
      base.PrepareReader();
    }
    /// <summary>
    /// Stop the receive thread and disconnect the reader. Update the status message
    /// </summary>
    /// <param name="statusMessage">the reader status message</param>
    protected override void Disconnect(string statusMessage)
    {
      if (Connected)
      { 
        try
        {
          EnableCrcCheck(false);
          EnableEndOfFrame(false);
        } catch (Exception)
        {
          // Ignore
        }
      }
      base.Disconnect(statusMessage);
    }
    /// <summary>
    /// Called if a new inventory response is received
    /// </summary>
    /// <param name="response">inventory response</param>
    protected abstract void HandleInventoryResponse(string response);
    /// <summary>
    /// Enable or Disable end of frame
    /// </summary>
    /// <param name="enable"></param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
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
    /// Split a multiline response (and check the crc)
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    protected string[] SplitResponse(string response)
    {
      string[] array = response.Split("\u000D", 128, StringSplitOptions.RemoveEmptyEntries);
      if (IsCRC)
      {
        if (response.StartsWith("CCE"))
          throw new MetratecCommunicationException($"CRC error - {response}");
        for (int i = 0; i < array.Length; i++)
        {
          if (CheckCRC(array[i]))
          {
            array[i] = array[i][..^5];
          }
          else
          {
            throw new MetratecCommunicationException($"CRC error - {response}");
          }
        }
      }
      return array;
    }
    /// <summary>
    /// Parse the error response and throw a MetratecCommunicationException with a detailed message
    /// </summary>
    /// <param name="response"></param>
    /// <returns>the MetratecReaderException</returns>
    protected MetratecReaderException ParseErrorResponse(String response)
    {
      if (response.Length < 3)
      {
        return new MetratecReaderException($"Invalid error response format: {response}");
      }
      switch (response[0..3])
      {
        // class hardware errors - produce exceptions / events in async cases
        case "ARH":
          return new MetratecReaderException("Hardware error detected: Antenna Reflectivity High. Please check hardware - especially antenna connection and tuning - or call support");
        case "BOD":
          return new MetratecReaderException("Hardware error detected: Brownout detected. Please check hardware or call support");
        case "BOF":
          return new MetratecReaderException("Hardware error detected: Buffer overflow. Please check hardware or call support");
        case "CRT":
          return new MetratecReaderException("Hardware error detected: Command Receive Timeout. Please check hardware or call support");
        case "EHF":
          return new MetratecReaderException("Hardware error detected: Hardware Failure. Please check hardware or call support");
        case "PLE":
          return new MetratecReaderException("Hardware error detected: PLL Error. Please check hardware or call support");
        case "SRT":
          return new MetratecReaderException("Hardware error detected: Hardware Reset. Please check hardware or call support");
        case "UER":
          return new MetratecReaderException($"Hardware error detected: Unknown Error. Please check hardware or call support. Full error string: {response}");
        case "URE":
          return new MetratecReaderException("Hardware error detected: UART Receive Error. Please check hardware or call support");
        // class parser / dll error - produce exceptions / events in async cases
        case "UCO":
          return new MetratecReaderException("Command not supported (UCO)");
        case "DNS":
          return new MetratecReaderException($"Did Not Sleep ({response})");
        case "EDX":
          return new MetratecReaderException($"Error Decimal value eXpected ({response})");
        case "EHX":
          return new MetratecReaderException($"Error Hex value eXpected ({response})");
        case "NOR":
          return new MetratecReaderException($"Number Out of Range ({response})");
        case "NOS":
          return new MetratecReaderException($"Not Supported ({response})");
        case "NRF":
          return new MetratecReaderException($"No RF-Field active ({response})");
        case "NSS":
          return new MetratecReaderException($"No Standard selected ({response})");
        case "UPA":
          return new MetratecReaderException($"Unknown PArameter ({response})");
        case "WDL":
          return new MetratecReaderException($"Wrong Data Length ({response})");
        // class tag answer / communication problems - are reported as is
        case "ACE":
          return new MetratecReaderException($"ACcessError ({response})");
        case "CCE":
          return new MetratecReaderException($"Communication CRC Error ({response})");
        case "CER":
          return new MetratecReaderException($"CRC error ({response})");
        case "FLE":
          return new MetratecReaderException($"Fifo Length Error  ({response})");
        case "HBE":
          return new MetratecReaderException($"HeaderBit error ({response})");
        case "PDE":
          return new MetratecReaderException($"Preamble Detect Error ({response})");
        case "RDL":
          return new MetratecReaderException($"Read Data too Long  ({response})");
        case "RXE":
          return new MetratecReaderException($"Response length not as eXpected Error ({response})");
        case "TCE":
          return new MetratecReaderException($"Tag Communication Error ({response})");
        case "TMT":
          return new MetratecReaderException($"Too Many Tags ({response})");
        case "TOE":
          return new MetratecReaderException($"TimeOut Error ({response})");
        case "TOR":
        default:
          //PLE and SRT can be found anywhere in the string - the others are to be reported as they are
          if (response.Contains("PLE"))
            return new MetratecReaderException("Hardware error detected: PLL Error. Please check hardware or call support");
          if (response.Contains("SRT"))
            return new MetratecReaderException("Hardware error detected: Hardware Reset. Please check hardware or call support");
          break;
      }
      return new MetratecReaderException($"Unhandled Error: {response}");
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
      try
      {
        SetCommand($"HBT {(intervalInSec > 0 ? intervalInSec : "OFF")}");
        base.SetHeartBeatInterval(intervalInSec);
      }
      catch (Exception e)
      {
        if (e is MetratecReaderException)
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
    /// Update the the firmware name and version ({firmware} {version})
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected override void UpdateDeviceRevisions()
    {
      string response = ReadFirmware();
      if (response.Length > 3)
      {
        FirmwareName = response[..^4].Replace(" ", "");
        if (response.Length < 4)
        {
          throw new MetratecReaderException($"Invalid firmware response length: {response}");
        }
        if (!int.TryParse(response.Substring(response.Length - 4, 2), out var firmwareMajor) ||
            !int.TryParse(response.Substring(response.Length - 2, 2), out var firmwareMinor))
        {
          throw new MetratecReaderException($"Invalid firmware version format: {response}");
        }
        FirmwareMajorVersion = firmwareMajor;
        FirmwareMinorVersion = firmwareMinor;
        FirmwareVersion = $"{FirmwareMajorVersion}.{FirmwareMinorVersion}";

        response = ReadHardware();
        HardwareName = response[..^4];
        if (response.Length < 4)
        {
          throw new MetratecReaderException($"Invalid hardware response length: {response}");
        }
        if (!int.TryParse(response.Substring(response.Length - 4, 2), out var hardwareMajor) ||
            !int.TryParse(response.Substring(response.Length - 2, 2), out var hardwareMinor))
        {
          throw new MetratecReaderException($"Invalid hardware version format: {response}");
        }
        HardwareVersion = hardwareMajor + "." + hardwareMinor;
      }
      else
      {
        response = ReadRevision();
        if (response.Length > 3)
        {
          // return response.Substring(0, response.Length - 8) + response.Substring(response.Length - 4, 4);
          FirmwareName = response[..^8].Replace(" ", "");
          if (response.Length < 8)
          {
            throw new MetratecReaderException($"Invalid revision response length: {response}");
          }
          if (!int.TryParse(response.Substring(response.Length - 4, 2), out var revFirmwareMajor) ||
              !int.TryParse(response.Substring(response.Length - 2, 2), out var revFirmwareMinor) ||
              !int.TryParse(response.Substring(response.Length - 8, 2), out var revHardwareMajor) ||
              !int.TryParse(response.Substring(response.Length - 6, 2), out var revHardwareMinor))
          {
            throw new MetratecReaderException($"Invalid revision format: {response}");
          }
          FirmwareMajorVersion = revFirmwareMajor;
          FirmwareMinorVersion = revFirmwareMinor;
          FirmwareVersion = $"{FirmwareMajorVersion}.{FirmwareMinorVersion}";
          HardwareName = response[..^8];
          HardwareVersion = $"{revHardwareMajor}.{revHardwareMinor}";
        }
        else
        {
          FirmwareName = "Unknown";
          FirmwareVersion = "0.0";
          HardwareName = "Unknown";
          HardwareVersion = "0.0";
        }
      }
      SerialNumber = ReadSerialNumber();
    }
    /// <returns>the firmware information or 'UCO' if not available</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected virtual string ReadFirmware()
    {
      return GetCommand("RFW");
    }
    /// <returns>the hardware information or 'UCO' if not available</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected virtual string ReadHardware()
    {
      return GetCommand("RHW");
    }
    /// <returns>the firmware and hardware information or 'UCO' if not available</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected virtual string ReadRevision()
    {
      return GetCommand("REV");
    }
    /// <returns>the serial number</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected virtual string ReadSerialNumber()
    {
      return GetCommand("RSN");
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
      InputChangedEventArgs args = new(inputPin, isHigh, DateTime.Now);
      ThreadPool.QueueUserWorkItem(o => InputChanged.Invoke(this, args));
    }
    /// <summary>
    /// Set the verbosity level
    /// </summary>
    /// <param name="level">
    /// 0.. Only necessary data (EPC, User Data, etc.) is returned. 
    /// 1..Default, most tag communication errors added.
    /// 2: All tag communication errors including RXE and CRE normally indicating a collision are send.
    /// </param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected void SetVerbosityLevel(int level)
    {
      SetCommand($"VBL {level}");
    }

    #endregion Protected Method

    #region Private Methods

    /// <summary>
    /// Method to compute CRC (starting value 0xFFFF, 8408 polynomial)
    /// </summary>
    /// <param name="toCompute">
    /// The string over which the CRC is to be computed
    /// </param>
    /// <returns>
    /// A string containing the CRC as 4 hex digits
    /// </returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>

    internal static string ComputeCRC(string toCompute)
    {
      if (toCompute == null)
        throw new MetratecReaderException("The string passed to the CRC computation function was null");
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
      if (toCheck.Length < 4)
      {
        return false;
      }
      return toCheck.Substring(toCheck.Length - 4, 4) == ComputeCRC(toCheck[..^4]);
    }
  }

  #endregion Private Methods
}