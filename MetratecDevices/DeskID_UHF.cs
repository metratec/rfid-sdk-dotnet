using Microsoft.Extensions.Logging;
using CommunicationInterfaces;
using System.Collections.Generic;

namespace MetraTecDevices
{
  /// <summary>
  /// The DeskID UHF is a compact and well-priced RFID reader/writer working at 868 MHz (UHF RFID, EU) or 902 – 928 MHz (FCC, USA).
  /// Its main use is to read and write data to EPC Gen 2 transponders directly from your PC or laptop. Thus, the device is a handy
  /// tool for all UHF applications for testing tags, writing an EPC, or just debugging your UHF gate.
  /// </summary>
  public class DeskID_UHF : UhfReaderAscii
  {
    #region Constructor
    /// <summary>The constructor of the DeskID_UHF object</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    public DeskID_UHF(string portName) : base(new SerialInterface(115200, portName)) { }
    /// <summary>The constructor of the DeskID_UHF object</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="logger">the logger</param>
    public DeskID_UHF(string portName, ILogger logger) : base(new SerialInterface(115200, portName), logger) { }
    #endregion

    #region Public Methods
    /// <summary>
    /// Set the reader power
    /// </summary>
    /// <param name="power">the reader power [-2, 17]</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public override void SetPower(int power)
    {
      base.SetPower(power);
    }
    #endregion
  }

  /// <summary>
  /// The DeskID UHF is a compact and well-priced RFID reader/writer working at 868 MHz (UHF RFID, EU) or 902 – 928 MHz (FCC, USA).
  /// Its main use is to read and write data to EPC Gen 2 transponders directly from your PC or laptop. Thus, the device is a handy
  /// tool for all UHF applications for testing tags, writing an EPC, or just debugging your UHF gate.
  /// </summary>
  public class DeskID_UHF_v2 : UhfReaderAT
  {
    #region Constructor
    /// <summary>The constructor of the DeskID_UHF object</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    public DeskID_UHF_v2(string portName) : this(portName, null!) { }
    /// <summary>The constructor of the DeskID_UHF object</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="logger">the logger</param>
    public DeskID_UHF_v2(string portName, ILogger logger) : base(new SerialInterface(115200, portName), logger)
    {
      CurrentAntennaPort = 1;
      SingleAntennaInUse = true;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Set the reader power
    /// </summary>
    /// <param name="power">the reader power [-2, 17]</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public override void SetPower(int power)
    {
      base.SetPower(power);
    }
    #endregion
  }
}
