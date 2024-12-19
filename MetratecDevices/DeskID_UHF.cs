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
    /// <param name="mode">The rfid standard to use. Defaults to ETSI</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DeskID_UHF(string portName, REGION mode = REGION.ETS, ILogger logger = null!, string id = null!) : base(new SerialInterface(portName), mode, logger, id) { }
    /// <summary>The constructor of the DeskID_UHF object</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="mode">The rfid standard to use. Defaults to ETSI</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DeskID_UHF(ICommunicationInterface connection, REGION mode = REGION.ETS, ILogger logger = null!, string id = null!) : base(connection, mode, logger, id) { }

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
    /// <summary>
    /// Not available for the DeskID_UHF reader. This method will throws a MetratecReaderException
    /// </summary>
    /// <param name="pin"></param>
    /// <returns></returns>
    /// <exception cref="MetratecReaderException"></exception>
    public override bool GetInput(int pin)
    {
      throw new MetratecReaderException($"The DeskID_UHF has no inputs");
    }
    /// <summary>
    /// Not available for the DeskID_UHF reader. This method will throws a MetratecReaderException
    /// </summary>
    /// <param name="pin"></param>
    /// <param name="value"></param>
    /// <exception cref="MetratecReaderException"></exception>
    public override void SetOutput(int pin, bool value)
    {
      throw new MetratecReaderException($"The DeskID_UHF has no outputs");
    }
    #endregion

    #region Protected Methods
    /// <summary>
    /// Input events not available
    /// </summary>
    /// <param name="enable"></param>
    protected override void EnableInputEvents(bool enable = true) { }
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
    /// <summary>The constructor of the DeskID_UHF_v2 object</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DeskID_UHF_v2(string portName, ILogger logger = null!, string id = null!) : base(new SerialInterface(portName), logger, id)
    {
      CurrentAntennaPort = 1;
      SingleAntennaInUse = true;
    }

    /// <summary>The constructor of the DeskID_UHF_v2 object</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DeskID_UHF_v2(ICommunicationInterface connection, ILogger logger = null!, string id = null!) : base(connection, logger, id)
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
    /// <summary>
    /// Not available for the DeskID_UHF_v2 reader. This method will throws a MetratecReaderException
    /// </summary>
    /// <param name="pin"></param>
    /// <returns></returns>
    /// <exception cref="MetratecReaderException"></exception>
    public override bool GetInput(int pin)
    {
      throw new MetratecReaderException($"The DeskID_UHF_v2 has no inputs");
    }
    /// <summary>
    /// Not available for the DeskID_UHF_v2 reader. This method will throws a MetratecReaderException
    /// </summary>
    /// <param name="pin"></param>
    /// <param name="value"></param>
    /// <exception cref="MetratecReaderException"></exception>
    public override void SetOutput(int pin, bool value)
    {
      throw new MetratecReaderException($"The DeskID_UHF_v2 has no outputs");
    }
    #endregion

  }
}
