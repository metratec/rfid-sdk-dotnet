using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The DeskID NFC is a multi-protocol RFID reader/writer device.
  /// It can read and write any 13.56 MHz RFID transponder. This includes ISO15693 tags (NFC Type 5) and 
  /// all ISO14443-based transponders including all products from the NXP Mifare® series. This includes 
  /// not only Mifare Classic and Ultralight® but also NTAG transponders as well as the very secure Mifare DESFire® tags.
  /// </summary>
  public class DeskID_NFC : NfcReader
  {
    #region Constructor
    /// <summary>The constructor of the DeskID_NFC object</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    public DeskID_NFC(string portName) : base(new SerialInterface(115200, portName)) { }
    /// <summary>The constructor of the DeskID_NFC object</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="logger">the logger</param>
    public DeskID_NFC(string portName, ILogger logger) : base(new SerialInterface(115200, portName), logger) { }
    #endregion
  }
}
