using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The QR15 HF RFID Module with integrated antenna is an easy to use RFID module which can be integrated into your electronics without big effort
  /// </summary>
  public class QR15 : HfReaderGen1
  {
    #region Constructor
    /// <summary>Creates a new MetraTecDevices.QR15 instance</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    public QR15(string ipAddress, int tcpPort) : base(new EthernetInterface(ipAddress, tcpPort)) { }
    /// <summary>Creates a new MetraTecDevices.QR15 instance</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    public QR15(string portName) : base(new SerialInterface(115200, portName)) { }
    /// <summary>Creates a new MetraTecDevices.QR15 instance</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    /// <param name="logger">the logger</param>
    public QR15(string ipAddress, int tcpPort, ILogger logger) : base(new EthernetInterface(ipAddress, tcpPort), logger) { }
    /// <summary>Creates a new MetraTecDevices.QR15 instance</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="logger">the logger</param>
    public QR15(string portName, ILogger logger) : base(new SerialInterface(115200, portName), logger) { }
    #endregion
  }
}
