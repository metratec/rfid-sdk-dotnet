using System;

namespace MetraTecDevices
{
  /// <summary>
  /// Object presentation of the real-world Rfid tag in data space
  /// </summary>
  public abstract class RfidTag
  {
    /// <summary>
    /// Default rfid tag constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    public RfidTag(string id, DateTime firstSeen, int antennaPort)
    {
      ID = id;
      FirstSeen = firstSeen;
      Antenna = antennaPort;
      SeenCount = 1;
    }
    /// <summary>
    /// Default rfid tag constructor
    /// </summary>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    public RfidTag(DateTime firstSeen, int antennaPort) : this(null!, firstSeen, antennaPort)
    {
    }
    /// <summary>
    /// Transponder ID
    /// </summary>
    public abstract string ID { get; internal set; }

    /// <summary>
    /// Transponder ID
    /// </summary>
    public string? TID { get; internal set; }

    /// <summary>
    /// First seen timestamp
    /// </summary>
    public DateTime FirstSeen { get; internal set; }

    /// <summary>
    /// Last seen timestamp
    /// </summary>
    public DateTime LastSeen { get; internal set; }

    /// <summary>
    /// Seen count
    /// </summary>
    public int SeenCount { get; internal set; }

    /// <summary>
    /// Antenna which detect this tag
    /// </summary>
    public int Antenna { get; internal set; }

    /// <summary>
    /// Tag data
    /// </summary>
    public string? Data { get; internal set; }

    /// <summary>
    /// Tag data
    /// </summary>
    public int? DataStartAddress { get; internal set; }

    /// <summary>
    /// True if the tag contains error information
    /// </summary>
    public bool HasError { get; internal set; }

    /// <summary>
    /// the error message 
    /// </summary>
    public string? Message { get; internal set; }

  }

  /// <summary>
  /// Hf transponder
  /// </summary>
  public class HfTag : RfidTag
  {
    /// <summary>
    /// Default rfid hf tag constructor
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    public HfTag(string tid, DateTime firstSeen, int antennaPort) : base(tid, firstSeen, antennaPort)
    {
      TID = tid ?? "";
    }
    /// <summary>
    /// Default rfid hf tag constructor
    /// </summary>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    public HfTag(DateTime firstSeen, int antennaPort) : base(firstSeen, antennaPort)
    {
      TID = "";
    }

    /// <summary>
    /// Transponder TID
    /// </summary>
    public override string ID { get => TID; internal set => TID = value ?? ""; }
    /// <summary>
    /// Transponder ID
    /// </summary>
    public new string TID { get; internal set; }

  }

  /// <summary>
  /// Hf transponder
  /// </summary>
  public class UhfTag : RfidTag
  {
    /// <summary>
    /// Default rfid hf tag constructor
    /// </summary>
    /// <param name="epc"></param>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    public UhfTag(string epc, DateTime firstSeen, int antennaPort) : base(epc, firstSeen, antennaPort)
    {
      EPC = epc;
    }
    /// <summary>
    /// Default rfid hf tag constructor
    /// </summary>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    public UhfTag(DateTime firstSeen, int antennaPort) : base(firstSeen, antennaPort)
    {
      EPC = "";
    }

    /// <summary>
    /// Transponder EPC
    /// </summary>
    public override string ID { get => EPC; internal set => EPC = value ?? ""; }

    /// <summary>
    /// Transponder EPC
    /// </summary>
    public string EPC { get; internal set; }

    /// <summary>
    /// RSSI value
    /// </summary>
    public int? RSSI { get; internal set; }

  }
}