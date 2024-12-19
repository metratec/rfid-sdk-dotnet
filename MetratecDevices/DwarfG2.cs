using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The PulsarMX is a UHF Mid Range Reader for applications with a medium read range between 1 and 3 m and
  /// up to 150 transponders simultaneously in the field. Its main applications are in container tracking,
  /// reading data from sensor tags (e.g. temperature sensors), and on a conveyor belt.
  /// </summary>
  public class DwarfG2 : UhfReaderAscii
  {
    #region Constructor

    /// <summary>Creates a new DwarfG2 instance</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    /// <param name="mode">The rfid standard to use. Defaults to ETSI</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DwarfG2(string ipAddress, int tcpPort, REGION mode = REGION.ETS, ILogger logger = null!, string id = null!) : base(new EthernetInterface(ipAddress, tcpPort), mode, logger, id) { }

    /// <summary>Creates a new DwarfG2 instance</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="mode">The rfid standard to use. Defaults to ETSI</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DwarfG2(string portName, REGION mode = REGION.ETS, ILogger logger = null!, string id = null!) : base(new SerialInterface(portName), mode, logger, id) { }

    /// <summary>Creates a new DwarfG2 instance</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="mode">The rfid standard to use. Defaults to ETSI</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DwarfG2(ICommunicationInterface connection, REGION mode = REGION.ETS, ILogger logger = null!, string id = null!) : base(connection, mode, logger, id) { }

    #endregion

    #region Protected Methods
    /// <inheritdoc/>
    protected override void EnableInputEvents(bool enable = true)
    {
      if (FirmwareMajorVersion != 3 || FirmwareMinorVersion < 14)
      {
        Logger.LogInformation("Input events disabled, minimum firmware version 3.14 required.");
        return;
      }
      base.EnableInputEvents(enable);
    }

    #endregion
  }

  /// <summary>
  /// Based on the Impinj E310 frontend IC, this module delivers great performance in a small package and without
  /// measurable heat development.
  /// 
  /// Supports all the latest EPC Gen2 v2 features as well as propriety Impinj
  /// tag features like FastID and TagFocus. Thanks to the wide operating frequency
  /// range, the same module can be used worldwide.
  /// 
  /// Read modern UHF transponders with up to 21 dBm and 1.5 m range.
  /// </summary>
  public class DwarfG2_v2 : UhfReaderATIO
  {

    #region Constructor
    /// <summary>Creates a new DwarfG2_v2 instance</summary>
    /// <param name="serialPort">The device IP address</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DwarfG2_v2(string serialPort, ILogger logger = null!, string id = null!) : base(new SerialInterface(serialPort), logger, id) { }

    /// <summary>Creates a new DwarfG2_v2 instance</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DwarfG2_v2(ICommunicationInterface connection, ILogger logger = null!, string id = null!) : base(connection, logger, id) { }

    #endregion

    #region Public Methods

    /// <summary>
    /// Set the antenna power of the reader for all antennas.
    /// </summary>
    /// <param name="power">Power value in dBm [0,21].</param>
    /// <exception cref="MetratecReaderException">
    /// If a reader error occurs, further details in the exception message
    /// </exception>
    public override void SetPower(int power)
    {
      base.SetPower(power);
    }
    
    #endregion
  }

  /// <summary>
  /// Based on the Impinj E310 frontend IC, this module delivers great performance in a small package and without
  /// measurable heat development.
  /// 
  /// Supports all the latest EPC Gen2 v2 features as well as propriety Impinj
  /// tag features like FastID and TagFocus. Thanks to the wide operating frequency
  /// range, the same module can be used worldwide.
  /// 
  /// Read modern UHF transponders with up to 21 dBm and 1.5 m range.
  /// </summary>
  [Obsolete("Use DwarfG2_v2 class instead", true)]
  public class DwarfG2V2 : DwarfG2_v2
  {

    #region Constructor
    /// <summary>Creates a new DwarfG2V2 instance</summary>
    /// <param name="serialPort">The device IP address</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DwarfG2V2(string serialPort, ILogger logger = null!, string id = null!) : base(new SerialInterface(serialPort), logger, id) { }

    /// <summary>Creates a new DwarfG2V2 instance</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DwarfG2V2(ICommunicationInterface connection, ILogger logger = null!, string id = null!) : base(connection, logger, id) { }

    #endregion
  }

  /// <summary>
  /// Based on the Impinj E310 frontend IC, this module delivers great performance in a small package and without
  /// measurable heat development.
  /// 
  /// Supports all the latest EPC Gen2 v2 features as well as propriety Impinj
  /// tag features like FastID and TagFocus. Thanks to the wide operating frequency
  /// range, the same module can be used worldwide.
  /// 
  /// Read modern UHF transponders with up to 27 dBm and 5 m range.
  /// </summary>
  public class DwarfG2_XR_v2: DwarfG2_v2
  {
    #region Constructor
    /// <summary>Creates a new DwarfG2_XR_v2 instance</summary>
    /// <param name="serialPort">The device IP address</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DwarfG2_XR_v2(string serialPort, ILogger logger = null!, string id = null!) : base(new SerialInterface(serialPort), logger, id) { }

    /// <summary>Creates a new DwarfG2_XR_v2 instance</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DwarfG2_XR_v2(ICommunicationInterface connection, ILogger logger = null!, string id = null!) : base(connection, logger, id) { }

    #endregion

    #region Public Methods

    /// <summary>
    /// Set the antenna power of the reader for all antennas.
    /// </summary>
    /// <param name="power">Power value in dBm [0,27].</param>
    /// <exception cref="MetratecReaderException">
    /// If a reader error occurs, further details in the exception message
    /// </exception>
    public override void SetPower(int power)
    {
      base.SetPower(power);
    }
    
    #endregion
  }

  /// <summary>
  /// Based on the Impinj E310 frontend IC, this module delivers great performance in a small package and without
  /// measurable heat development.
  /// 
  /// Supports all the latest EPC Gen2 v2 features as well as propriety Impinj
  /// tag features like FastID and TagFocus. Thanks to the wide operating frequency
  /// range, the same module can be used worldwide.
  /// 
  /// Read modern UHF transponders with up to 9 dBm and 50 cm range.
  /// </summary>
  public class DwarfG2_Mini_v2: DwarfG2_v2
  {
    #region Constructor
    /// <summary>Creates a new DwarfG2_Mini_v2 instance</summary>
    /// <param name="serialPort">The device IP address</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DwarfG2_Mini_v2(string serialPort, ILogger logger = null!, string id = null!) : base(new SerialInterface(serialPort), logger, id) { }

    /// <summary>Creates a new DwarfG2_Mini_v2 instance</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DwarfG2_Mini_v2(ICommunicationInterface connection, ILogger logger = null!, string id = null!) : base(connection, logger, id) { }

    #endregion

    #region Public Methods

    /// <summary>
    /// Set the antenna power of the reader for all antennas.
    /// </summary>
    /// <param name="power">Power value in dBm [0,9].</param>
    /// <exception cref="MetratecReaderException">
    /// If a reader error occurs, further details in the exception message
    /// </exception>
    public override void SetPower(int power)
    {
      base.SetPower(power);
    }
    
    #endregion
  }
}