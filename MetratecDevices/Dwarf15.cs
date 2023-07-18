using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The Dwarf15 SMD module is a RFID module which can be integrated into your electronics
  /// </summary>
  public class Dwarf15 : HfReaderGen1
  {
    #region Constructor
    /// <summary>The constructor of the Dwarf15 object</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    public Dwarf15(string ipAddress, int tcpPort) : base(new EthernetInterface(ipAddress, tcpPort)) { }
    /// <summary>The constructor of the Dwarf15 object</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    public Dwarf15(string portName) : base(new SerialInterface(115200, portName)) { }
    /// <summary>The constructor of the Dwarf15 object</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    /// <param name="logger">the logger</param>
    public Dwarf15(string ipAddress, int tcpPort, ILogger logger) : base(new EthernetInterface(ipAddress, tcpPort), logger) { }
    /// <summary>The constructor of the Dwarf15 object</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="logger">the logger</param>
    public Dwarf15(string portName, ILogger logger) : base(new SerialInterface(115200, portName), logger) { }
    #endregion
  }
}
