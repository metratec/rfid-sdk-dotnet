using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The reader class for the metratec uhf readers based on the AT protocol
  /// </summary>
  public class UhfReaderAT : MetratecReaderAT<UhfTag>
  {

    private InventorySettings? _inventorySettings;
    /// <summary>
    /// Create a new instance of the UhfReaderAT class.
    /// </summary>
    /// <param name="connection">The connection interface</param>
    public UhfReaderAT(ICommunicationInterface connection) : base(connection)
    {
    }

    /// <summary>
    /// Create a new instance of the UhfReaderAT class.
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    public UhfReaderAT(ICommunicationInterface connection, ILogger logger) : base(connection, logger)
    {
    }

    /// <summary>
    /// Create a new instance of the UhfReaderAT class.
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// /// <param name="id">The reader id</param>

    public UhfReaderAT(ICommunicationInterface connection, string id) : base(connection, id)
    {
    }

    /// <summary>
    /// Create a new instance of the UhfReaderAT class.
    /// </summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="id">The reader id</param>
    /// <param name="logger">The connection interface</param>
    public UhfReaderAT(ICommunicationInterface connection, string id, ILogger logger) : base(connection, id, logger)
    {
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
      SetCommand("ATE1");
      StopInventoryReport();
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
    }
    /// <summary>
    /// Return the current inventory settings
    /// </summary>
    /// <returns>The current inventory settings</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public InventorySettings GetInventorySettings()
    {
      if (null != _inventorySettings)
      {
        return _inventorySettings;
      }
      string[] split = SplitLine(GetCommand("AT+INVS?")[7..]); // +INVS: 0,0,0
      _inventorySettings = new InventorySettings(split[0] == "1", split[1] == "1",
                                                 split[2] == "1", split[3] == "1");
      return _inventorySettings;
    }
    /// <summary>
    /// Sets the inventory settings
    /// </summary>
    /// <param name="settings">the inventory settings</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetInventorySettings(InventorySettings settings)
    {
      SetCommand($"AT+INVS={(settings.OnlyNewTag ? "1" : "0")},{(settings.WithRssi ? "1" : "0")}," +
                 $"{(settings.WithTid ? "1" : "0")},{(settings.FastStart ? "1" : "0")}");
      _inventorySettings = settings;
    }
    /// <summary>
    /// Returns the tag count setting
    /// </summary>
    /// <returns>the tag count setting</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public TagCountSetting GetTagCountSetting()
    {
      string response = GetCommand("AT+Q?");
      // +Q: 4,2,15
      string[] values = SplitLine(response[4..]);
      TagCountSetting setting = new((int)Math.Pow(2, int.Parse(values[0])));
      if (values.Length > 1)
      {
        setting.Min = (int)Math.Pow(2, int.Parse(values[1]));
        setting.Max = (int)Math.Pow(2, int.Parse(values[2]));
      }
      return setting;
    }
    /// <summary>
    /// Sets the expected tag count
    /// </summary>
    /// <param name="settings">the tag count settings</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetTagCountSettings(TagCountSetting settings)
    {
      int start = 0;
      while (settings.Start >= Math.Pow(2, start))
      {
        start++;
      }
      start--;
      if (settings.Max < 0 || settings.Min < 0)
      {
        SetCommand($"AT+Q={start}");
        return;
      }
      int min = 0;
      while (settings.Min >= Math.Pow(2, min))
      {
        min++;
      }
      min--;
      int max = 0;
      while (settings.Max > Math.Pow(2, max))
      {
        max++;
      }
      SetCommand($"AT+Q={start},{min},{max}");
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
      SetCommand($"AT+PWR={power}");
    }
    /// <summary>
    /// Get the current reader power
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public int GetPower()
    {
      string response = GetCommand("AT+PWR?");
      //+PWR: 20
      return int.Parse(response[6..]);
    }
    /// <summary>
    /// Set the region
    /// </summary>
    /// <param name="region">the region to set</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetRegion(UHF_REGION region)
    {
      SetCommand($"AT+REG={region}");
    }
    /// <summary>
    /// Get the current region
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public UHF_REGION GetRegion()
    {
      string response = GetCommand("AT+REG?");
      //+REG: ETSI
      return (UHF_REGION)Enum.Parse(typeof(UHF_REGION), response[6..]);
    }

    /// <summary>
    /// Scan for the current inventory
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public override List<UhfTag> GetInventory()
    {
      List<UhfTag> tags = SingleAntennaInUse ? ParseInventory(GetCommand("AT+INV"), "+INV: ".Length) :
                                             ParseInventory(GetCommand("AT+MINV"), "+MINV: ".Length);
      FireInventoryEvent(tags, false);
      return tags;
    }

    /// <summary>
    /// Get the current inventory report
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> GetInventoryReport()
    {
      List<UhfTag> tags = ParseInventoryReport(GetCommand("AT+INVR"), "+INVR: ".Length);
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
      SetCommand(SingleAntennaInUse ? "AT+CINV" : "AT+CMINV");
    }

    /// <summary>
    /// Start the continuous inventory report scan.
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void StartInventoryReport()
    {
      SetCommand("AT+CINVR");
    }

    /// <summary>
    /// Stops the continuous inventory report scan.
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void StopInventoryReport()
    {
      try
      {
        SetCommand("AT+BINVR");
      }
      catch (MetratecReaderException e)
      {
        if (!e.ToString().Contains("is not running"))
        {
          throw;
        }
      }
    }
    /// <summary>
    /// Parse the inventory event (+CINV, +CMINV, +CINVR)
    /// </summary>
    /// <param name="response"></param>
    protected override void HandleInventoryEvent(string response)
    {
      try
      {
        if (response[2] == 'M')
        {
          List<UhfTag> tags = ParseInventory(response, "+CMINV: ".Length);
          FireInventoryEvent(tags, true);
        }
        else if (response[5] == 'R')
        {
          List<UhfTag> tags = ParseInventoryReport(response, "+CINVR: ".Length);
          FireInventoryEvent(tags, true);
        }
        else
        {
          List<UhfTag> tags = ParseInventory(response, "+CINV: ".Length);
          FireInventoryEvent(tags, true);
        }
      }
      catch (MetratecReaderException e)
      {
        Logger.LogDebug("{} Error parse inventory - {}", id, e);
      }
    }

    private List<UhfTag> ParseInventory(string response, int prefixLength, bool isReport = false, bool throwError = false)
    {
      // +CINV: 3034257BF468D480000003EC,E200600311753E33,1755 +CINV: <ROUND FINISHED, ANT=2>
      // +INV: 0209202015604090990000145549021C,E200600311753F23,1807
      // available messages: <Antenna Error> <NO TAGS FOUND> <ROUND FINISHED, ANT=2>
      DateTime timestamp = DateTime.Now;
      List<UhfTag> tags = new();
      int antenna = -1;
      string error = "";
      foreach (string tagInfo in SplitResponse(response))
      {
        if (tagInfo[0] != '+')
        {
          continue;
        }
        string[] split = SplitLine(tagInfo[prefixLength..]);
        if (split[0][0] == '<')
        {
          // message
          switch (split[0][1])
          {
            case 'R': //Round finished
              antenna = int.Parse(split[1].Substring(5, 1));
              foreach (UhfTag tag in tags)
              {
                tag.Antenna = antenna;
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
          UhfTag tag = new(split[0], timestamp, CurrentAntennaPort);
          if (_inventorySettings!.WithTid)
          {
            tag.TID = split[1];
          }
          if (_inventorySettings!.WithRssi)
          {
            tag.RSSI = int.Parse(split[_inventorySettings.WithTid ? 2 : 1]);
          }
          if (isReport)
          {
            tag.SeenCount = int.Parse(split[^1]);
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
          Logger.LogWarning("Inventory warning ({}) - {}", tagInfo, e.Message);
        }
      }
      if (error.Length > 0)
      {
        throw new MetratecReaderException((0 > antenna ? $"Antenna {antenna}: " : "") + error);
      }
      if (isReport)
      {
        foreach (UhfTag tag in tags)
        {
          tag.Antenna = 0;
        }
      }
      return tags;
    }

    private List<UhfTag> ParseInventoryReport(string response, int prefixLength)
    {
      return ParseInventory(response, prefixLength, true);
    }
    /// <summary>
    /// Set the reader mask
    /// </summary>
    /// <param name="membank">the memory bank to check</param>
    /// <param name="startAddress">the start address</param>
    /// <param name="mask">the mask</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetMask(MEMBANK_GEN2 membank, int startAddress, string mask)
    {
      SetCommand($"AT+MSK={membank},{startAddress},{mask}");
    }
    /// <summary>
    /// Set the epc mask
    /// </summary>
    /// <param name="mask">the mask</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetEpcMask(string mask)
    {
      SetMask(MEMBANK_GEN2.EPC, 0, mask);
    }
    /// <summary>
    /// Set the epc mask
    /// </summary>
    /// <param name="startAddress">the start address</param>
    /// <param name="mask">the mask</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetEpcMask(int startAddress, string mask)
    {
      SetMask(MEMBANK_GEN2.EPC, startAddress, mask);
    }
    /// <summary>
    /// Reset/Disable the current reader mask
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void ResetMask()
    {
      SetCommand("AT+MSK=OFF");
    }
    /// <summary>
    /// Set the reader bit mask
    /// </summary>
    /// <param name="membank">the memory bank to check</param>
    /// <param name="startAddress">the start address</param>
    /// <param name="mask">the binary mask, e.g. '0110'</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetBitmask(MEMBANK_GEN2 membank, int startAddress, string mask)
    {
      SetCommand($"AT+BMSK={membank},{startAddress},{mask}");
    }
    /// <summary>
    /// Reset/Disable the current reader bitmask
    /// </summary>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void ResetBitmask()
    {
      SetCommand("AT+BMSK=OFF");
    }

    /// <summary>
    /// Read tag data
    /// </summary>
    /// <param name="memory">the memory bank to read [TID, USR, EPC]</param>
    /// <param name="startAddress">the start address</param>
    /// <param name="length">the bytes to read</param>
    /// <param name="epcMask">the epc mask to use, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> ReadTagData(MEMBANK_GEN2 memory, int startAddress, int length, String epcMask = "")
    {
      String response = GetCommand($"AT+READ={memory},{startAddress},{length}{(epcMask.Length != 0 ? $",{epcMask}" : "")}");
      List<UhfTag> tags = new();
      DateTime timestamp = DateTime.Now;
      foreach (String tagInfo in SplitResponse(response))
      {
        string[] values = SplitLine(tagInfo[7..]);
        UhfTag tag = new(values[0], timestamp, CurrentAntennaPort);
        if (values[1].StartsWith("OK"))
        {
          switch (memory)
          {
            case MEMBANK_GEN2.USR:
              tag.Data = values[2];
              break;
            case MEMBANK_GEN2.TID:
              tag.TID = values[2];
              break;
          }
        }
        else
        {
          tag.HasError = true;
          tag.Message = values[1];
        }
        tags.Add(tag);
      }
      return tags;
    }
    /// <summary>
    /// Read the tag TIDs 
    /// </summary>
    /// <param name="startAddress">startAddress</param>
    /// <param name="length">bytes to read from the tid</param>
    /// <param name="epcMask">the epc mask to use, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> ReadTagTid(int startAddress, int length, String epcMask = "")
    {
      return ReadTagData(MEMBANK_GEN2.TID, startAddress, length, epcMask);
    }
    /// <summary>
    /// Read the tag user data 
    /// </summary>
    /// <param name="startAddress">startAddress</param>
    /// <param name="length">bytes to read from the user data</param>
    /// <param name="epcMask">the epc mask to use, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> ReadTagUsrData(int startAddress, int length, String epcMask = "")
    {
      return ReadTagData(MEMBANK_GEN2.USR, startAddress, length, epcMask);
    }
    /// <summary>
    /// Write data to a tag
    /// </summary>
    /// <param name="memory">tag memory to use</param>
    /// <param name="startAddress">start address</param>
    /// <param name="data">data, hex string</param>
    /// <param name="epcMask">ecp mask, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> WriteTagData(MEMBANK_GEN2 memory, int startAddress, string data, string epcMask = "")
    {
      string response = GetCommand($"AT+WRT={memory},{startAddress},{data}{(epcMask.Length != 0 ? $",{epcMask}" : "")}");
      return ParseWriteResponse(response, "+WRT: ".Length, DateTime.Now);
    }

    private List<UhfTag> ParseWriteResponse(string response, int prefixLength, DateTime timestamp)
    {
      List<UhfTag> tags = new();
      foreach (String tagInfo in SplitResponse(response))
      {
        string[] values = SplitLine(tagInfo[prefixLength..]);
        UhfTag tag = new(values[0], timestamp, CurrentAntennaPort);
        if (!values[1].StartsWith("OK"))
        {
          tag.HasError = true;
          tag.Message = values[1];
        }
        tags.Add(tag);
      }
      return tags;
    }

    /// <summary>
    /// Write the user data of a tag
    /// </summary>
    /// <param name="startAddress">start address</param>
    /// <param name="data">data, hex string</param>
    /// <param name="epcMask">ecp mask, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> WriteTagUsrData(int startAddress, string data, string epcMask = "")
    {
      return WriteTagData(MEMBANK_GEN2.USR, startAddress, data, epcMask);
    }

    /// <summary>
    /// Killing tags
    /// </summary>
    /// <param name="password">the kill password</param>
    /// <param name="epcMask">the epc mask to use, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> KillTag(String password, String epcMask = "")
    {
      String resp = GetCommand($"AT+KILL={password}{(epcMask.Length != 0 ? $",{epcMask}" : "")}");
      // +KILL: ABCD01237654321001234567,ACCESS ERROR<CR><LF>
      return ParseWriteResponse(resp, 7, DateTime.Now);
    }

    /// <summary>
    /// Locking a tag memory
    /// </summary>
    /// <param name="membank">tag memory to lock</param>
    /// <param name="password">the kill password</param>
    /// <param name="epcMask">the epc mask to use, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> LockTag(MEMBANK_GEN2 membank, String password, String epcMask = "")
    {
      String resp = GetCommand($"AT+LCK={membank},{password}{(epcMask.Length != 0 ? $",{epcMask}" : "")}");
      // +LCK: ABCD01237654321001234567,ACCESS ERROR<CR><LF>
      return ParseWriteResponse(resp, 6, DateTime.Now);
    }

    /// <summary>
    /// Locking user memory of a tag
    /// </summary>
    /// <param name="password">the kill password</param>
    /// <param name="epcMask">the epc mask to use, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> LockTagData(String password, String epcMask = "")
    {
      return LockTag(MEMBANK_GEN2.USR, password, epcMask);
    }


    /// <summary>
    /// Locking epc memory of the tag
    /// </summary>
    /// <param name="password">the kill password</param>
    /// <param name="epcMask">the epc mask to use, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> LockTagEpc(String password, String epcMask = "")
    {
      return LockTag(MEMBANK_GEN2.EPC, password, epcMask);
    }

    /// <summary>
    /// Permanent locking of a tag memory
    /// </summary>
    /// <param name="membank">tag memory to lock</param>
    /// <param name="password">the kill password</param>
    /// <param name="epcMask">the epc mask to use, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> LockTagPermament(MEMBANK_GEN2 membank, String password, String epcMask = "")
    {
      String resp = GetCommand($"AT+PLCK={membank},{password}{(epcMask.Length != 0 ? $",{epcMask}" : "")}");
      // +PLCK: ABCD01237654321001234567,ACCESS ERROR<CR><LF>
      return ParseWriteResponse(resp, 7, DateTime.Now);
    }

    /// <summary>
    /// Permanent locking of the user memory of the tag
    /// </summary>
    /// <param name="password">the kill password</param>
    /// <param name="epcMask">the epc mask to use, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> LockTagMemoryPermament(String password, String epcMask = "")
    {
      return LockTagPermament(MEMBANK_GEN2.USR, password, epcMask);
    }

    /// <summary>
    /// Permanent locking of the epc memory of the tag
    /// </summary>
    /// <param name="password">the kill password</param>
    /// <param name="epcMask">the epc mask to use, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> LockTagEpcPermament(String password, String epcMask = "")
    {
      return LockTagPermament(MEMBANK_GEN2.EPC, password, epcMask);
    }

    /// <summary>
    /// Unlocking of a tag memory
    /// </summary>
    /// <param name="membank">tag memory to lock</param>
    /// <param name="password">the kill password</param>
    /// <param name="epcMask">the epc mask to use, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> UnlockTag(MEMBANK_GEN2 membank, String password, String epcMask = "")
    {
      String resp = GetCommand($"AT+ULCK={membank},{password}{(epcMask.Length != 0 ? $",{epcMask}" : "")}");
      // +ULCK: ABCD01237654321001234567,ACCESS ERROR<CR><LF>
      return ParseWriteResponse(resp, 7, DateTime.Now);
    }

    /// <summary>
    /// Unlocking the user memory of the tag
    /// </summary>
    /// <param name="password">the kill password</param>
    /// <param name="epcMask">the epc mask to use, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> UnlockTagData(String password, String epcMask = "")
    {
      return UnlockTag(MEMBANK_GEN2.USR, password, epcMask);
    }

    /// <summary>
    /// Unlocking the epc memory of the tag
    /// </summary>
    /// <param name="password">the kill password</param>
    /// <param name="epcMask">the epc mask to use, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> UnlockTagEpc(String password, String epcMask = "")
    {
      return UnlockTag(MEMBANK_GEN2.EPC, password, epcMask);
    }

    /// <summary>
    /// Change the kill password of a tag
    /// </summary>
    /// <param name="password">the current kill password</param>
    /// <param name="newPassword">the new kill password</param>
    /// <param name="epcMask">the epc mask to use, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> ChangeKillPassword(String password, String newPassword, String epcMask = "")
    {
      String resp = GetCommand($"AT+PWD=KILL,{password},{newPassword}{(epcMask.Length != 0 ? $",{epcMask}" : "")}");
      // +PWD: ABCD01237654321001234567,ACCESS ERROR<CR><LF>
      return ParseWriteResponse(resp, 6, DateTime.Now);
    }

    /// <summary>
    /// Change the lock password of a tag
    /// </summary>
    /// <param name="password">the current lock password</param>
    /// <param name="newPassword">the new lock password</param>
    /// <param name="epcMask">the epc mask to use, optional</param>
    /// <returns>List with processed tags. If the tag has error, the kill was not successful</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTag> ChangeLockPassword(String password, String newPassword, String epcMask = "")
    {
      String resp = GetCommand($"AT+PWD=LCK,{password},{newPassword}{(epcMask.Length != 0 ? $",{epcMask}" : "")}");
      // +PWD: ABCD01237654321001234567,ACCESS ERROR<CR><LF>
      return ParseWriteResponse(resp, 6, DateTime.Now);
    }
    /// <summary>
    /// This command tags to an Impinj M775 tag using the proprietary authentication command.
    /// It sends a random challenge to the transponder and gets the authentication payload in return.
    /// You can use this to check the authenticity of the transponder with Impinj Authentication Service.
    /// For further details, please contact Impinj directly.
    /// </summary>
    /// <returns>a list with the authentication responses</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public List<UhfTagAuth> CallImpinjAuthenticationService()
    {
      string[] responses = SplitResponse(GetCommand("AT+IAS"));
      List<UhfTagAuth> tags = new();
      foreach (string response in responses)
      {
        string[] split = SplitLine(response[6..]);
        if (split[1] == "OK")
        {
          tags.Add(new UhfTagAuth(split[0], split[2], split[3], split[4]));
        }
        else
        {
          tags.Add(new UhfTagAuth(split[0], split[1]));
        }
      }
      return tags;
    }
    /// <summary>
    /// Returns the current selected session. See SetSession for more details.
    /// </summary>
    /// <returns>the current selected session</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public string GetSession()
    {
      string response = GetCommand("AT+SES?");
      return response[6..];
    }
    /// <summary>
    /// Manually select the session according to the EPC Gen 2 Protocol to use during inventory scan.
    /// Default value is "auto" and in most cases this should stay auto.
    /// Only change this if you absolutely know what you are doing and if you can control the types of tags you scan.
    /// Otherwise, unexpected results during inventory scans with "only new tags" active might happen.
    /// </summary>
    /// <param name="sessionId">Session to set ["0", "1", "2", "3", "AUTO"]</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetSession(string sessionId)
    {
      SetCommand($"AT+SES={sessionId}");
    }
    /// <summary>
    /// Returns the current rf mode. See SetRfMode for more details.
    /// </summary>
    /// <returns>the current selected session</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public string GetRfMode()
    {
      string response = GetCommand("AT+RFM?");
      return response[6..];
    }
    /// <summary>
    /// Configure the internal RF communication settings between tag and reader. Each mode ID corresponds
    /// to a set of RF parameters that fit together. Not all devices support all modes and not all modes can
    /// be access in all regions.
    /// See reader description for more detail.
    /// </summary>
    /// <param name="modeId">mode id</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetRfMode(string modeId)
    {
      SetCommand($"AT+RFM={modeId}");
    }
    /// <summary>
    /// The RFID tag IC manufacturer Impinj has added two custom features to its tag ICs 
    /// that are not compatible with tag ICs from other manufacturers. Activate these features with this command.
    /// But make sure that you only use tags with Impinj ICs like Monza6 or M7xx or M8xx series.
    /// Tags from other manufacturers will most likely not answer at all when those options are active!
    /// </summary>
    /// <param name="settings">the settings</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public void SetCustomImpinjSettings(CustomImpinjSettings settings)
    {
      SetCommand($"AT+ICS={(settings.FastId ? "1" : "0")},{(settings.TagFocus ? "1" : "0")}");
    }
    /// <summary>
    /// Gets the current custom impinj settings
    /// </summary>
    /// <returns>the setting</returns>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public CustomImpinjSettings GetCustomImpinjSettings()
    {
      string[] split = SplitLine(GetCommand("AT+ICS?")[6..]);
      return new CustomImpinjSettings(split[0] == "1", split[1] == "1");
    }

    /// <summary>
    /// Parse the error message and return the reader or transponder exception
    /// </summary>
    /// <param name="response">error response</param>
    /// <returns></returns>
    protected override MetratecReaderException ParseErrorResponse(String response)
    {
      return new MetratecReaderException(response);
    }
  }
  /// <summary>
  /// Tag memory
  /// </summary>
  public enum MEMBANK_GEN2
  {
    /// <summary>
    /// The EPC Membank. Contains CRC, PC and EPC.
    /// </summary>
    EPC,
    /// <summary>
    /// The tag ID of the tag (sometimes contains a unique ID, sometimes only a manufacturer code,
    /// depending on the tag type)
    /// </summary>
    TID,
    /// <summary>
    /// The optional user memory some tags have
    /// </summary>
    USR,
    /// <summary>
    /// The optional user memory some tags have
    /// </summary>
    LCK,
    /// <summary>
    /// The optional user memory some tags have
    /// </summary>
    KILL,
  }

  /// <summary>
  /// Inventory settings
  /// </summary>
  public class InventorySettings
  {
    /// <summary>
    /// Only new tag setting
    /// </summary>
    /// <value>True, to report only new tags</value>
    public bool OnlyNewTag { get; set; }
    /// <summary>
    /// With Rssi setting
    /// </summary>
    /// <value>if true, the Received Signal Strength Indication of each tag is reported</value>
    public bool WithRssi { get; set; }
    /// <summary>
    /// With Tid setting
    /// </summary>
    /// <value>if true, the tid of each tag is reported</value>
    public bool WithTid { get; set; }
    /// <summary>
    /// Enable for do an inventory without putting all tags into session state
    /// </summary>
    public bool FastStart { get; set; }
    /// <summary>
    /// Create the inventory settings
    /// </summary>
    /// <returns></returns>  
    public InventorySettings() : this(false, false, false, false) { }
    /// <summary>
    /// Create the inventory settings with the given parameters
    /// </summary>
    /// <param name="onlyNewTags">if true, only new tags are reported</param>
    /// <param name="withRssi">if true, the Received Signal Strength Indication of each tag is reported</param>
    /// <param name="withTid">if true, the tid of each tag is reported</param>
    /// <param name="fastStart">if true, an inventory without putting all tags into session state</param>
    public InventorySettings(bool onlyNewTags, bool withRssi, bool withTid, bool fastStart)
    {
      this.OnlyNewTag = onlyNewTags;
      this.WithRssi = withRssi;
      this.WithTid = withTid;
      this.FastStart = fastStart;
    }
  }
  /// <summary>
  /// Tag Report id to use
  /// </summary>
  public enum ReportId
  {

    /// <summary>
    /// Report the EPC of the tags
    /// </summary>
    EPC,
    /// <summary>
    /// Report the TID of the tags
    /// </summary>
    TID
  }
  /// <summary>
  /// Uhf reader region setting for the uhf reader based on the at protocol
  /// </summary>
  public enum UHF_REGION
  {

    /// <summary>
    /// ETSI
    /// </summary>
    ETSI,
    /// <summary>
    /// ETSI_HIGH
    /// </summary>
    ETSI_HIGH,
    /// <summary>
    /// FCC
    /// </summary>
    FCC,
  }

  /// <summary>
  /// Inventory report settings for the Pulsar LR reader
  /// </summary>
  public class InventoryReportSettings
  {
    /// <summary>
    /// The tag id to use
    /// </summary>
    /// <value>EPC or TID</value>
    public ReportId ReportId { get; set; }

    /// <summary>
    /// Create the inventory report settings
    /// </summary>
    /// <returns></returns>  
    public InventoryReportSettings() : this(ReportId.EPC) { }
    /// <summary>
    /// Create the inventory report settings with the given parameters
    /// </summary>
    /// <param name="reportId">The tag id to use</param>
    public InventoryReportSettings(ReportId reportId)
    {
      this.ReportId = reportId;
    }
  }
  /// <summary>
  /// Inventory settings for the Pulsar LR reader
  /// </summary>
  public class TagCountSetting
  {
    /// <summary>
    /// The initial expected tag count
    /// </summary>
    /// <value>The initial expected tag count</value>
    public int Start { get; set; }
    /// <summary>
    /// The minimum expected tag count
    /// </summary>
    /// <value>The minimum expected tag count</value>
    public int Min { get; set; }
    /// <summary>
    /// The maximum expected tag count
    /// </summary>
    /// <value>The maximum expected tag count</value>
    public int Max { get; set; }
    /// <summary>
    /// Create the inventory settings
    /// </summary>
    /// <returns></returns>  
    public TagCountSetting() : this(32, 0, 128) { }
    /// <summary>
    /// Create the inventory settings with the given parameters
    /// </summary>
    /// <param name="start">The initial expected tag count</param>
    public TagCountSetting(int start) : this(start, -1, -1) { }

    /// <summary>
    /// Create the inventory settings with the given parameters
    /// </summary>
    /// <param name="start">The initial expected tag count</param>
    /// <param name="min">The minimum expected tag count</param>
    /// <param name="max">The maximum expected tag count</param>
    public TagCountSetting(int start, int min, int max)
    {
      this.Start = start;
      this.Min = min;
      this.Max = max;
    }
  }
  /// <summary>
  /// Transponder response from the authentication service
  /// </summary>
  public class UhfTagAuth
  {
    /// <summary>
    /// Create a new positive response
    /// </summary>
    /// <param name="epc">Transponder epc</param>
    /// <param name="shortTID">The short TID</param>
    /// <param name="response">The response</param>
    /// <param name="challenge">The challenge</param>
    internal UhfTagAuth(string epc, string shortTID, string response, string challenge)
    {
      EPC = epc;
      HasError = false;
      ShortTID = shortTID;
      Response = response;
      Challenge = challenge;

    }
    /// <summary>
    /// Create a failure response
    /// </summary>
    /// <param name="epc">Transponder epc</param>
    /// <param name="errorMessage">Error message</param>
    internal UhfTagAuth(string epc, string errorMessage)
    {
      EPC = epc;
      HasError = true;
      Message = errorMessage;
    }
    /// <summary>
    /// Transponder EPC
    /// </summary>
    public string EPC { get; internal set; }
    /// <summary>
    /// True if the tag contains error information
    /// </summary>
    public bool HasError { get; internal set; }
    /// <summary>
    /// the error message 
    /// </summary>
    public string? Message { get; internal set; }
    /// <summary>
    /// Short Transponder ID
    /// </summary>
    public string? ShortTID { get; internal set; }
    /// <summary>
    /// Transponder response
    /// </summary>
    public string? Response { get; internal set; }
    /// <summary>
    /// Challenge response
    /// </summary>
    public string? Challenge { get; internal set; }
  }
  /// <summary>
  /// Custom impinj settings
  /// </summary>
  public class CustomImpinjSettings
  {
    /// <summary>
    /// Allows to read the TagID together with the EPC and can speed up getting TID data.
    /// </summary>
    public bool FastId { get; set; }

    /// <summary>
    /// Uses a proprietary tag feature where each tag only answers once until it is repowered.
    /// This allows to scan a high number of tags because each tag only answers once and makes
    /// anti-collision easier for the following tags.
    /// </summary>
    public bool TagFocus { get; set; }

    /// <summary>
    /// Create a new instance
    /// </summary>
    /// <returns></returns>  
    public CustomImpinjSettings() : this(false, false) { }
    /// <summary>
    ///  Create the custom impinj settings with the given parameters
    /// </summary>
    /// <param name="fastId">True to allows to read the TagID together with the EPC and can speed up getting TID data</param>
    /// <param name="tagFocus">True to uses a proprietary tag feature where each tag only answers once until it is repowered</param>
    public CustomImpinjSettings(bool fastId, bool tagFocus)
    {
      this.FastId = fastId;
      this.TagFocus = tagFocus;
    }
  }
}