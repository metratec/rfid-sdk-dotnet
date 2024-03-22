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
    /// <summary>Creates a new MetraTecDevices.PulsarMX instance</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    public PulsarMX(string ipAddress, int tcpPort) : base(new EthernetInterface(ipAddress, tcpPort)) { }
    /// <summary>Creates a new MetraTecDevices.PulsarMX instance</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    public PulsarMX(string portName) : base(new SerialInterface(115200, portName)) { }
    /// <summary>Creates a new MetraTecDevices.PulsarMX instance</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    /// <param name="logger">the logger</param>
    public PulsarMX(string ipAddress, int tcpPort, ILogger logger) : base(new EthernetInterface(ipAddress, tcpPort), logger) { }
    /// <summary>Creates a new MetraTecDevices.PulsarMX instance</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="logger">the logger</param>
    public PulsarMX(string portName, ILogger logger) : base(new SerialInterface(115200, portName), logger) { }
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
  }
}
