using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The QR15 HF RFID Module with integrated antenna is an easy to use RFID module
  /// which can be integrated into your electronics without big effort
  /// </summary>
  public class QR15 : MetratecReaderAsciiHf
  {
    #region Constructor
    /// <summary>Creates a new QR15 instance</summary>
    /// <param name="serialPort">The device IP address</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public QR15(string serialPort, ILogger? logger = null, string? id = null) : base(new SerialInterface(serialPort), logger, id) { }

    /// <summary>The constructor of the QR15 object</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public QR15(string ipAddress, int tcpPort, ILogger? logger = null, string? id = null) : base(new EthernetInterface(ipAddress, tcpPort), logger, id) { }

    /// <summary>The constructor of the QR15 object</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public QR15(ICommunicationInterface connection, ILogger? logger = null, string? id = null) : base(connection, logger, id) { }

    #endregion

    #region Protected Methods
    /// <inheritdoc/>
    protected override void EnableInputEvents(bool enable = true)
    {
      if (FirmwareName?.ToLower().Contains("v2") == true)
      {
        Logger.LogInformation("Input events disabled, minimum QR15_V2 required.");
        return;
      }
      base.EnableInputEvents(enable);
    }
    #endregion
  }
}
