using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The reader class for the metratec hf readers based on the Ascii protocol
  /// </summary>
  public class HfReaderAscii : MetratecReaderAscii<HfTag>
  {

    #region Properties

    private List<HfTag>? _lastInventory = null;
    private bool _continuousStarted = false;

    private HfTag? _lastRequest = null;
    // private RfInterfaceMode _mode = RfInterfaceMode.SingleSubcarrier_100percentASK;

    private bool _isSingleSubcarrier = true;

    #endregion Properties

    #region Event Handlers

    /// <summary>
    /// request response event handler
    /// </summary>
    public event EventHandler<NewRequestResponseArgs>? NewRequestResponse;

    #endregion Event Handlers

    #region Constructor

    /// <summary>
    /// Create a new instance of the HfReaderAscii class.
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public HfReaderAscii(ICommunicationInterface connection, ILogger logger = null!, string id = null!) : base(connection, logger, id)
    {
    }

    #endregion Constructor

    #region Public Methods

    #region Reader Settings
    /// <inheritdoc/>
    public override void EnableCrcCheck(bool enable = true)
    {
      base.EnableCrcCheck(enable);
      // update input event setting
      EnableInputEvents();
    }
    /// <summary>
    /// Set the reader power
    /// </summary>
    /// <param name="power">the reader power</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public override void SetPower(int power)
    {
      if (FirmwareMajorVersion >= 3)
      {
        SetCommand($"SET PWR {power}");
      }
      else
      {
        throw new MetratecReaderException($"Firmware Version {FirmwareVersion} does not support power settings");
      }
    }
    /// <summary>
    /// Enable the rf interface 
    /// </summary>
    /// <param name="subCarrier">rf interface sub carrier</param>
    /// <param name="modulationDepth">rf interface modulation depth</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void EnableRfInterface(SubCarrier subCarrier = SubCarrier.SINGLE, ModulationDepth modulationDepth = ModulationDepth.Depth100)
    {
      string command = "SRI ";
      switch (subCarrier)
      {
        case SubCarrier.SINGLE:
          command += "SS ";
          break;
        case SubCarrier.DOUBLE:
          command += "DS ";
          break;
        default:
          throw new MetratecReaderException($"Unhandled mode sub carrier {subCarrier}");
      }
      switch (modulationDepth)
      {
        case ModulationDepth.Depth10:
          command += "10";
          if (subCarrier == SubCarrier.DOUBLE && FirmwareMajorVersion < 3)
          {
            throw new MetratecReaderException($"Double subcarrier and modulation depth 10 is not supported by Firmware less than 3.0");
          }
          break;
        case ModulationDepth.Depth100:
          command += "100";
          break;
        default:
          throw new MetratecReaderException($"Unhandled mode modulation depth {modulationDepth}");
      }
      SetCommand(command);
      // _mode = mode;
    }
    #endregion Reader Settings

    #region Tag Commands
    /// <summary>
    /// Scan for the current inventory
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public override List<HfTag> GetInventory()
    {
      return GetInventory(false, false, 0);
    }

    /// <summary>
    /// Scan for the current inventory
    /// </summary>
    /// <param name="afi">The Application Family Identifier group the tag has to belong to to be read</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
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
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<HfTag> GetInventory(bool singleTag, bool onlyNewTags, int afi = 0)
    {
      _lastInventory = null;
      SendCommand($"INV{(singleTag ? " SSL" : "")}{(onlyNewTags ? " ONT" : "")}{(afi != 0 ? $" AFI {afi:X2}" : "")}");
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
        throw new MetratecReaderException("Not connected");
      throw new MetratecReaderException("Response timeout");
    }

    /// <summary>
    /// Starts the continuous inventory scan.
    /// If the inventory event handler is set, any transponder found will be delivered via it. 
    /// If the event handler is not set, the found transponders can be fetched
    /// via the method FetchInventory
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
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
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void StartInventory(int afi)
    {
      StartInventory(false, false, afi);
    }
    /// <summary>
    /// Stops the continuous inventory scan.
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
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
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void StartInventory(bool singleTag, bool onlyNewTags, int afi = 0)
    {
      SendCommand($"CNR INV{(singleTag ? " SSL" : "")}{(onlyNewTags ? " ONT" : "")}{(afi != 0 ? $"AFI {afi:X2}" : "")}");
      _continuousStarted = true;
    }
    /// <summary>
    /// Read the transponder information
    /// </summary>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <returns>the transponder information</returns>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, the tag error message is in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public HFTagInformation ReadTagInformation(string? tagId = null, bool optionFlag = false)
    {
      HfTag tag = SendRequest("WRQ", "2B", null, tagId, optionFlag);
      HFTagInformation info = new() { TID = tag.TID };
      if (tag.HasError)
      {
        throw new TransponderException(tag.Message);
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
    /// Read a data block of a transponder as hex
    /// </summary>
    /// <param name="block">block to read</param>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <returns>the transponder data</returns>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, the tag error message is in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public string ReadBlock(int block, string tagId = null!, bool optionFlag = false)
    {
      HfTag tag = SendRequest("REQ", "20", $"{block:X2}", tagId, optionFlag);
      if (tag.HasError)
      {
        throw new TransponderException(tag.Message);
      }
      return tag.Data ?? "";
    }
    /// <summary>
    /// Read the data of a transponder
    /// </summary>
    /// <param name="startBlock">start block</param>
    /// <param name="blocksToRead">Blocks to read</param>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <returns>the transponder data</returns>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, the tag error message is in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public string ReadMultipleBlocks(int startBlock, int blocksToRead, string tagId = null!, bool optionFlag = false)
    {
      if (FirmwareMajorVersion < 3 && blocksToRead > 25)
      {
        throw new MetratecReaderException("The number of blocks must not be greater than 25.");
      }
      if (0 > startBlock || startBlock > 255 || (startBlock + blocksToRead) > 255)
      {
        throw new MetratecReaderException("wrong parameter number\n0<=startBlock<256  (startBlock+numberBlocks)<256");
      }
      int numberOfFollowingBlock = blocksToRead - 1;
      if (numberOfFollowingBlock < 0)
      {
        numberOfFollowingBlock = 0;
      }
      HfTag tag = SendRequest("REQ", "23", $"{startBlock:X2}{numberOfFollowingBlock:X2}", tagId, optionFlag);
      if (tag.HasError)
      {
        throw new TransponderException(tag.Message);
      }
      return tag.Data ?? "";
    }
    /// <summary>
    /// Write a block of a transponder
    /// </summary>
    /// <param name="block">the block to write</param>
    /// <param name="data">the data to write</param>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, the tag error message is in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void WriteBlock(int block, string data, string tagId = null!, bool optionFlag = false)
    {
      HfTag tag = SendRequest("WRQ", "21", $"{block:X2}{data}", tagId, optionFlag);
      if (tag.HasError)
      {
        throw new TransponderException(tag.Message);
      }
    }
    /// <summary>
    /// Write a data to a transponder
    /// </summary>
    /// <param name="startBlock">the tag start block</param>
    /// <param name="data">the data to write</param>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="blockSize">the tag block size, default 4 Byte</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, the tag error message is in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void WriteMultipleBlocks(int startBlock, string data, string tagId = null!, int blockSize = 4, bool optionFlag = false)
    {
      int hexDataSize = blockSize * 2;
      int numberBlocks = (data.Length + hexDataSize - 1) / hexDataSize;
      while (data.Length < hexDataSize * numberBlocks)
      {
        // given data is too short...fill with '00'
        data += "0";
      }
      for (int i = 0; i < numberBlocks; i++)
      {
        for (int n = 0; n < 2; n++)
        {
          try
          {
            WriteBlock(startBlock + i, data.Substring(startBlock + hexDataSize * i, hexDataSize), tagId, optionFlag);
          }
          catch (TransponderException)
          {
            if (n >= 1)
            {
              // second retry has also an error
              throw;
            }
          }
        }
      }
    }
    /// <summary>
    /// Write the transponder application family identifier value
    /// </summary>
    /// <param name="afi">the application family identifier to set</param>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, the tag error message is in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void WriteTagAFI(int afi, string? tagId, bool optionFlag = false)
    {
      HfTag tag = SendRequest("WRQ", "27", afi.ToString("X2"), tagId, optionFlag);
      if (tag.HasError)
      {
        throw new TransponderException(tag.Message);
      }
    }
    /// <summary>
    /// Lock the transponder application family identifier
    /// </summary>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, the tag error message is in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void LockTagAFI(string? tagId, bool optionFlag = false)
    {
      HfTag tag = SendRequest("WRQ", "28", null, tagId, optionFlag);
      if (tag.HasError)
      {
        throw new TransponderException(tag.Message);
      }
    }
    /// <summary>
    /// Write the transponder data storage format identifier
    /// </summary>
    /// <param name="dsfid">the data storage format identifier to set</param>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, the tag error message is in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void WriteTagDSFID(int dsfid, string? tagId, bool optionFlag = false)
    {
      HfTag tag = SendRequest("WRQ", "29", dsfid.ToString("X2"), tagId, optionFlag);
      if (tag.HasError)
      {
        throw new TransponderException(tag.Message);
      }
    }
    /// <summary>
    /// Lock the transponder data storage format identifier
    /// </summary>
    /// <param name="tagId">the optional tag id, if not set, the currently available tag is write</param>
    /// <param name="optionFlag">Meaning is defined by the tag command description</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, the tag error message is in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void LockTagDSFID(string? tagId, bool optionFlag = false)
    {
      HfTag tag = SendRequest("WRQ", "2A", null, tagId, optionFlag);
      if (tag.HasError)
      {
        throw new TransponderException(tag.Message);
      }
    }
    #endregion Tag Commands

    #endregion Public Methods

    #region Protected Methods

    /// <summary>
    /// Configure the reader.
    /// The base implementation must be called after success.
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected override void ConfigureReader()
    {
      SetVerbosityLevel(2);
      SetCommand("MOD 156");
      EnableRfInterface();
      EnableInputEvents();
    }
    /// <summary>
    /// Process the reader response...override for event check
    /// /// The base implementation must be called after success.
    /// </summary>
    /// <param name="response">a reader response</param>
    protected override void HandleResponse(string response)
    {
      switch (response[0])
      {
        case 'T':
          // check for special hf events
          if (response[1] == 'D' || response[1] == 'N')
          {
            // TDT or TND
            HandleRequestResponse(response);
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
            throw new MetratecReaderException(s);
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
    /// prepare the input for event handling
    /// </summary>
    /// <param name="enable"></param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    protected virtual void EnableInputEvents(bool enable = true)
    {
      try
      {
        if (enable)
        {
          SetCommand("EGC 0 #SPIN0#RIP 0" + (IsCRC ? " 640F" : ""));
          SetCommand("EGC 0 BOTH");
          SetCommand("EGC 1 #SPIN1#RIP 1" + (IsCRC ? " FC68" : ""));
          SetCommand("EGC 1 BOTH");
        }
        else
        {
          SetCommand("EGC 0 NONE");
          SetCommand("EGC 1 NONE");
        }
      }
      catch (MetratecReaderException)
      {
        Logger.LogDebug("Inputs events disabled");
      }
    }
    /// <summary>
    /// Fire a inventory event
    /// </summary>
    /// <param name="tag">the founded tags</param>
    protected void FireRequestResponse(HfTag tag)
    {
      if (null == NewRequestResponse)
        return;
      NewRequestResponseArgs args = new(tag, new DateTime());
      ThreadPool.QueueUserWorkItem(o => NewRequestResponse.Invoke(this, args));
    }
    /// <summary>
    /// Send a request to the transponder
    /// </summary>
    /// <param name="command">The request command "REQ" or "WRQ"</param>
    /// <param name="tagCommand">The tag command</param>
    /// <param name="data">The additional data</param>
    /// <param name="tagId">The transponder id. Defaults to null.</param>
    /// <param name="optionFlag">The option flag. Defaults to False.</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
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
        throw new MetratecReaderException("Not connected");
      throw new MetratecReaderException("Response timeout");
    }
    #endregion Protected Methods

    #region Private Methods

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

    #endregion Private Methods
  }

  #region Configuration Enums

  /// <summary>
  /// Used RF interface sub carrier
  /// </summary>
  public enum SubCarrier
  {
    /// <summary>
    /// Single mode
    /// </summary>
    SINGLE,
    /// <summary>
    /// Double Mode
    /// </summary>
    DOUBLE
  }
  /// <summary>
  /// Used RF interface modulation depth
  /// </summary>
  public enum ModulationDepth
  {
    /// <summary>
    /// Modulation depth 10
    /// </summary>
    Depth10,
    /// <summary>
    /// Modulation depth 100
    /// </summary>
    Depth100
  }

  #endregion Configuration Enums

  #region Response Classes

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
  }

  #endregion Response Classes
}