using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The reader class for the metratec nfc readers based on the AT protocol
  /// </summary>
  public class NfcReader : MetratecReaderAT<HfTag>
  {
    private InventorySettingsNfc? _inventorySettings;
    private NfcReaderMode? _mode;
    private string _selectedTag = "";
    /// <summary>
    /// Create a new instance of the NfcReader class.
    /// </summary>
    /// <param name="connection">The connection interface</param>
    public NfcReader(ICommunicationInterface connection) : base(connection)
    {
    }

    /// <summary>
    /// Create a new instance of the NfcReader class.
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    public NfcReader(ICommunicationInterface connection, ILogger logger) : base(connection, logger)
    {
    }

    /// <summary>
    /// Create a new instance of the NfcReader class.
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// /// <param name="id">The reader id</param>

    public NfcReader(ICommunicationInterface connection, string id) : base(connection, id)
    {
    }

    /// <summary>
    /// Create a new instance of the NfcReader class.
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="id">The reader id</param>
    /// <param name="logger">The connection interface</param>
    public NfcReader(ICommunicationInterface connection, string id, ILogger logger) : base(connection, id, logger)
    {
    }


    /// <summary>
    /// Parse the inventory event (+CINV, +CMINV, +CINVR)
    /// </summary>
    /// <param name="response"></param>
    protected override void HandleInventoryEvent(string response)
    {
      List<HfTag> tags = ParseInventory(response, "+CINV: ".Length);
      FireInventoryEvent(tags, true);
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
      //TODO
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
      base.ConfigureReader();
      GetInventorySettings();
      GetMode();
    }

    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Feedback
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Play a preconfigured sequence.
    /// </summary>
    /// <param name="feedback">0 for the Startup jingle, 1 for the OK Feedback, 2 for the ERROR feedback</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void PlayFeedback(int feedback)
    {
      SetCommand($"AT+FDB={feedback}");
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="notes">Encoded notes to be played. A note is always encoded by its name written as a capital letter
    /// and octave e.g. C4 or D5. Half-tone steps are encoded by adding a s or b to the note. For example Ds4 or
    /// Eb4. Note that Ds4 and Eb4 are basically the same note. A pause is denoted by a lowercase x.</param>
    /// <param name="repetitions">Number of times the sequence should be repeated</param>
    /// <param name="stepLength">Step length of a single step in the sequence</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void PlayNotes(string notes, int repetitions, int stepLength)
    {
      SetCommand($"AT+PLY={notes},{repetitions},{stepLength}");
    }
    /// <summary>
    /// Set the play frequency. The frequency is given in Hertz. To stop playback a frequency of 0 Hz should be issued.
    /// </summary>
    /// <param name="frequency">The frequency is given in Hertz.</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void PlayAFrequency(int frequency)
    {
      SetCommand($"AT+FRQ={frequency}");
    }

    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// RFID Settings
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Enable the rf interface 
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void EnableRfInterface()
    {
      SetCommand("AT+CW=1");
    }
    /// <summary>
    /// Disable the rf interface 
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void DisableRfInterface()
    {
      SetCommand("AT+CW=0");
    }
    /// <summary>
    /// Set the reader mode
    /// </summary>
    /// <param name="mode">the reader mode</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetMode(NfcReaderMode mode)
    {
      SetCommand($"AT+MOD={mode.ToString()}");
      _mode = mode;
    }
    /// <summary>
    /// Get the current reader mode
    /// </summary>
    /// <returns>the current reader mode</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public NfcReaderMode GetMode()
    {
      string response = GetCommand("AT+MOD?");
      // +MOD= ISO14A
      NfcReaderMode mode = Enum.Parse<NfcReaderMode>(response[6..]);
      _mode = mode;
      return mode;
    }
    /// <summary>
    /// Configure the rf interface
    /// </summary>
    /// <param name="subCarrier"></param>
    /// <param name="modulationDepth"></param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetRfInterfaceConfig(SubCarrier subCarrier, ModulationDepth modulationDepth)
    {
      SetCommand($"AT+CRI={subCarrier},{(modulationDepth == ModulationDepth.Depth10 ? "10" : "100")}");
    }
    /// <summary>
    /// Get the current rf interface subcarrier
    /// </summary>
    /// <returns>the current rf subcarrier</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public SubCarrier GetRfInterfaceSubCarrier()
    {
      //+CRI: SINGLE,100
      string[] response = SplitLine(GetCommand("AT+CRI?")[6..]);
      return (SubCarrier)Enum.Parse(typeof(SubCarrier), response[0]);
    }
    /// <summary>
    /// Get the current rf interface modulation depth
    /// </summary>
    /// <returns>the current rf interface modulation depth </returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public ModulationDepth GetRfInterfaceModulationDepth()
    {
      //+CRI: SINGLE,100
      string[] response = SplitLine(GetCommand("AT+CRI?")[6..]);
      return response[1].Contains("100") ? ModulationDepth.Depth100 : ModulationDepth.Depth10;
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
      throw new MetratecReaderException("Command not supported");
    }

    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///Tag Operation
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Return the current inventory settings
    /// </summary>
    /// <returns>The current inventory settings</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public InventorySettingsNfc GetInventorySettings()
    {
      if (null != _inventorySettings)
      {
        return _inventorySettings;
      }
      string[] split = SplitLine(GetCommand("AT+INVS?")[7..]); // +INVS: 0,0,0
      _inventorySettings = new InventorySettingsNfc(split[0] == "1", split[1] == "1",
                                                 split[2] == "1");
      return _inventorySettings;
    }
    /// <summary>
    /// Sets the inventory settings
    /// </summary>
    /// <param name="settings">the inventory settings</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetInventorySettings(InventorySettingsNfc settings)
    {
      if (null == settings)
      {
        return;
      }
      SetCommand($"AT+INVS={(settings.AddTagDetails ? "1" : "0")},{(settings.OnlyNewTags ? "1" : "0")}," +
                 $"{(settings.SingleSlot ? "1" : "0")}");
      _inventorySettings = settings;
    }
    /// <summary>
    /// Scan for the current inventory
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public override List<HfTag> GetInventory()
    {
      List<HfTag> tags = ParseInventory(GetCommand("AT+INV"), "+INV: ".Length);
      FireInventoryEvent(tags, false);
      return tags;
    }

    /// <summary>
    /// Starts the continuous inventory scan. Make sure that the inventory is set. 
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public override void StartInventory()
    {
      SetCommand("AT+CINV");
    }

    private List<HfTag> ParseInventory(string response, int prefixLength, bool throwError = false)
    {
      // +INV: E0040150954F0983,ISO15,01<CR>
      // +INV: 801E837A2ABC04,ISO14A,00,4400<CR><LF>
      // +INV: <NO TAGS FOUND><CR><LF>
      DateTime timestamp = DateTime.Now;
      List<HfTag> tags = new();
      int antenna = 1;
      string error = "";
      InventorySettingsNfc settings = GetInventorySettings();
      foreach (string info in SplitResponse(response))
      {
        if (info[0] != '+')
        {
          continue;
        }
        string[] split = SplitLine(info[prefixLength..]);
        if (split[0][0] == '<')
        {
          // message
          switch (split[0][1])
          {
            case 'R': //Round finished
              if (split[0].Length > 16)
              {
                antenna = int.Parse(split[1].Substring(5, 1));
                foreach (HfTag tag in tags)
                {
                  tag.Antenna = antenna;
                }
              }
              break;
            case 'N': // No Tags
              break;
            default:
              if (throwError)
              {
                error = split[0][1..^2];
              }
              break;
          }
          continue;
        }
        try
        {
          HfTag tag;
          if (settings.AddTagDetails)
          {
            switch (_mode)
            {
              case NfcReaderMode.ISO15:
                {
                  ISO15Tag transponder = new ISO15Tag(split[0], timestamp, CurrentAntennaPort);
                  transponder.DSFID = split[1];
                  tag = transponder;
                }
                break;
              case NfcReaderMode.ISO14A:
                {
                  tag = new ISO14ATag(split[0], timestamp, CurrentAntennaPort)
                  {
                    SAK = split[1],
                    ATQA = split[2]
                  };
                }
                break;
              case NfcReaderMode.AUTO:
                if (split[1] == NfcReaderMode.ISO15.ToString())
                {
                  tag = new ISO15Tag(split[0], timestamp, CurrentAntennaPort)
                  {
                    DSFID = split[2]
                  };
                }
                else
                {
                  tag = new ISO14ATag(split[0], timestamp, CurrentAntennaPort)
                  {
                    SAK = split[2],
                    ATQA = split[3]
                  };
                }
                break;
              default:
                tag = new(split[0], timestamp, CurrentAntennaPort);
                break;
            }
          }
          else
          {
            switch (_mode)
            {
              case NfcReaderMode.ISO15:
                tag = new ISO15Tag(split[0], timestamp, CurrentAntennaPort);
                break;
              case NfcReaderMode.ISO14A:
                tag = new ISO14ATag(split[0], timestamp, CurrentAntennaPort);
                break;
              case NfcReaderMode.AUTO:
              default:
                tag = new(split[0], timestamp, CurrentAntennaPort);
                break;
            }
          }
          tags.Add(tag);
        }
        catch (Exception e)
        {
          if (null == _inventorySettings)
          {
            // not initialised - ignore
            return tags;
          }
          Logger.LogWarning("Inventory warning ({}) - {}", info, e.Message);
        }
      }
      if (error.Length > 0)
      {
        throw new MetratecReaderException((0 > antenna ? $"Antenna {antenna}: " : "") + error);
      }
      return tags;
    }
    /// <summary>
    /// Read data from the card's memory.
    /// Depending on the protocol a select and authenticate is needed prior to this command.
    /// </summary>
    /// <param name="block">the block to read</param>
    /// <param name="numberOfBlocks">Number of blocks to read</param>
    /// <returns>the block data as hex string</returns>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public string ReadBlock(int block, int numberOfBlocks = 1)
    {
      // +READ: 01020304
      if (numberOfBlocks == 1)
      {
        return GetCommand($"AT+READ={block}")[7..];
      }
      else
      {
        string data = "";
        foreach (String response in SplitResponse(GetCommand($"AT+READM={block},{numberOfBlocks}")))
        {
          data += response[8..];
        }
        return data;
      }
    }
    /// <summary>
    /// Write data to a block of the tags memory. 
    /// Depending on the protocol a select and authenticate is needed prior to this command.
    /// </summary>
    /// <param name="block">Number of the block to write</param>
    /// <param name="data">Data to write to the card</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void WriteBlock(int block, string data)
    {
      SetCommand($"AT+WRT={block},{data}");
    }
    /// <summary>
    /// Select a tag by its TID
    /// </summary>
    /// <param name="tagId">The transponder tid</param>
    /// <exception cref="TransponderException">
    /// If the transponder is not responding
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SelectTag(string tagId)
    {
      SetCommand($"AT+SEL={tagId}");
      _selectedTag = tagId;
    }
    /// <summary>
    /// Deselect the current selected tag.
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void DeselectTag()
    {
      SetCommand($"AT+DEL");
      _selectedTag = "";
    }
    /// <summary>
    /// Get current selected transponder.
    /// </summary>
    /// <returns>The current selected transponder tid, 
    /// if no transponder is selected, the return value is empty</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public string GetSelectedTag()
    {
      return _selectedTag;
    }
    /// <summary>
    /// Detect the type of tags that are in the rf field.
    /// </summary>
    /// <returns>List with the detected tags</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<HfTag> DetectTagTypes()
    {
      List<HfTag> tags = new();
      string answer = GetCommand("AT+DTT");
      DateTime timestamp = DateTime.Now;
      foreach (String response in SplitResponse(answer))
      {
        // +DTT: E002223504422958,ISO15
        string[] data = SplitLine(response[6..]);
        if (data[0][0] == '<')
        {
          if (data[0][1] == 'N')
          {
            // No tags found
            break;
          }
          // error?
          throw new MetratecReaderException($"Unexpected Reader response: {answer}");
        }
        if (data[1] == "ISO15")
        {
          tags.Add(new ISO15Tag(data[0], timestamp, CurrentAntennaPort));
        }
        else
        {
          tags.Add(new ISO14ATag(data[0], timestamp, CurrentAntennaPort, data[1]));
        }
      }
      return tags;
    }

    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// ISO15693 Commands
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Send an ISO15693 read request with read-alike timing to a card.
    /// </summary>
    /// <param name="request">the request</param>
    /// <returns>the tag response</returns>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public string SendIso15ReadRequest(string request)
    {
      if (_mode != NfcReaderMode.ISO15)
      {
        throw new MetratecReaderException("Only available in ISO15 mode!");
      }
      string response = GetCommand($"AT+RRQ={request}");
      return string.IsNullOrEmpty(response) ? "" : response[6..];
    }

    /// <summary>
    /// Send an ISO15693 write request with write-alike timing to a card.
    /// </summary>
    /// <param name="request">the request</param>
    /// <returns>the tag response, empty if no response</returns>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public string SendIso15WriteRequest(string request)
    {
      if (_mode != NfcReaderMode.ISO15)
      {
        throw new MetratecReaderException("Only available in ISO15 mode!");
      }
      string response = GetCommand($"AT+WRQ={request}");
      return string.IsNullOrEmpty(response) ? "" : response[6..];
    }
    /// <summary>
    /// This command set the "Application Family Identifier" for IOS15693 inventories.
    /// An AFI of 0 is treated as no AFI set. If set to non-zero only transponders with
    /// the same AFI will respond in a inventory.
    /// </summary>
    /// <param name="afi">Application Family Identifier as hex string [0..255]</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetAfi(int afi)
    {
      SetCommand($"AT+AFI={afi:X2}");
    }
    /// <summary>
    /// This command returns "Application Family Identifier" of the reader for IOS15693 inventories.
    /// </summary>
    /// <returns>the "Application Family Identifier" of the reader</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public int GetAfi()
    {
      return int.Parse(GetCommand($"AT+AFI?")[6..], System.Globalization.NumberStyles.HexNumber);
    }
    /// <summary>
    /// Write the "Application Family Identifier" to an ISO15693 transponder.
    /// </summary>
    /// <param name="afi">afi value</param>
    /// <param name="withOptionFlag">request option flag. Defaults to False.</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void WriteTagAFI(int afi, bool withOptionFlag = false)
    {
      SetCommand($"AT+WAFI={afi:X2},{(withOptionFlag ? "1" : "0")}");
    }
    /// <summary>
    /// Use to permanently lock the AFI of an ISO15693 transponder
    /// </summary>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void LockTagAFI()
    {
      SetCommand($"AT+LAFI");
    }
    /// <summary>
    /// Write the "Data Storage Format Identifier" to an ISO15693 transponder.
    /// </summary>
    /// <param name="dsfid">dsfid value</param>
    /// <param name="withOptionFlag">request option flag. Defaults to False.</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void WriteTagDSFID(int dsfid, bool withOptionFlag = false)
    {
      SetCommand($"AT+WDSFID={dsfid:X2},{(withOptionFlag ? "1" : "0")}");
    }
    /// <summary>
    /// Use to permanently lock the "Data Storage Format Identifier" of an ISO15693 transponder
    /// </summary>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void LockTagDSFID()
    {
      SetCommand($"AT+LDSFID");
    }

    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// ISO14A Commands
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Generic ISO14A Commands
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Send a raw ISO 14A request to a previously selected tag
    /// </summary>
    /// <param name="request">request string</param>
    /// <returns>the tag response</returns>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public string SendISO14Request(string request)
    {
      if (_mode != NfcReaderMode.ISO14A)
      {
        throw new MetratecReaderException("Only available in ISO14A mode!");
      }
      if (_selectedTag.Length == 0)
      {
        throw new MetratecReaderException("No tag selected");
      }
      string response = GetCommand($"AT+REQ14={request}");
      return response.Length == 0 ? response : response[6..];
    }

    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Generic ISO14A Commands
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Authenticate command for Mifare classic cards to access memory blocks.
    /// Prior to this command, the card has to be selected.
    /// </summary>
    /// <param name="block">Block to authenticate</param>
    /// <param name="key">Mifare Key to authenticate with (6 bytes as Hex)</param>
    /// <param name="keyType">Type of key</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void AuthenticateMifareClassicBlock(int block, string key, KeyType keyType = KeyType.A)
    {
      SetCommand($"AT+AUT={block},{key.ToUpper()},{keyType}");
    }
    /// <summary>
    /// Authenticate command for Mifare classic cards to access memory blocks.
    /// Prior to this command, the card has to be selected.
    /// </summary>
    /// <param name="block">Block to authenticate</param>
    /// <param name="storedKey">Use a stored key instead of key and key type [0..16]</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void AuthenticateMifareClassicBlock(int block, int storedKey)
    {
      SetCommand($"AT+AUTN={block},{storedKey}");
    }
    /// <summary>
    /// Store an authenticate key in the reader.
    /// </summary>
    /// <param name="keyStore">the key store [0..16]</param>
    /// <param name="key">Mifare Key to authenticate with (6 bytes as Hex)</param>
    /// <param name="keyType">Type of key</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void StoreMifareClassicKey(int keyStore, string key, KeyType keyType = KeyType.A)
    {
      SetCommand($"AT+SIK={keyStore},{key.ToUpper()},{keyType}");
    }
    /// <summary>
    /// Get the access bits for a given Mifare Classic block.
    /// Prior to this command, the card has to be selected and the block has to be authenticated.
    /// </summary>
    /// <param name="block">Block to read access bits for</param>
    /// <returns>access bits as string, like "001"</returns>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public string ReadMifareClassicAccessBits(int block)
    {
      return GetCommand($"AT+GAB={block}")[6..];
    }
    /// <summary>
    /// Set the keys and optional also the access bits for a given block
    /// Prior to this command, the card has to be selected and the block has to be authenticated.
    /// </summary>
    /// <param name="block">Block to set keys and access bits for</param>
    /// <param name="keyA">Mifare KeyA</param>
    /// <param name="keyB">Mifare KeyB</param>
    /// <param name="accessBits">The Mifare access bits for the block as string</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void WriteMifareClassicKeys(int block, string keyA, string keyB, string accessBits = "")
    {
      if (accessBits.Length == 0)
        SetCommand($"AT+SKO={block},{keyA},{keyB}");
      else
        SetCommand($"AT+SKA={block},{keyA},{keyB},{accessBits}");
    }
    /// <summary>
    /// Write/Create a mifare classic value block.
    /// Prior to this command, the card has to be selected and the block has to be authenticated.
    /// </summary>
    /// <param name="block">block number</param>
    /// <param name="initialValue">initial value</param>
    /// <param name="backupAddress">address of the block used for backup</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void WriteMifareClassicValueBlock(int block, int initialValue, int backupAddress)
    {
      SetCommand($"AT+WVL={block},{initialValue},{backupAddress}");
    }
    /// <summary>
    /// Read a mifare classic value block
    /// Prior to this command, the card has to be selected and the block has to be authenticated.
    /// </summary>
    /// <param name="block">block number</param>
    /// <returns>the value</returns>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public int ReadMifareClassicValueBlock(int block)
    {
      // +RVL: 32,5
      return int.Parse(SplitLine(GetCommand($"AT+RVL={block}")[6..])[0]);
    }

    /// <summary>
    /// Read a mifare classic value block backup address
    /// Prior to this command, the card has to be selected and the block has to be authenticated.
    /// </summary>
    /// <param name="block">block number</param>
    /// <returns>the backup address</returns>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public int ReadMifareClassicValueBlockBackupAddress(int block)
    {
      // +RVL: 32,5
      return int.Parse(SplitLine(GetCommand($"AT+RVL={block}")[6..])[1]);
    }
    /// <summary>
    /// Add a value of a Mifare Classic block.
    /// Prior to this command, the card has to be selected and the block has to be authenticated.
    /// </summary>
    /// <param name="block">block number</param>
    /// <param name="value">value to add, can be positive or negative</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void AddMifareClassicValue(int block, int value)
    {
      if (value > 0)
      {
        IncrementMifareClassicValue(block, value);
      }
      else
      {
        DecrementMifareClassicValue(block, Math.Abs(value));
      }
    }
    /// <summary>
    /// Increment the value of a Mifare Classic block.
    /// Prior to this command, the card has to be selected and the block has to be authenticated.
    /// </summary>
    /// <param name="block">block number</param>
    /// <param name="valueToAdd">value to add</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void IncrementMifareClassicValue(int block, int valueToAdd)
    {
      SetCommand($"AT+IVL={block},{valueToAdd}");
    }
    /// <summary>
    /// Decrement the value of a Mifare Classic block.
    /// Prior to this command, the card has to be selected and the block has to be authenticated.
    /// </summary>
    /// <param name="block">block number</param>
    /// <param name="valueToSubtract">value to subtract</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void DecrementMifareClassicValue(int block, int valueToSubtract)
    {
      SetCommand($"AT+DVL={block},{valueToSubtract}");
    }
    /// <summary>
    /// Restore the value of a Mifare Classic block.
    /// This will load the current value from the block. 
    /// With the transfer method this value can be stored in a other block.
    /// Note that this operation only will have an effect after the transfer command is executed.
    /// Prior to this command, the card has to be selected and the block has to be authenticated.
    /// </summary>
    /// <param name="block"></param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void RestoreMifareClassicValue(int block)
    {
      SetCommand($"AT+RSVL={block}");
    }
    /// <summary>
    /// Write all pending transactions to a mifare classic block.
    /// Prior to this command, the card has to be selected and the block has to be authenticated.
    /// </summary>
    /// <param name="block"></param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void TransferMifareClassicValue(int block)
    {
      SetCommand($"AT+TXF={block}");
    }

    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Generic ISO14A Commands
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Authenticate command for NTAG / Mifare Ultralight cards.
    /// Prior to this command, the card has to be selected.
    /// After the authentication password protected pages can be accessed.
    /// Checks the password confirmation if it has been specified
    /// </summary>
    /// <param name="password">password 4Byte (hex)</param>
    /// <param name="passwordAcknowledge">password acknowledge (hex)</param>
    /// <returns>The password acknowledge.</returns>
    /// <exception cref="TransponderException">
    /// if the password acknowledge is not correct
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public string AuthenticateNTAG(string password, string passwordAcknowledge = "")
    {
      // +NPAUTH: ABCD
      string acknowledge = GetCommand($"AT+NPAUTH={password}")[9..];
      if (!string.IsNullOrEmpty(passwordAcknowledge) && passwordAcknowledge != acknowledge)
      {
        throw new TransponderException("wrong acknowledge");
      }
      return acknowledge;
    }
    /// <summary>
    /// Set the password and the password acknowledge for NTAG / Mifare Ultralight cards.
    /// Prior to this command, the card has to be selected.
    /// </summary>
    /// <param name="password">password 4Byte (hex)</param>
    /// <param name="passwordAcknowledge">password acknowledge (hex)</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void WriteNTAGAuthenticate(string password, string passwordAcknowledge)
    {
      SetCommand($"AT+NPWD={password},{passwordAcknowledge}");
    }
    /// <summary>
    /// Configure the NTAG access.
    /// </summary>
    /// <param name="config">Access config</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void WriteNTAGAccessConfig(NTAGAccessConfig config)
    {
      SetCommand($"AT+NACFG={config.StartAddress},{(config.ReadProtected ? 1 : 0)},{config.MaxAttempts}");
    }
    /// <summary>
    /// Read the NTAG access configuration.
    /// Prior to this command, the card has to be selected and authenticated
    /// </summary>
    /// <returns>The access configuration</returns>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public NTAGAccessConfig ReadNTAGAccessConfig()
    {
      // +NACFG: 4,1,0
      string[] response = SplitLine(GetCommand("AT+NACFG?")[8..]);
      return new NTAGAccessConfig(int.Parse(response[0]), response[1] != "0", int.Parse(response[2]));
    }
    /// <summary>
    /// Configure the NTAG mirror configuration.
    /// Prior to this command, the card has to be selected and authenticated
    /// </summary>
    /// <param name="config">The mirror configuration</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void WriteNTAGMirrorConfig(NTAGMirrorConfig config)
    {
      SetCommand($"AT+NMCFG={config.Mode},{config.Page},{config.Offset}");
    }
    /// <summary>
    /// Read the NTAG mirror configuration.
    /// Prior to this command, the card has to be selected and authenticated
    /// </summary>
    /// <returns>The mirror configuration</returns>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public NTAGMirrorConfig ReadNTAGMirrorConfig()
    {
      // +NMCFG: BOTH,4,0
      string[] response = SplitLine(GetCommand("AT+NMCFG?")[8..]);
      return new NTAGMirrorConfig((NTAGMirrorConfig.MirrorMode)Enum.Parse(typeof(NTAGMirrorConfig.MirrorMode),
                                  response[0]), int.Parse(response[1]), int.Parse(response[2]));
    }
    /// <summary>
    /// Configure the NTAG counter configuration.
    /// Prior to this command, the card has to be selected and authenticated
    /// </summary>
    /// <param name="config">The counter configuration</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void WriteNTAGCounterConfig(NTAGCounterConfig config)
    {
      SetCommand($"AT+NCCFG={(config.Enable ? 1 : 0)},{(config.EnablePasswordProtection ? 1 : 0)}");
    }
    /// <summary>
    /// Read the NTAG counter configuration.
    /// Prior to this command, the card has to be selected and authenticated
    /// </summary>
    /// <returns>The counter configuration</returns>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public NTAGCounterConfig ReadNTAGCounterConfig()
    {
      // +NCCFG: BOTH,4,0
      string[] response = SplitLine(GetCommand("AT+NCCFG?")[8..]);
      return new NTAGCounterConfig(response[0] != "0", response[1] != "0");
    }
    /// <summary>
    /// Enable or Disable the NTAG strong modulation
    /// Prior to this command, the card has to be selected and authenticated
    /// </summary>
    /// <param name="enable">true for enable the strong modulation</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void WriteNTAGStrongModulation(bool enable)
    {
      SetCommand($"AT+NDCFG={(enable ? 1 : 0)}");
    }
    /// <summary>
    /// Read if the NTAG strong modulation is enabled
    /// Prior to this command, the card has to be selected and authenticated
    /// </summary>
    /// <returns>true if enabled</returns>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public bool ReadNTAGStrongModulation()
    {
      return GetCommand("AT+NDCFG?")[8..] == "1";
    }
    /// <summary>
    /// Permanently lock the NTAG configuration
    /// Prior to this command, the card has to be selected and authenticated
    /// </summary>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void LockNTAGConfig()
    {
      SetCommand("AT+NCLK");
    }
    /// <summary>
    /// Check if the NTAG configuration is locked.
    /// Prior to this command, the card has to be selected and authenticated
    /// </summary>
    /// <returns>True if the configuration is locked</returns>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public bool IsNTAGConfigLooked()
    {
      return GetCommand("AT+NCLK?")[7..] == "1";
    }
    /// <summary>
    /// Read the NTAG counter.
    /// Prior to this command, the card has to be selected and authenticated
    /// </summary>
    /// <returns></returns>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public int ReadNTAGCounter()
    {
      return int.Parse(GetCommand("AT+NCNT?")[7..]);
    }
    /// <summary>
    /// Lock a NTAG page. The lock is irreversible.
    /// Prior to this command, the card has to be selected and authenticated
    /// </summary>
    /// <param name="page">the page number to lock</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void LockNTAGPage(int page)
    {
      SetCommand($"AT+NLK={page}");
    }
    /// <summary>
    /// Set the NTAG block-lock-bits. The block-lock bits are used to lock the lock bits.
    /// Refer to the NTAG data sheet for details.
    /// Prior to this command, the card has to be selected and authenticated
    /// </summary>
    /// <param name="page">the page number to lock the lock bits for.</param>
    /// <exception cref="TransponderException">
    /// If the transponder return an error, further details in the exception message
    /// </exception>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void LockNTAGBlockLock(int page)
    {
      SetCommand($"AT+NBLK={page}");
    }
    /// <summary>
    /// Parse the error message and return the reader or transponder exception
    /// </summary>
    /// <param name="response">error response</param>
    /// <returns></returns>
    protected override MetratecReaderException ParseErrorResponse(String response)
    {
      switch (response)
      {
        // Tag Errors:
        case "No Tag selected": // NFC_CORE_ERROR_NOT_SELECTED:
        case "Wrong Tag type": // NFC_CORE_ERROR_TAG_TYPE:
        case "Unexpected Tag response": // NFC_CORE_ERROR_UNEXPECTED_RESPONSE:
        case "Block out of range": // NFC_CORE_ERROR_BLOCK_RANGE:
        case "Not authenticated": // NFC_CORE_ERROR_NOT_AUTHENTICATED:
        case "Access prohibited": // NFC_CORE_ERROR_ACCESS_PROHIBITED:
        case "Wrong block size": // NFC_CORE_ERROR_BLOCK_SIZE:
        case "Tag timeout": // NFC_CORE_ERROR_IO_TIMEOUT:
        case "Collision error": // NFC_CORE_ERROR_COLLISION:
        case "Overflow": // NFC_CORE_ERROR_OVERFLOW:
        case "Parity error": // NFC_CORE_ERROR_PARITY:
        case "Framing error": // NFC_CORE_ERROR_FRAMING:
        case "Protocol violation": // NFC_CORE_ERROR_PROTOCOL_VIOLATION:
        case "Authentication failure": // NFC_CORE_ERROR_AUTHENTICATION:
        case "Length error": // NFC_CORE_ERROR_LENGTH:
        case "Received NAK": // NFC_CORE_ERROR_NAK:
        case "NTAG invalid argument": // NFC_CORE_ERROR_NTAG_INVALID_ARG:
        case "NTAG parity/crc error": // NFC_CORE_ERROR_NTAG_PARITY:
        case "NTAG auth limit reached": // NFC_CORE_ERROR_NTAG_AUTH_LIMIT:
        case "NTAG EEPROM failure (maybe locked?)": // NFC_CORE_ERROR_NTAG_EEPROM:
        case "Mifare NAK 0": // NFC_CORE_ERROR_MIFARE_NAK0:
        case "Mifare NAK 1": // NFC_CORE_ERROR_MIFARE_NAK1:
        case "Mifare NAK 3": // NFC_CORE_ERROR_MIFARE_NAK3:
        case "Mifare NAK 4": // NFC_CORE_ERROR_MIFARE_NAK4:
        case "Mifare NAK 5": // NFC_CORE_ERROR_MIFARE_NAK5:
        case "Mifare NAK 6": // NFC_CORE_ERROR_MIFARE_NAK6:
        case "Mifare NAK 7": // NFC_CORE_ERROR_MIFARE_NAK7:
        case "Mifare NAK 8": // NFC_CORE_ERROR_MIFARE_NAK8:
        case "Mifare NAK 9": // NFC_CORE_ERROR_MIFARE_NAK9:
        case "ISO15 custom command error": // NFC_CORE_ERROR_ISO15_CUSTOM_CMD_ERR:
        case "ISO15 command not supported": // NFC_CORE_ERROR_ISO15_CMD_NOT_SUPPORTED:
        case "ISO15 command not recognized": // NFC_CORE_ERROR_ISO15_CMD_NOT_RECOGNIZED:
        case "ISO15 option not supported": // NFC_CORE_ERROR_ISO15_OPT_NOT_SUPPORTED:
        case "ISO15 no information": // NFC_CORE_ERROR_ISO15_NO_INFO:
        case "ISO15 block not available": // NFC_CORE_ERROR_ISO15_BLOCK_NOT_AVAIL:
        case "ISO15 block locked": // NFC_CORE_ERROR_ISO15_BLOCK_LOCKED:
        case "ISO15 content change failure": // NFC_CORE_ERROR_ISO15_CONTENT_CHANGE_FAIL:
        case "ISO15 block programming failure": // NFC_CORE_ERROR_ISO15_BLOCK_PROGRAMMING_FAIL:
        case "ISO15 block protected": // NFC_CORE_ERROR_ISO15_BLOCK_PROTECTED:
        case "ISO15 cryptographic error": // NFC_CORE_ERROR_ISO15_CRYPTO:
          return new TransponderException(response);
        // Reader Errors:
        // case "No such protocol": // NFC_CORE_ERROR_NO_SUCH_PROTO:
        // case "No frontend selected": // NFC_CORE_ERROR_NO_FRONTEND:
        // case "Failed to initialize frontend": // NFC_CORE_ERROR_FRONTEND_INIT:
        // case "Wrong operation mode": // NFC_CORE_ERROR_OP_MODE:
        // case "Invalid parameter": // NFC_CORE_ERROR_INVALID_PARAM:
        // case "Command failed": // NFC_CORE_ERROR_COMMAND_FAILED:
        // case "IO error": // NFC_CORE_ERROR_IO:
        // case "Timeout": // NFC_CORE_ERROR_TIMEOUT:
        // case "Temperature error": // NFC_CORE_ERROR_TEMPERATURE:
        // case "Resource error": // NFC_CORE_ERROR_RESOURCE:
        // case "RF error": // NFC_CORE_ERROR_RF:
        // case "Noise error": // NFC_CORE_ERROR_NOISE:
        // case "Aborted": // NFC_CORE_ERROR_ABORTED:
        // case "Authentication delay": // NFC_CORE_ERROR_AUTH_DELAY:
        // case "Unsupported parameter": // NFC_CORE_ERROR_UNSUPPORTED_PARAM:
        // case "Unsupported command": // NFC_CORE_ERROR_UNSUPPORTED_CMD:
        // case "Wrong use condition": // NFC_CORE_ERROR_USE_CONDITION:
        // case "Key error": // NFC_CORE_ERROR_KEY:
        // case "No key at given index": // NFC_CORE_ERROR_KEYSTORE_NO_KEY:
        // case "Could not save key": // NFC_CORE_ERROR_KEYSTORE_SAVE_ERROR:
        // case "Feedback out of range": // NFC_CORE_ERROR_FEEDBACK_OUT_OF_RANGE:
        // case "Invalid feedback string": // NFC_CORE_ERROR_FEEDBACK_PARSING_ERROR:
        // case "Feedback already running": // NFC_CORE_ERROR_FEEDBACK_ALREADY_RUNNING:
        // case "Unknown Error":  // NFC_CORE_ERROR_UNKNOWN:
        default:
          return new MetratecReaderException(response);
      }
    }
  }

  /// <summary>
  /// RF interface mode for the tag communication
  /// </summary>
  public enum NfcReaderMode
  {
    /// <summary>
    /// Automatic mode, detect iso15 and iso14a transponder
    /// </summary>
    AUTO,
    /// <summary>
    /// Iso15 mode, needed for execute iso15 tag commands
    /// </summary>
    ISO15,
    /// <summary>
    /// Iso15 mode, needed for execute iso14a tag commands
    /// </summary>
    ISO14A,
  }

  /// <summary>
  /// The Inventory settings for the nfc reader
  /// </summary>
  public class InventorySettingsNfc
  {
    /// <summary>
    /// Add transponder details
    /// </summary>
    /// <value>True, to add transponder details to the inventory call</value>
    public bool AddTagDetails { get; set; }
    /// <summary>
    /// Only new tags filter only has an effect in ISO15 mode
    /// </summary>
    /// <value>if true, only new tags are reported</value>
    public bool OnlyNewTags { get; set; }
    /// <summary>
    /// Uso only one slot for communication, only has an effect in ISO15 mode.
    /// </summary>
    /// <value>if true, the tid of each tag is reported</value>
    public bool SingleSlot { get; set; }
    /// <summary>
    /// Create the inventory settings
    /// </summary>
    /// <returns></returns>  
    public InventorySettingsNfc() : this(false, false, false) { }
    /// <summary>
    /// Create the inventory settings with the given parameters
    /// </summary>
    /// <param name="addTagDetails">if true, only new tags are reported</param>
    /// <param name="onlyNewTags">if true, the Received Signal Strength Indication of each tag is reported</param>
    /// <param name="singleSlot">if true, the tid of each tag is reported</param>
    public InventorySettingsNfc(bool addTagDetails, bool onlyNewTags, bool singleSlot)
    {
      AddTagDetails = addTagDetails;
      OnlyNewTags = onlyNewTags;
      SingleSlot = singleSlot;
    }
  }

  /// <summary>
  /// Mifare Classic Key Type
  /// </summary>
  public enum KeyType
  {
    /// <summary>
    /// Key A
    /// </summary>
    A,
    /// <summary>
    /// Key B
    /// </summary>
    B
  }
  /// <summary>
  /// NTAG Access configuration
  /// </summary>
  public class NTAGAccessConfig
  {
    /// <summary>
    /// Page address from which password authentication is required
    /// </summary>
    public int StartAddress { get; set; }
    /// <summary>
    /// Indicates if read is also protected
    /// </summary>
    public bool ReadProtected { get; set; }
    /// <summary>
    /// Number of authentication attempts
    /// </summary>
    public int MaxAttempts { get; set; }
    /// <summary>
    /// Create a new access configuration
    /// </summary>
    /// <param name="startAddress">Page address from which password authentication is required</param>
    /// <param name="readProtected">Number of authentication attempts</param>
    /// <param name="maxAttempts">Number of authentication attempts</param>
    public NTAGAccessConfig(int startAddress, bool readProtected, int maxAttempts)
    {
      StartAddress = startAddress;
      ReadProtected = readProtected;
      MaxAttempts = maxAttempts;
    }
  }
  /// <summary>
  /// NTAG Mirror Configuration
  /// </summary>
  public class NTAGMirrorConfig
  {
    /// <summary>
    /// NTAG Mirror Mode
    /// </summary>
    public enum MirrorMode
    {
      /// <summary>
      /// Mirror mode off
      /// </summary>
      OFF,
      /// <summary>
      /// UID mirror mode
      /// </summary>
      UID,
      /// <summary>
      /// CNT mirror mode
      /// </summary>
      /// <value></value>
      CNT,
      /// <summary>
      /// Use both mirror modes
      /// </summary>
      BOTH
    }
    /// <summary>
    /// The mirror mode
    /// </summary>
    public MirrorMode Mode { get; set; }
    /// <summary>
    /// The start page where the configured data is mirrored to.
    /// </summary>
    public int Page { get; set; }
    /// <summary>
    /// Byte Offset of the mirrored data in the Mirror Page.
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// Create the mirror configuration
    /// </summary>
    /// <param name="mode">The mirror mode</param>
    /// <param name="page">The start page where the configured data is mirrored to.</param>
    /// <param name="offset">Byte Offset of the mirrored data in the Mirror Page.</param>
    public NTAGMirrorConfig(MirrorMode mode, int page, int offset)
    {
      Mode = mode;
      Page = page;
      Offset = offset;
    }
  }
  /// <summary>
  /// NTAG counter config
  /// </summary>
  public class NTAGCounterConfig
  {
    /// <summary>
    /// Enable counter
    /// </summary>
    public bool Enable;
    /// <summary>
    /// Enable password protected
    /// </summary>
    public bool EnablePasswordProtection;
    /// <summary>
    /// Create a new config
    /// </summary>
    /// <param name="enable">Enable counter</param>
    /// <param name="enablePasswordProtection">Enable password protected</param>
    public NTAGCounterConfig(bool enable, bool enablePasswordProtection)
    {
      Enable = enable;
      EnablePasswordProtection = enablePasswordProtection;
    }
  }
}