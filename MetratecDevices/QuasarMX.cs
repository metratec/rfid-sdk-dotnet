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
    /// <summary>Creates a new MetraTecDevices.QuasarMX instance</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    public QuasarMX(string ipAddress, int tcpPort) : base(new EthernetInterface(ipAddress, tcpPort)) { }
    /// <summary>Creates a new MetraTecDevices.QuasarMX instance</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    public QuasarMX(string portName) : base(new SerialInterface(115200, portName)) { }
    /// <summary>Creates a new MetraTecDevices.QuasarMX instance</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    /// <param name="logger">the logger</param>
    public QuasarMX(string ipAddress, int tcpPort, ILogger logger) : base(new EthernetInterface(ipAddress, tcpPort), logger) { }
    /// <summary>Creates a new MetraTecDevices.QuasarMX instance</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="logger">the logger</param>
    public QuasarMX(string portName, ILogger logger) : base(new SerialInterface(115200, portName), logger) { }
    #endregion

    #region Public Methods

    #endregion
  }
}
