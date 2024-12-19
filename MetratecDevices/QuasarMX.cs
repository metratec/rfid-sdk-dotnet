using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The QuasarMX is an HF RFID reader/writer for demanding industrial applications,
  /// where high reading reliability, speed, and extensive special tag features are needed.
  /// Highlights include a reading rate of up to 100 tag IDs/sec and reading and writing data on tags
  /// without needing to address them individually. This allows applications directly at conveyor belts,
  /// in production machinery, and in electric control cabinets.
  /// </summary>
  public class QuasarMX : HfReaderAscii
  {
    #region Constructor
    /// <summary>Creates a new QuasarMX instance</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public QuasarMX(string ipAddress, int tcpPort, ILogger logger = null!, string id = null!) : base(new EthernetInterface(ipAddress, tcpPort), logger, id) { }

    /// <summary>Creates a new QuasarMX instance</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public QuasarMX(string portName, ILogger logger = null!, string id = null!) : base(new SerialInterface(portName), logger, id) { }

    /// <summary>Creates a new QuasarMX instance</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public QuasarMX(ICommunicationInterface connection, ILogger logger = null!, string id = null!) : base(connection, logger, id) { }

    #endregion

    #region Public Methods

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
