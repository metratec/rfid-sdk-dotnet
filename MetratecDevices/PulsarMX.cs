using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The PulsarMX is a UHF Mid Range Reader for applications with a medium read range between 1 and 3 m and
  /// up to 150 transponders simultaneously in the field. Its main applications are in container tracking,
  /// reading data from sensor tags (e.g. temperature sensors), and on a conveyor belt.
  /// </summary>
  public class PulsarMX : UhfReaderAscii
  {
    #region Constructor

    /// <summary>Creates a new PulsarMX instance</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    /// <param name="mode">The rfid standard to use. Defaults to ETSI</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public PulsarMX(string ipAddress, int tcpPort, REGION mode = REGION.ETS, ILogger logger = null!, string id = null!) : base(new EthernetInterface(ipAddress, tcpPort), mode, logger, id) { }

    /// <summary>Creates a new PulsarMX instance</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="mode">The rfid standard to use. Defaults to ETSI</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public PulsarMX(string portName, REGION mode = REGION.ETS, ILogger logger = null!, string id = null!) : base(new SerialInterface(portName), mode, logger, id) { }

    /// <summary>Creates a new PulsarMX instance</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="mode">The rfid standard to use. Defaults to ETSI</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public PulsarMX(ICommunicationInterface connection, REGION mode = REGION.ETS, ILogger logger = null!, string id = null!) : base(connection, mode, logger, id) { }

    #endregion

    #region Public Methods

    /// <summary>
    /// Set the reader power
    /// </summary>
    /// <param name="power">the reader power</param>
    /// <exception cref="MetratecReaderException">
    /// If the reader is not connected or an error occurs, further details in the exception message
    /// </exception>
    public override void SetPower(int power)
    {
      base.SetPower(power);
    }
    
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
}
