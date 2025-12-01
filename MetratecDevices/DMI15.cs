using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The DMI15 is an HF RFID reader designed specifically for the requirements of the Internet of Things.
  /// Based on the ISO 15693 standard, the reader is designed to be integrated and controlled in industrial
  /// or logistic environments without much effort. The DMI15 Reader features easy installation,
  /// as the reader only needs to be connected via Power over Ethernet (PoE). An external antenna is not necessary,
  /// as it is already built into the device.
  /// </summary>
  public class DMI15 : MetratecReaderAsciiHf
  {
    #region Constructor
    /// <summary>The constructor of the DMI15 object</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DMI15(string ipAddress, int tcpPort, ILogger? logger = null, string? id = null) : base(new EthernetInterface(ipAddress, tcpPort), logger, id) { }

    /// <summary>The constructor of the DMI15 object</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DMI15(ICommunicationInterface connection, ILogger? logger = null, string? id = null) : base(connection, logger, id) { }
    #endregion
  }
}
