using System;

namespace MetraTecDevices
{
  /// <summary>
  /// The RFID Transponder base class
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
    public abstract string ID { get; set; }

    /// <summary>
    /// Transponder ID
    /// </summary>
    public string? TID { get; set; }

    /// <summary>
    /// First seen timestamp
    /// </summary>
    public DateTime FirstSeen { get; set; }

    /// <summary>
    /// Last seen timestamp
    /// </summary>
    public DateTime LastSeen { get; set; }

    /// <summary>
    /// Seen count
    /// </summary>
    public int SeenCount { get; set; }

    /// <summary>
    /// Antenna which detect this tag
    /// </summary>
    public int Antenna { get; set; }

    /// <summary>
    /// Tag data
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// Tag data
    /// </summary>
    public int? DataStartAddress { get; set; }

    /// <summary>
    /// True if the tag contains error information
    /// </summary>
    public bool HasError { get; set; }

    /// <summary>
    /// the error message 
    /// </summary>
    public string? Message { get; set; }

  }

  /// <summary>
  /// The hf transponder class
  /// </summary>
  public class HfTag : RfidTag
  {
    /// <summary>
    /// Default rfid hf tag constructor
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    public HfTag(string tid, DateTime firstSeen, int antennaPort) : this(tid, firstSeen, antennaPort, "HfTag")
    {
    }
    /// <summary>
    /// Default rfid hf tag constructor
    /// </summary>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    public HfTag(DateTime firstSeen, int antennaPort) : this("", firstSeen, antennaPort)
    {
    }

    /// <summary>
    /// Default rfid hf tag constructor
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    /// <param name="type"></param>
    public HfTag(string tid, DateTime firstSeen, int antennaPort, string type) : base(tid, firstSeen, antennaPort)
    {
      TID = tid ?? "";
      Type = type;
    }
    /// <summary>
    /// Default rfid hf tag constructor
    /// </summary>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    /// <param name="type"></param>
    public HfTag(DateTime firstSeen, int antennaPort, string type) : this("", firstSeen, antennaPort, type)
    {
    }

    /// <summary>
    /// Transponder TID
    /// </summary>
    public override string ID { get => TID; set => TID = value ?? ""; }
    /// <summary>
    /// Transponder ID
    /// </summary>
    public new string TID { get; set; }
    /// <summary>
    /// Tag Type if available
    /// </summary>
    /// <value>the tag type information</value>
    public string Type { get; set; }

  }

  /// <summary>
  /// The ISO15 Transponder class
  /// </summary>
  public class ISO15Tag : HfTag
  {
    /// <summary>
    /// Default rfid ISO15 hf tag constructor
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    public ISO15Tag(string tid, DateTime firstSeen, int antennaPort) : base(tid, firstSeen, antennaPort, "ISO15")
    {
    }
    /// <summary>
    /// Default rfid ISO15 hf tag constructor
    /// </summary>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    public ISO15Tag(DateTime firstSeen, int antennaPort) : base(firstSeen, antennaPort, "ISO15")
    {
    }
    /// <summary>
    /// The dsfid byte as hex
    /// </summary>
    /// <value>The dsfid byte as hex</value>
    public string? DSFID { get; set; }
  }

  /// <summary>
  /// The ISO14A Transponder class
  /// </summary>
  public class ISO14ATag : HfTag
  {
    /// <summary>
    /// Default rfid ISO14A hf tag constructor
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    public ISO14ATag(string tid, DateTime firstSeen, int antennaPort) : base(tid, firstSeen, antennaPort, "ISO14A")
    {
    }
    /// <summary>
    /// Default rfid ISO14A hf tag constructor
    /// </summary>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    public ISO14ATag(DateTime firstSeen, int antennaPort) : base(firstSeen, antennaPort, "ISO14A")
    {
    }
    /// <summary>
    /// Default rfid ISO14A hf tag constructor
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    /// <param name="type"></param>
    public ISO14ATag(string tid, DateTime firstSeen, int antennaPort, string type) : base(tid, firstSeen, antennaPort, type)
    {
    }
    /// <summary>
    /// Default rfid ISO14A hf tag constructor
    /// </summary>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    /// <param name="type"></param>
    public ISO14ATag(DateTime firstSeen, int antennaPort, string type) : base(firstSeen, antennaPort, type)
    {
    }
    /// <summary>
    /// The sak byte as hex
    /// </summary>
    /// <value>The sak byte as hex</value>
    public string? SAK { get; set; }
    /// <summary>
    /// The atqa bytes as hex
    /// </summary>
    /// <value>The atqa bytes as hex</value>
    public string? ATQA { get; set; }

  }
  /// <summary>
  /// The uhf transponder class
  /// </summary>
  public class UhfTag : RfidTag
  {
    /// <summary>
    /// Default rfid uhf tag constructor
    /// </summary>
    /// <param name="epc"></param>
    /// <param name="firstSeen"></param>
    /// <param name="antennaPort"></param>
    public UhfTag(string epc, DateTime firstSeen, int antennaPort) : base(epc, firstSeen, antennaPort)
    {
      EPC = epc;
    }
    /// <summary>
    /// Default rfid uhf tag constructor
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
    public override string ID { get => EPC; set => EPC = value ?? ""; }

    /// <summary>
    /// Transponder EPC
    /// </summary>
    public string EPC { get; internal set; }

    /// <summary>
    /// RSSI value
    /// </summary>
    public int? RSSI { get; internal set; }

  }

  /// <summary>
  /// The exception that is triggered when a transponder returns an error
  /// </summary>
  public class TransponderException : MetratecReaderException
  {
    /// <summary>
    /// Initializes a new instance of the TransponderException class.
    /// </summary>
    public TransponderException() : base() { }
    /// <summary>
    /// Initializes a new instance of the TransponderException class with a specified
    /// error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public TransponderException(string? message) : base(message) { }
    /// <summary>
    /// Initializes a new instance of the TransponderException class with a specified
    /// error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception. If the innerException
    /// parameter is not null, the current exception is raised in a catch block that
    /// handles the inner exception.</param>
    /// <returns></returns>
    public TransponderException(string? message, Exception? innerException) : base(message, innerException) { }
  }
}