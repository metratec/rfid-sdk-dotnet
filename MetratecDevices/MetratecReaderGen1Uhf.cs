using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The reader class for the ASCII based metratec reader
  /// </summary>
  public class UhfReaderGen1 : MetratecReaderGen1<UhfTag>
  {
    private List<UhfTag>? _lastInventory = null;

    private bool _addEPC = false;
    private bool _addTRS = false;
    private MEMBANK_GEN1 _lastMemoryCall;
    private bool _parseMemory = false;
    private bool _inventoryIsEvent = true;
    private bool _continuousStarted = false;
    private bool _busy = false;
    private int _dataStartAddress;
    private string _dataToWrite = "";
    private REGION_GEN1 _rfidStandard;

    /// <summary>
    /// Input change event handler
    /// </summary>
    public event EventHandler<InputChangedEventArgs>? InputChanged;

    /// <summary>
    /// The reader class for all Metratec reader
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="mode">The rfid standard to use. Defaults to ETSI</param>
    public UhfReaderGen1(ICommunicationInterface connection, REGION_GEN1 mode = REGION_GEN1.ETS) : base(connection)
    {
      _rfidStandard = mode;
    }

    /// <summary>
    /// The reader class for all Metratec reader
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="mode">The rfid standard to use. Defaults to ETSI</param>
    public UhfReaderGen1(ICommunicationInterface connection, ILogger logger, REGION_GEN1 mode = REGION_GEN1.ETS) : base(connection, logger)
    {
      _rfidStandard = mode;
    }

    /// <summary>
    /// The reader class for all Metratec reader
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="id">The reader id</param>
    /// <param name="mode">The rfid standard to use. Defaults to ETSI</param>
    public UhfReaderGen1(ICommunicationInterface connection, string id, REGION_GEN1 mode = REGION_GEN1.ETS) : base(connection, id)
    {
      _rfidStandard = mode;
    }

    /// <summary>
    /// The reader class for all Metratec reader
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="id">The reader id</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="mode">The rfid standard to use. Defaults to ETSI</param>
    public UhfReaderGen1(ICommunicationInterface connection, string id, ILogger logger, REGION_GEN1 mode = REGION_GEN1.ETS) : base(connection, id, logger)
    {
      _rfidStandard = mode;
    }

    /// <summary>
    /// Configure the reader.
    /// The base implementation must be called after success.
    /// </summary>
    protected override void ConfigureReader()
    {
      SetVerbosityLevel(1); // default
      SetRegion(_rfidStandard);
      EnableAdditionalEPC(true);
      EnableAdditionalTRS(true);
      if (FirmwareName!.ToLower().Contains("pulsar"))
        EnableInputEvents(true);
      EnableRfField(true);
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

    /// <summary>
    /// Process the reader response...override for event check
    /// The base implementation must be called after success.
    /// </summary>
    /// <param name="response">a reader response</param>
    protected override void HandleResponse(string response)
    {
      switch (response[0])
      {
        case 'I':
          if (response[1] == 'N')
          {
            // input event: IN0 IN1 
            bool state = response.Contains("HI!");
            FireInputChangeEvent(int.Parse(response[2].ToString()), state);
            return;
          }
          break;
      }
      base.HandleResponse(response);
    }

    /// <summary>
    /// Handle an inventory response
    /// </summary>
    /// <param name="response"></param>
    protected override void HandleInventoryResponse(string response)
    {
      Logger.LogTrace("Handle Inventory - {}", response);
      _lastInventory = ParseInventory(SplitResponse(response), new DateTime());
      if (_inventoryIsEvent && _lastInventory.Count > 0)
      {
        FireInventoryEvent(_lastInventory, _continuousStarted);
      }
    }

    private List<UhfTag> ParseInventory(string[] answers, DateTime timestamp)
    {
      List<UhfTag> tags = new();
      UhfTag? tag = null;
      for (int i = 0; i < answers.Length; i++)
      {
        String s = answers[i];
        if (3 >= s.Length || s[3] == ' ')
        {
          // System.err.println(s);
          switch (s[0])
          {
            case '-':
              // should not happen
              tag!.RSSI = int.Parse(s);
              continue;
            case 'A':
              if (s.StartsWith("ARP"))
              {
                // RESPONSE_ANTENNA_REPORT
                CurrentAntennaPort = int.Parse(s[4..]);
                foreach (UhfTag item in tags)
                {
                  item.Antenna = CurrentAntennaPort;
                }
                continue;
              }
              break;
            case 'C':
              if (s.StartsWith("CER"))
              {
                //RESPONSE_ERROR_CER
                Logger.LogTrace("Get Inventory: CRC error");
              }
              else
              {
                throw ParseErrorResponse(s);
              }
              break;
            case 'F':
              if (s.StartsWith("FLE"))
              {
                // FLE per tag, all other tags not korrupt
                Logger.LogTrace("Get Inventory: Fifo length error");
              }
              else
              {
                throw ParseErrorResponse(s);
              }
              break;
            case 'H':
              if (s.StartsWith("HBE"))
              {
                Logger.LogTrace("Get Inventory: HBE");
              }
              else
              {
                throw ParseErrorResponse(s);
              }
              break;
            case 'I':
              if (s.StartsWith("IVF"))
              {
                return tags;
              }
              else
              {
                throw ParseErrorResponse(s);
              }
            case 'O':
              if (s.StartsWith("OK!"))
              {
                // was a write data command...and this tag was ok...replace the OK! with a string
                // greater than three and rerun
                answers[i] = _dataToWrite;
                --i;
                continue;
              }
              else
              {
                throw ParseErrorResponse(s);
              }
            case 'P':
              if (s.StartsWith("PDE"))
              {
                Logger.LogTrace("Get Inventory: PDE error");
              }
              else
              {
                throw ParseErrorResponse(s);
              }
              break;
            case 'R':
              if (s.StartsWith("RXE"))
              {
                Logger.LogTrace("Get Inventory: RXE error");
              }
              else if (s.StartsWith("RDL"))
              {
                Logger.LogTrace("Get Inventory: RDL error");
              }
              else
              {
                throw ParseErrorResponse(s);
              }
              break;
            case 'T': // TCE TOE TOR
              if (s.StartsWith("TOE"))
              {
                Logger.LogTrace("Get Inventory: Time out error");
              }
              else if (s.StartsWith("TCE"))
              {
                Logger.LogTrace("Get Inventory: Tag Communication Error");
              }
              else if (s.StartsWith("TOR"))
              {
                Logger.LogTrace("Get Inventory: Tag Out of Range");
              }
              else
              {
                throw ParseErrorResponse(s);
              }
              break;
            default:
              throw ParseErrorResponse(s);
          }
          // ignore following EPC and TRS
          if (_addEPC)
          {
            i++;
          }
          if (_addTRS)
          {
            i++;
          }
        }
        else
        {
          tag = new UhfTag(timestamp, CurrentAntennaPort);
          if (_addEPC)
          {
            tag.EPC = answers[++i];
          }
          if (_addTRS)
          {
            tag.RSSI = int.Parse(answers[++i]);
          }
          tags.Add(tag);
          if (_parseMemory)
          {
            switch (_lastMemoryCall)
            {
              case MEMBANK_GEN1.EPC:
              case MEMBANK_GEN1.USR:
                tag.Data = s;
                tag.DataStartAddress = _dataStartAddress;
                break;
              case MEMBANK_GEN1.TID:
                tag.TID = s;
                break;
              case MEMBANK_GEN1.ACP:
                tag.Data = s;
                break;
              case MEMBANK_GEN1.KLP:
                tag.Data = s;
                break;
              case MEMBANK_GEN1.RES:
                break;
              default:
                throw new InvalidOperationException("Inventory response but no membank are set");
            }
          }
        }
      }
      return tags;
    }

    /**
   * prepare the input for event handling
   * 
   * @throws CommConnectionException if an error occurs
   * @throws RFIDReaderException if an error occurs
   */
    protected void EnableInputEvents(bool enable = true)
    {
      if (enable)
      {
        SetCommand("SEC COMM 0 #SPIN0#RIP 0");
        SetCommand("SEC EDGE 0 BOTH");
        SetCommand("SEC COMM 1 #SPIN1#RIP 1");
        SetCommand("SEC EDGE 1 BOTH");
      }
      else
      {
        SetCommand("SEC EDGE 0 NONE");
        SetCommand("SEC EDGE 1 NONE");
      }
    }

    /// <summary>
    /// Set the rfid region standard to use
    /// </summary>
    /// <param name="standard">the rfid region standard</param>
    public void SetRegion(REGION_GEN1 standard)
    {
      SetCommand($"STD {standard}");
      _rfidStandard = standard;
    }
    /// <summary>
    /// Configure the expected numbers of transponders in the field
    /// </summary>
    /// <param name="tag_count">expected numbers of transponders</param>
    public void SetTagCount(int tag_count)
    {
      int q = 0;
      while (tag_count > Math.Pow(2, q))
      {
        q++;
      }
      SetCommand($"SQV {q}");
    }
    /// <summary>
    /// Enable to add the optional EPC values to every transponder response.
    /// </summary>
    /// <param name="enable">Set to true for add the epc to every transponder response</param>
    protected void EnableAdditionalEPC(bool enable)
    {
      SetCommand($"SET EPC {(enable ? "ON" : "OFF")}");
      _addEPC = enable;
    }

    /// <summary>
    /// Enable to add the optional RSSI values to the founded transponder.
    /// </summary>
    /// <param name="enable">Set to true for enable the additional RSSI value</param>
    protected void EnableAdditionalTRS(bool enable)
    {
      SetCommand($"SET TRS {(enable ? "ON" : "OFF")}");
      _addTRS = enable;
    }

    /// <summary>
    /// Enable or disable the rf field
    /// </summary>
    /// <param name="enable">Set to true for enable the rf field</param>
    protected void EnableRfField(bool enable)
    {
      SetCommand($"SRI {(enable ? "ON" : "OFF")}");
    }

    /// <summary>
    /// Timer controlled RF field reset. Turns the field off, waits for the specified number of ms and
    /// then turns the field back on. Can be useful to reset all tags in the field without managing
    /// everything
    /// </summary>
    /// <param name="delayInMilliseconds">field off time in milliseconds</param>
    protected void ResetRfField(int delayInMilliseconds)
    {
      SetCommand($"SRI TIM {delayInMilliseconds}");
    }

    /// <summary>
    /// If enable the reader will switch off the power amplifier automatically after every tag operation starts
    /// </summary>
    /// <param name="enable">Set to true for enable the power save</param>
    protected void EnablePowerSave(bool enable)
    {
      SetCommand($"SRI SPM {(enable ? "ON" : "OFF")}");
    }

    /// <summary>
    /// Set the reader power
    /// </summary>
    /// <param name="power">the reader power [-2, 27]</param>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    public override void SetPower(int power)
    {
      SetCommand($"CFG PWR {power}");
    }

    /// <summary>
    /// Scan for the current inventory
    /// </summary>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    public override List<UhfTag> GetInventory()
    {
      return GetInventory(false, false);
    }

    /// <summary>
    /// Scan for the current inventory
    /// </summary>
    /// <param name="singleTag">Set to true if only one tag is expected. Throws an error if more tags are found</param>
    /// <param name="onlyNewTags">Find each tag only once as long as it stays powered within the rf field</param>
    /// <returns>List with the processed tags</returns>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    public List<UhfTag> GetInventory(bool singleTag, bool onlyNewTags)
    {
      return GetTransponderResponses(MEMBANK_GEN1.EPC, $"INV{(singleTag ? " SSL" : "")}{(onlyNewTags ? " ONT" : "")}", true, false);
    }

    private List<UhfTag> GetTransponderResponses(MEMBANK_GEN1 membank, String command, bool fireEvent, bool parseMemory)
    {
      if (_busy)
      {
        throw new InvalidOperationException("Reader is busy");
      }
      try
      {
        _busy = true;
        _lastMemoryCall = membank;
        _parseMemory = parseMemory;
        _lastInventory = null;
        _inventoryIsEvent = fireEvent;
        SendCommand(command);
        DateTime start = DateTime.Now;
        while (DateTime.Now.Subtract(start).TotalMilliseconds < ResponseTimeout)
        {
          if (null == _lastInventory)
          {
            Thread.Sleep(20);
          }
          else
          {
            return _lastInventory;
          }
        }
        if (!Connected)
          throw new ObjectDisposedException("Not connected");
        throw new TimeoutException("Response timeout");
      }
      finally
      {
        _busy = false;
      }
    }

    /// <summary>
    /// Starts the continuous inventory scan.
    /// If the inventory event handler is set, any transponder found will be delivered via it. 
    /// If the event handler is not set, the found transponders can be fetched
    /// via the method FetchInventory
    /// event listener is set. 
    /// </summary>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    public override void StartInventory()
    {
      StartInventory(false, false);
    }

    /// <summary>
    /// Starts the continuous inventory scan.
    /// If the inventory event handler is set, any transponder found will be delivered via it. 
    /// If the event handler is not set, the found transponders can be fetched
    /// via the method FetchInventory
    /// event listener is set. 
    /// </summary>
    /// <param name="singleTag">Set to true if only one tag is expected. Throws an error if more tags are found</param>
    /// <param name="onlyNewTags">Find each tag only once as long as it stays powered within the rf field</param>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    public void StartInventory(bool singleTag, bool onlyNewTags)
    {
      if (_busy)
      {
        throw new InvalidOperationException("Reader is busy");
      }
      _busy = true;
      _parseMemory = false;
      SendCommand($"CNR INV{(singleTag ? " SSL" : "")}{(onlyNewTags ? " ONT" : "")}");
      _continuousStarted = true;
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
      base.StopInventory();
      _busy = false;
      _continuousStarted = false;
    }
    /// <summary>
    /// Most tags manipulation instruction can be limited to a population of tags with certain data
    /// values, e.g. tags that start with a certain EPC, a certain TID or even contain certain data in
    /// the user memory. This is done via a mask given with each command. Using this feature you can
    /// address certain tags in the field with directly accessing each tag via its TID or EPC.
    /// For setting the a epc mask, the start should be 32.
    /// </summary>
    /// <param name="membank">membank from the chip</param>
    /// <param name="mask">mask value, hex string</param>
    /// <param name="startBit">start address in bit, default = 0</param>
    /// <param name="bitLength">length in bit (max is 248 Bits (31 Byte)). Default = Length of Mask Value (full Nibbles)</param>
    public void SetMask(MEMBANK_GEN1 membank, String mask, int startBit = -1, int bitLength = -1)
    {
      switch (membank)
      {
        case MEMBANK_GEN1.EPC:
        case MEMBANK_GEN1.TID:
        case MEMBANK_GEN1.USR:
          break;
        default:
          throw new InvalidOperationException($"Membank {membank} not allowed for mask");
      }
      SetCommand($"SET MSK {membank} {mask}{(startBit >= 0 ? $" {startBit:X2}" : "")}{(bitLength >= 0 ? $" {bitLength:X2}" : "")}");
    }

    /// <summary>
    /// Disable the current mask
    /// </summary>
    public void ResetMask()
    {
      SetCommand("SET MSK OFF");
    }

    /// <summary>
    /// Set a EPC mask
    /// </summary>
    /// <param name="mask">mask value, hex string</param>
    public void SetEpcMask(string mask)
    {
      SetMask(MEMBANK_GEN1.EPC, mask, 32);
    }

    /// <summary>
    /// Reader the data from tags
    /// </summary>
    /// <param name="membank">The tag memory bank to use</param>
    /// <param name="startAddress">the start address</param>
    /// <param name="words">number of words to read</param>
    /// <param name="singleTag">When set to true, only one tag is expected. Defaults to False.</param>
    /// <returns>List with the processed tags</returns>
    public List<UhfTag> ReadTagData(MEMBANK_GEN1 membank, int startAddress, int words, bool singleTag = false)
    {
      _dataStartAddress = startAddress;
      return GetTransponderResponses(membank, $"RDT{(singleTag ? " SSL" : "")} {membank} {startAddress:X} {words:X}", false, true);
    }

    /// <summary>
    /// Reader the user data from tags
    /// </summary>
    /// <param name="startAddress">the start address</param>
    /// <param name="words">number of words to read</param>
    /// <param name="singleTag">When set to true, only one tag is expected. Defaults to False.</param>
    /// <returns>List with the processed tags</returns>
    public List<UhfTag> ReadTagUsrData(int startAddress, int words, bool singleTag = false)
    {
      return ReadTagData(MEMBANK_GEN1.USR, startAddress, words, singleTag);
    }

    /// <summary>
    /// Reader the user data from tags
    /// </summary>
    /// <param name="startAddress">the start address</param>
    /// <param name="words">number of words to read</param>
    /// <param name="singleTag">When set to true, only one tag is expected. Defaults to False.</param>
    /// <returns>List with the processed tags</returns>
    public List<UhfTag> ReadTagTid(int startAddress, int words, bool singleTag = false)
    {
      return ReadTagData(MEMBANK_GEN1.TID, startAddress, words, singleTag);
    }

    /// <summary>
    /// Writes data to tags
    /// </summary>
    /// <param name="membank">The tag memory bank to use</param>
    /// <param name="startAddress">the start address</param>
    /// <param name="data">data to write, hex string</param>
    /// <param name="singleTag">When set to true, only one tag is expected. Defaults to False.</param>
    /// <returns>List with the processed tags</returns>
    public List<UhfTag> WriteTagData(MEMBANK_GEN1 membank, int startAddress, string data, bool singleTag = false)
    {
      _dataStartAddress = startAddress;
      _dataToWrite = data;

      return GetTransponderResponses(membank, $"WDT{(singleTag ? " SSL" : "")} {membank} {startAddress:X} {data}", false, true);

    }

    /// <summary>
    /// Writes data to the tags user memory
    /// </summary>
    /// <param name="startAddress">the start address</param>
    /// <param name="data">data to write, hex string</param>
    /// <param name="singleTag">When set to true, only one tag is expected. Defaults to False.</param>
    /// <returns>List with the processed tags</returns>
    public List<UhfTag> WriteTagUsrData(int startAddress, string data, bool singleTag = false)
    {
      return WriteTagData(MEMBANK_GEN1.USR, startAddress, data, singleTag);
    }
    /// <summary>
    /// write the epc memory of the found transponder
    /// </summary>
    /// <param name="newEPC">the new epc - the length must be a multiple of 4</param>
    /// <param name="singleTag">When set to true, only one tag is expected. Defaults to False.</param>
    /// <returns>List with the processed tags</returns>
    public List<UhfTag> WriteTagEpc(string newEPC, bool singleTag = false)
    {
      if (0 != newEPC.Length % 4)
      {
        throw new InvalidOperationException("The new epc length must be a multiple of 4");
      }
      // prepare new data block 01 with the epc length
      int tagIDWords = newEPC.Length / 4;
      int block01 = tagIDWords / 2 << 12;
      if (1 == tagIDWords % 2)
      {
        block01 |= 0x0800;
      }
      // get the old block 01
      List<UhfTag> tags = ReadTagData(MEMBANK_GEN1.EPC, 1, 1, singleTag);
      if (tags.Count == 0)
      {
        // no tags in field
        return new List<UhfTag>();
      }
      // check if the oldBlock01 is all the same for the tags in field
      int oldBlock01 = -1;
      foreach (UhfTag tag in tags)
      {
        int data = int.Parse(tag.Data!, System.Globalization.NumberStyles.HexNumber) & 0x7ff;
        if (-1 == oldBlock01)
        {
          oldBlock01 = data;
        }
        else if (oldBlock01 != data)
        {
          throw new InvalidOperationException("Different tags are in the field, which would result in data loss when writing. Please edit individually.");
        }
      }
      // copy old block data into the new block 01 data
      block01 |= oldBlock01;
      String block01Hex = block01.ToString("X");
      if (4 > block01Hex.Length)
      {
        block01Hex = "0" + block01Hex;
      }
      tags = WriteTagData(MEMBANK_GEN1.EPC, 1, block01Hex + newEPC);
      if (tags.Count != 0)
      {
        // the epc is written by at least one tag, reset the RF field to also reset the tags
        ResetRfField(50);
        Thread.Sleep(50);
      }
      return tags;
    }

    /// <summary>
    /// Sets the access password for authenticated access
    /// </summary>
    /// <param name="password">8 characters long hexadecimal password (32bit access code)</param>
    public void SetAccessPassword(string password)
    {
      SetCommand($"SET ACP {password}");
    }
    /// <summary>
    /// Disable the current access password
    /// </summary>
    public void DisableAccessPassword()
    {
      SetCommand("SET ACP OFF");
    }
    /// <summary>
    /// Storing the access password in a non-volatile memory of the reader for later use. 
    /// (So you do not have to transmit it over an unsecure line later)
    /// </summary>
    /// <param name="slot">slot number [0,7]</param>
    /// <param name="password">8 characters long hexadecimal password (32bit access code)</param>
    public void SaveAccessPassword(int slot, string password)
    {
      SetCommand($"SET APS {password} {slot}");
    }
    /// <summary>
    /// Load a stored access password from a non-volatile storage location. This is useful
    /// for higher security as the password is not sent over an insecure line.
    /// </summary>
    /// <param name="slot"></param>
    public void LoadAccessPassword(int slot)
    {
      SetCommand($"SET APL {slot}");
    }
    /// <summary>
    /// read the access password of the found transponder
    /// </summary>
    /// <param name="singleTag">When set to true, only one tag is expected. Defaults to False.</param>
    /// <returns>List with the processed tags</returns>
    public List<UhfTag> ReadTagAccessPassword(bool singleTag = false)
    {
      return GetTransponderResponses(MEMBANK_GEN1.ACP, $"RDT{(singleTag ? " SSL" : "")} ACP", false, true);
    }
    /// <summary>
    /// Write the access password of the found transponder
    /// </summary>
    /// <param name="password">8 characters long hexadecimal password (32bit access code)</param>
    /// <param name="singleTag">When set to true, only one tag is expected. Defaults to False.</param>
    /// <returns>List with the processed tags</returns>
    public List<UhfTag> WriteTagAccessPassword(string password, bool singleTag = false)
    {
      return GetTransponderResponses(MEMBANK_GEN1.ACP, $"WDT{(singleTag ? " SSL" : "")} ACP {password}", false, true);
    }
    /// <summary>
    /// The Lock command is used to set the access rights of the different data blocks, including
    /// the access password itself and the kill password. To use this command you have to be in the
    /// secured state (i.e. authenticated yourself with the correct password).
    /// </summary>
    /// <param name="membank">Memory bank to lock. Available: ["EPC", "TID", "USR", "ACP", "KLP"].</param>
    /// <param name="mode">mode (int):
    /// '0' data is writeable and readable in any case.
    /// '1' data is writeable and readable and may never be locked.
    /// '2' data is only writeable and readable in secured state.
    /// '3' data is not writeable or readable.
    /// Note: The 'EPC', 'TIC', 'USR' memory are in any case readable.</param>
    /// <param name="singleTag">When set to true, only one tag is expected. Defaults to False.</param>
    /// <returns>List with the processed tags</returns>
    public List<UhfTag> LockTagMemory(MEMBANK_GEN1 membank, int mode, bool singleTag = false)
    {
      return GetTransponderResponses(membank, $"LCK{(singleTag ? " SSL" : "")} {membank} {mode}", false, true);
    }
    /// <summary>
    /// Lock the tag epc memory. To use this command you have to be in the
    /// secured state (i.e. authenticated yourself with the correct password).
    /// </summary>
    /// <param name="mode">mode (int):
    /// '0' epc is writeable and readable in any case.
    /// '1' epc is writeable and readable and may never be locked.
    /// '2' epc is only writeable and readable in secured state.
    /// '3' epc is not writeable or readable.</param>
    /// <param name="singleTag">When set to true, only one tag is expected. Defaults to False.</param>
    /// <returns>List with the processed tags</returns>
    public List<UhfTag> LockTagEPC(int mode, bool singleTag = false)
    {
      return LockTagMemory(MEMBANK_GEN1.EPC, mode, singleTag);
    }
    /// <summary>
    /// Lock the tag data memory. To use this command you have to be in the
    /// secured state (i.e. authenticated yourself with the correct password).
    /// </summary>
    /// <param name="mode">mode (int):
    /// '0' data is writeable and readable in any case.
    /// '1' data is writeable and readable and may never be locked.
    /// '2' data is only writeable and readable in secured state.
    /// '3' data is not writeable or readable.</param>
    /// <param name="singleTag">When set to true, only one tag is expected. Defaults to False.</param>
    /// <returns>List with the processed tags</returns>
    public List<UhfTag> LockTagData(int mode, bool singleTag = false)
    {
      return LockTagMemory(MEMBANK_GEN1.USR, mode, singleTag);
    }
    /// <summary>
    /// Lock the tag access password memory. To use this command you have to be in the
    /// secured state (i.e. authenticated yourself with the correct password).
    /// </summary>
    /// <param name="mode">mode (int):
    /// '0' access password is writeable and readable in any case.
    /// '1' access password is writeable and readable and may never be locked.
    /// '2' access password is only writeable and readable in secured state.
    /// '3' access password is not writeable or readable.</param>
    /// <param name="singleTag">When set to true, only one tag is expected. Defaults to False.</param>
    /// <returns>List with the processed tags</returns>
    public List<UhfTag> LockTagAccessPassword(int mode, bool singleTag = false)
    {
      return LockTagMemory(MEMBANK_GEN1.ACP, mode, singleTag);
    }
    /// <summary>
    /// Lock the tag kill password memory. To use this command you have to be in the
    /// secured state (i.e. authenticated yourself with the correct password).
    /// </summary>
    /// <param name="mode">mode (int):
    /// '0' kill password is writeable and readable in any case.
    /// '1' kill password is writeable and readable and may never be locked.
    /// '2' kill password is only writeable and readable in secured state.
    /// '3' kill password is not writeable or readable.</param>
    /// <param name="singleTag">When set to true, only one tag is expected. Defaults to False.</param>
    /// <returns>List with the processed tags</returns>
    public List<UhfTag> LockTagKillPassword(int mode, bool singleTag = false)
    {
      return LockTagMemory(MEMBANK_GEN1.KLP, mode, singleTag);
    }
    /// <summary>
    /// Set the kill password. For further details on this topic please refer to the
    /// EPC Gen 2 Protocol Description and the kill command. The default kill password is 00000000
    /// </summary>
    /// <param name="password">8 characters long hexadecimal password (32bit access code)</param>
    public void SetKillPassword(string password)
    {
      SetCommand($"SET KLP {password}");
    }
    /// <summary>
    /// Storing the kill password in a non-volatile memory of the reader for later use. 
    /// (So you do not have to transmit it over an unsecure line later)
    /// </summary>
    /// <param name="slot">slot number [0,7]</param>
    /// <param name="password">8 characters long hexadecimal password (32bit access code)</param>
    public void SaveKillPassword(int slot, string password)
    {
      SetCommand($"SET KPS {password} {slot}");
    }
    /// <summary>
    /// Load a stored kill password from a non-volatile storage location. This is useful
    /// for higher security as the password is not sent over an insecure line.
    /// </summary>
    /// <param name="slot"></param>
    public void LoadKillPassword(int slot)
    {
      SetCommand($"SET KPL {slot}");
    }
    /// <summary>
    /// Read the kill password of the found transponder
    /// </summary>
    /// <param name="singleTag">When set to true, only one tag is expected. Defaults to False.</param>
    /// <returns>List with the processed tags</returns>
    public List<UhfTag> ReadTagKillPassword(bool singleTag = false)
    {
      return GetTransponderResponses(MEMBANK_GEN1.KLP, $"RDT{(singleTag ? " SSL" : "")} KPL", false, true);
    }
    /// <summary>
    /// Write the kill password of the found transponder
    /// </summary>
    /// /// <param name="password">8 characters long hexadecimal password (32bit access code)</param>
    /// <param name="singleTag">When set to true, only one tag is expected. Defaults to False.</param>
    /// <returns>List with the processed tags</returns>
    public List<UhfTag> WriteTagKillPassword(string password, bool singleTag = false)
    {
      return GetTransponderResponses(MEMBANK_GEN1.KLP, $"WDT{(singleTag ? " SSL" : "")} KLP {password}", false, true);
    }
    /// <summary>
    /// Use to disable UHF Gen2 tags forever. Please set the kill password before use this command.
    /// ATTENTION If you use this command incorrectly you can irreversibly kill a big number of UHF tags in a very short time
    /// </summary>
    /// <param name="singleTag">When set to true, only one tag is expected. Defaults to False.</param>
    /// <returns>List with the processed tags</returns>
    public List<UhfTag> KillTags(bool singleTag = false)
    {
      return GetTransponderResponses(MEMBANK_GEN1.KLP, $"KIL{(singleTag ? " SSL" : "")}", false, true);
    }
  }

  /// <summary>
  /// RFID communication standart
  /// </summary>
  public enum REGION_GEN1
  {

    /// <summary>
    /// European standard
    /// </summary>
    ETS,
    /// <summary>
    /// Israel standard
    /// </summary>
    ISR,
    /// <summary>
    /// US standard
    /// </summary>
    FCC,
  }

  /// <summary>
  /// Tag memory
  /// </summary>
  public enum MEMBANK_GEN1
  {
    /// <summary>
    /// The EPC membank. Contains CRC, PC and EPC.
    /// </summary>
    EPC,
    /// <summary>
    /// The tag ID of the tag (sometimes contains a unique ID, sometimes only a manufacturer code,
    /// depending on the tag type)
    /// </summary>
    /**
     * 
     */
    TID,
    /// <summary>
    /// The optional user memory some tags have
    /// </summary>
    USR,
    /// <summary>
    /// Reserved membank. Contains Kill password and Access password
    /// </summary>
    RES,
    /// <summary>
    /// Access password
    /// </summary>
    ACP,
    /// <summary>
    /// Kill password
    /// </summary>
    KLP,
  }

}