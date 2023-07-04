using CommunicationInterfaces;
using Microsoft.Extensions.Logging;

namespace MetraTecDevices
{
  /// <summary>
  /// The reader class for the ASCII based metratec reader
  /// </summary>
  public class HfReaderGen1 : MetratecReaderGen1<HfTag>
  {
    private List<HfTag>? _lastInventory = null;
    private bool _continuousStarted = false;
    
    private HfTag? _lastRequest = null;
    // private RfInterfaceMode _mode = RfInterfaceMode.SingleSubcarrier_100percentASK;

    private bool _isSingleSubcarrier = true;
    /// <summary>
    /// The reader class for all Metratec reader
    /// </summary>
    /// <param name="connection">The connection interface</param>
    public HfReaderGen1(ICommunicationInterface connection) : base(connection)
    {
    }

    /// <summary>
    /// The reader class for all Metratec reader
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    public HfReaderGen1(ICommunicationInterface connection, ILogger logger) : base(connection, logger)
    {
    }

    /// <summary>
    /// The reader class for all Metratec reader
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="id">The reader id</param>
    public HfReaderGen1(ICommunicationInterface connection, string id) : base(connection, id)
    {
    }

    /// <summary>
    /// The reader class for all Metratec reader
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="id">The reader id</param>
    /// <param name="logger">The connection interface</param>
    public HfReaderGen1(ICommunicationInterface connection, string id, ILogger logger) : base(connection, id, logger)
    {
    }

    /// <summary>
    /// Configure the reader.
    /// The base implementation must be called after success.
    /// </summary>
    protected override void ConfigureReader()
    {
      SetVerbosityLevel(2);
      SetCommand("MOD 156");
      EnableRfInterface();
    }

    /// <summary>
    /// Process the reader response...override for event check
    /// The base implementation must be called after success.
    /// </summary>
    /// <param name="response">a reader response</param>
    protected override void HandleResponse(string response)
    {
      // check for special hf events
      if (response[0] == 'T' && (response[1] == 'D' || response[1] == 'N'))
      {
        // TDT TND
        HandleRequestResponse(response);
        return;
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
      string[] answers = SplitResponse(response);
      DateTime timestamp = DateTime.Now;
      List<HfTag> tags = new();
      for (int i = 0; i < answers.Length; i++)
      {
        String s = answers[i];
        if (s.Length <= 3)
        {
          /*
           * check error codes - if it is a single tag error - ignore the error for this tag Error
           * codes to ignore: CER, FLE, RDL, TCE, TOE (see 'ISO 15693 Protocol Guide', Chapter Error
           * Codes)
           */
          if (s.StartsWith("CER") || s.StartsWith("RXE") || s.StartsWith("TOE") || s.StartsWith("FLE")
              || s.StartsWith("RDL") || s.StartsWith("TCE"))
          {
            Logger.LogDebug("receive inventory error code - {}", s);
          }
          else
          {
            throw new InvalidOperationException(s);
          }
        }
        else if (s.StartsWith("ARP"))
        {
          CurrentAntennaPort = int.Parse(s[4..]);
          foreach (HfTag tag in tags)
          {
            tag.Antenna = CurrentAntennaPort;
          }
          continue;
        }
        else if (s.StartsWith("IVF"))
        {
          break;
        }
        else
        {
          tags.Add(new HfTag(s, timestamp, CurrentAntennaPort));
        }
      }
      _lastInventory = tags;
      FireInventoryEvent(_lastInventory, _continuousStarted);
    }

    /// <summary>
    /// request response event handler
    /// </summary>
    public event EventHandler<NewRequestResponseArgs>? NewRequestResponse;

    /// <summary>
    /// Fire a inventory event
    /// </summary>
    /// <param name="tag">the founded tags</param>
    protected void FireRequestResponse(HfTag tag)
    {
      if (null == NewRequestResponse)
        return;
      NewRequestResponseArgs args = new(tag, new DateTime());
      NewRequestResponse(this, args);
    }

    private void HandleRequestResponse(string response)
    {
      // TDT<CR>0011112222B7DD<CR>COK<CR>NCL<CR>
      // TNR<CR>
      Logger.LogTrace("Handle request response - {}", response);
      string[] answers = SplitResponse(response);
      HfTag tag = new(DateTime.Now, CurrentAntennaPort);
      if (answers.Last().StartsWith("ARP"))
      {
        tag.Antenna = int.Parse(answers.Last()[4..]);
        Array.Resize(ref answers, answers.Length - 1);
      }
      if (answers.Last().Equals("NCL"))
      {
        if (answers[2].Equals("COK"))
        {
          if (answers[1].StartsWith("00"))
          {
            tag.Data = answers[1][2..^4];
          }
          else
          {
            tag.HasError = true;
            tag.Message = $"TEC {answers[1][2..4]}";
          }
        }
        else
        {
          tag.HasError = true;
          tag.Message = answers[2];
        }
      }
      else
      {
        // CDT - Collision detect
        // TNR - Tag not responding - no tag
        // RDL - read data too long
        tag.HasError = true;
        tag.Message = answers.Last();
      }
      _lastRequest = tag;
      FireRequestResponse(_lastRequest);
    }

    /// <summary>
    /// Set the reader power
    /// </summary>
    /// <param name="power">the reader power (100, 200)</param>
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
      if (FirmwareMajorVersion >= 3)
      {
        SetCommand($"SET PWR {power}");
      }
      else
      {
        throw new InvalidOperationException($"Firmware Version {FirmwareVersion} does not support power settings");
      }
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
    public override List<HfTag> GetInventory()
    {
      return GetInventory(false, false, 0);
    }

    /// <summary>
    /// Scan for the current inventory
    /// </summary>
    /// <param name="afi">The Application Family Identifier group the tag has to belong to to be read</param>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    public List<HfTag> GetInventory(int afi)
    {
      return GetInventory(false, false, afi);
    }

    /// <summary>
    /// Scan for the current inventory
    /// </summary>
    /// <param name="singleTag">Set to true if only one tag is expected. Throws an error if more tags are found</param>
    /// <param name="onlyNewTags">Find each tag only once as long as it stays powered within the rf field</param>
    /// <param name="afi">The Application Family Identifier group the tag has to belong to to be read</param>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    public List<HfTag> GetInventory(bool singleTag, bool onlyNewTags, int afi = 0)
    {
      _lastInventory = null;
      SendCommand($"INV{(singleTag ? " SSL" : "")}{(onlyNewTags ? " ONT" : "")}{(afi != 0 ? $"AFI {afi:X2}" : "")}");
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

    /// <summary>
    /// Starts the continuous inventory scan.
    /// If the inventory event handler is set, any transponder found will be delivered via it. 
    /// If the event handler is not set, the found transponders can be fetched
    /// via the method FetchInventory
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
      StartInventory(false, false, 0);
    }

    /// <summary>
    /// Starts the continuous inventory scan.
    /// If the inventory event handler is set, any transponder found will be delivered via it. 
    /// If the event handler is not set, the found transponders can be fetched
    /// via the method FetchInventory
    /// </summary>
    /// <param name="afi">The Application Family Identifier group the tag has to belong to to be read</param>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    public void StartInventory(int afi)
    {
      StartInventory(false, false, afi);
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
      _continuousStarted = false;
    }
    /// <summary>
    /// Starts the continuous inventory scan.
    /// If the inventory event handler is set, any transponder found will be delivered via it. 
    /// If the event handler is not set, the found transponders can be fetched
    /// via the method FetchInventory
    /// </summary>
    /// <param name="singleTag">Set to true if only one tag is expected. Throws an error if more tags are found</param>
    /// <param name="onlyNewTags">Find each tag only once as long as it stays powered within the rf field</param>
    /// <param name="afi">The Application Family Identifier group the tag has to belong to to be read</param>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    public void StartInventory(bool singleTag, bool onlyNewTags, int afi = 0)
    {
      SendCommand($"CNR INV{(singleTag ? " SSL" : "")}{(onlyNewTags ? " ONT" : "")}{(afi != 0 ? $"AFI {afi:X2}" : "")}");
      _continuousStarted = true;
    }
    /// <summary>
    /// Enable the rf interface 
    /// </summary>
    /// <param name="mode">the interface mode. Defaults to SingeSubcarrier with 100% ASK modulation</param>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    public void EnableRfInterface(RfInterfaceMode mode = RfInterfaceMode.SingleSubcarrier_100percentASK)
    {
      string command = "SRI ";
      switch (mode)
      {
        case RfInterfaceMode.SingleSubcarrier_10percentASK:
          command += "SS 10";
          _isSingleSubcarrier = true;
          break;
        case RfInterfaceMode.SingleSubcarrier_100percentASK:
          command += "SS 100";
          _isSingleSubcarrier = true;
          break;
        case RfInterfaceMode.DoubleSubcarrier_10percentASK:
          if (FirmwareMajorVersion < 3)
          {
            throw new InvalidOperationException($"Not supported by Firmware less than 3.0");
          }
          command += "DS 10";
          _isSingleSubcarrier = false;
          break;
        case RfInterfaceMode.DoubleSubcarrier_100percentASK:
          command += "DS 100";
          _isSingleSubcarrier = false;
          break;
        default:
          throw new InvalidOperationException($"Unhandled mode {mode}");
      }
      SetCommand(command);
      // _mode = mode;
    }

    /// <summary>
    /// Read the transponder information
    /// </summary>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <returns>HfTag with the data or the error message</returns>
    public HFTagInformation ReadTagInformation(string? tagId = null, bool optionFlag = false)
    {
      HfTag tag = SendRequest("WRQ", "2B", null, tagId, optionFlag);
      HFTagInformation info = new() { TID = tag.TID, HasError = tag.HasError, Message = tag.Message };
      if (info.HasError)
      {
        return info;
      }
      byte infoFlag = byte.Parse(tag.Data![0..2], System.Globalization.NumberStyles.HexNumber);
      info.DSFIDSupported = 0 != (infoFlag & 0x01);
      if (info.DSFIDSupported)
      {
        info.DSFID = int.Parse(tag.Data![18..20], System.Globalization.NumberStyles.HexNumber);
      }
      info.AFISupported = 0 != (infoFlag & 0x02);
      if (info.AFISupported)
      {
        info.AFI = int.Parse(tag.Data![20..22], System.Globalization.NumberStyles.HexNumber);
      }
      info.VICCMemorySizeSupported = 0 != (infoFlag & 0x04);
      if (info.VICCMemorySizeSupported)
      {
        info.VICCBlockCount = int.Parse(tag.Data![22..24], System.Globalization.NumberStyles.HexNumber);
        info.VICCBlockSize = int.Parse(tag.Data![24..26], System.Globalization.NumberStyles.HexNumber) + 1;
      }
      info.ICReferenceSupported = 0 != (infoFlag & 0x08);
      if (info.ICReferenceSupported)
      {
        info.ICReference = int.Parse(tag.Data![26..28], System.Globalization.NumberStyles.HexNumber);
      }
      return info;
    }

    /// <summary>
    /// Read a data block of a transponder
    /// </summary>
    /// <param name="block">block to read</param>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <returns>HfTag with the data or the error message</returns>
    public HfTag ReadBlock(int block, string tagId = null!, bool optionFlag = false)
    {
      return SendRequest("REQ", "20", $"{block:X2}", tagId, optionFlag);
    }

    /// <summary>
    /// Read the data of a transponder
    /// </summary>
    /// <param name="startBlock">start block</param>
    /// <param name="blocksToRead">Blocks to read</param>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <returns>HfTag with the data or the error message</returns>
    public HfTag ReadMultipleBlocks(int startBlock, int blocksToRead, string tagId = null!, bool optionFlag = false)
    {
      if (FirmwareMajorVersion < 3 && blocksToRead > 25)
      {
        throw new InvalidOperationException("The number of blocks must not be greater than 25.");
      }
      if (0 > startBlock || startBlock > 255 || (startBlock + blocksToRead) > 255)
      {
        throw new InvalidOperationException("wrong parameter number\n0<=startBlock<256  (startBlock+numberBlocks)<256");
      }
      int numberOfFollowingBlock = blocksToRead - 1;
      if (numberOfFollowingBlock < 0)
      {
        numberOfFollowingBlock = 0;
      }
      return SendRequest("REQ", "23", $"{startBlock:X2}{numberOfFollowingBlock:X2}", tagId, optionFlag);
    }
    /// <summary>
    /// Write a block of a transponder
    /// </summary>
    /// <param name="block">the block to write</param>
    /// <param name="data">the data to write</param>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <returns></returns>
    public HfTag WriteBlock(int block, string data, string tagId = null!, bool optionFlag = false)
    {
      HfTag response = SendRequest("WRQ", "21", $"{block:X2}{data}", tagId, optionFlag);
      if (!response.HasError)
      {
        response.Data = data;
      }
      return response;
    }

    /// <summary>
    /// Write a data to a transponder
    /// </summary>
    /// <param name="startBlock">the tag start block</param>
    /// <param name="data">the data to write</param>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="blockSize">the tag block size, default 4 Byte</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <returns></returns>
    public HfTag WriteMultipleBlocks(int startBlock, string data, string tagId = null!, int blockSize = 4, bool optionFlag = false)
    {
      int hexDataSize = blockSize * 2;
      int numberBlocks = (data.Length + hexDataSize - 1) / hexDataSize;
      while (data.Length < hexDataSize * numberBlocks)
      {
        // given data is too short...fill with '00'
        data += "0";
      }
      HfTag tag = null!;
      for (int i = 0; i < numberBlocks; i++)
      {
        for (int n = 0; n < 2; n++)
        {
          HfTag response = WriteBlock(startBlock + i, data.Substring(startBlock + hexDataSize * i, hexDataSize), tagId, optionFlag);
          if (!response.HasError)
          {
            if (null != tag)
            {
              tag.Data += response.Data;
            }
            else
            {
              tag = response;
            }
            break;
          }
          else if (n >= 1)
          {
            // second retry has also an error
            if (tag == null)
            {
              return response;
            }
            else
            {
              tag.HasError = response.HasError;
              tag.Message = response.Message;
              return tag;
            }
          }
        }
      }
      return tag;
    }
    /// <summary>
    /// Write the transponder application family identifier value
    /// </summary>
    /// <param name="afi">the application family identifier to set</param>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <returns></returns>
    public HfTag WriteTagAFI(int afi, string? tagId, bool optionFlag = false)
    {
      return SendRequest("WRQ", "27", afi.ToString("X2"), tagId, optionFlag);
    }
    /// <summary>
    /// Lock the transponder application family identifier
    /// </summary>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <returns></returns>
    public HfTag LockTagAFI(string? tagId, bool optionFlag = false)
    {
      return SendRequest("WRQ", "28", null, tagId, optionFlag);
    }

    /// <summary>
    /// Write the transponder data storage format identifier
    /// </summary>
    /// <param name="dsfid">the data storage format identifier to set</param>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <returns></returns>
    public HfTag WriteTagDSFID(int dsfid, string? tagId, bool optionFlag = false)
    {
      return SendRequest("WRQ", "29", dsfid.ToString("X2"), tagId, optionFlag);
    }
    /// <summary>
    /// Lock the transponder data storage format identifier
    /// </summary>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <returns></returns>
    public HfTag LockTagDSFID(string? tagId, bool optionFlag = false)
    {
      return SendRequest("WRQ", "2A", null, tagId, optionFlag);
    }



    /// <summary>
    /// Send a request to the transponder
    /// </summary>
    /// <param name="command">The request command "REQ" or "WRQ"</param>
    /// <param name="tagCommand">The tag command</param>
    /// <param name="data">The additional data</param>
    /// <param name="tagId">The transponder id. Defaults to null.</param>
    /// <param name="optionFlag">The option flag. Defaults to False.</param>
    protected HfTag SendRequest(string command, string tagCommand, string? data = null,
                                string? tagId = null, bool optionFlag = false)
    {
      string flags;
      if (null != tagId)
      {
        flags = $"{(optionFlag ? "6" : "2")}{(_isSingleSubcarrier ? "2" : "3")}{tagCommand}{tagId}{data ?? ""}";
      }
      else
      {
        flags = $"{(optionFlag ? "4" : "0")}{(_isSingleSubcarrier ? "2" : "3")}{tagCommand}{data ?? ""}";
      }
      _lastRequest = null;
      SendCommand($"{command} {flags} CRC");
      DateTime start = DateTime.Now;
      while (DateTime.Now.Subtract(start).TotalMilliseconds < ResponseTimeout)
      {
        if (null == _lastRequest)
        {
          Thread.Sleep(20);
        }
        else
        {
          _lastRequest.TID = tagId!;
          return _lastRequest;
        }
      }
      if (!Connected)
        throw new ObjectDisposedException("Not connected");
      throw new TimeoutException("Response timeout");
    }


  }

  /// <summary>
  /// RF interface mode for the tag communication
  /// </summary>
  public enum RfInterfaceMode
  {
    /// <summary>
    /// Single subcarrier with 10% ASK modulation
    /// </summary>
    SingleSubcarrier_10percentASK,
    /// <summary>
    /// Default Setting. Single subcarrier with 100% ASK modulation
    /// </summary>
    SingleSubcarrier_100percentASK,
    /// <summary>
    /// Double subcarrier with 10% ASK modulation
    /// </summary>
    DoubleSubcarrier_10percentASK,
    /// <summary>
    /// Double subcarrier with 100% ASK modulation
    /// </summary>
    DoubleSubcarrier_100percentASK
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

  /// <summary>
  /// HF tag information
  /// </summary>
  public class HFTagInformation
  {
    /// <summary>tag id</summary>
    /// <value></value>
    public string? TID { get; internal set; }
    /// <summary>Data storage format identifier support</summary>
    /// <value></value>
    public bool DSFIDSupported { get; internal set; }
    /// <summary>Data storage format identifier</summary>
    /// <value></value>
    public int DSFID { get; internal set; }
    /// <summary>Application family identifier support</summary>
    /// <value></value>
    public bool AFISupported { get; internal set; }
    /// <summary>Application family identifier</summary>
    /// <value></value>
    public int AFI { get; internal set; }
    /// <summary>Vicinity integrated circuit card memory size support</summary>
    /// <value></value>
    public bool VICCMemorySizeSupported { get; internal set; }
    /// <summary>Vicinity integrated circuit card block numbers</summary>
    /// <value></value>
    public int VICCBlockCount { get; internal set; }
    /// <summary>Vicinity integrated circuit card block size</summary>
    /// <value></value>
    public int VICCBlockSize { get; internal set; }
    /// <summary>IC Reference support</summary>
    /// <value></value>
    public bool ICReferenceSupported { get; internal set; }
    /// <summary>IC Reference</summary>
    /// <value></value>
    public int ICReference { get; internal set; }
    /// <summary>
    /// True if the tag contains error information
    /// </summary>
    public bool HasError { get; internal set; }
    /// <summary>
    /// the error message 
    /// </summary>
    public string? Message { get; internal set; }
  }
}