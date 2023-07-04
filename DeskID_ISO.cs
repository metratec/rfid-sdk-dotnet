using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The DeskID ISO is a compact HF RFID Reader/Writer for RFID applications in the office or factory.
  /// Typical applications include customer management (e.g. in sports studios), the configuration of transponders
  /// in automation systems, and all other applications in which ISO15693 RFID tags need to be read with a PC or notebook computer. 
  /// </summary>
  public class DeskID_ISO : HfReaderGen1
  {
    #region Constructor
    /// <summary>The constructor of the DeskID_ISO object</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    public DeskID_ISO(string portName) : base(new SerialInterface(115200, portName)) { }
    /// <summary>The constructor of the DeskID_ISO object</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="logger">the logger</param>
    public DeskID_ISO(string portName, ILogger logger) : base(new SerialInterface(115200, portName), logger) { }
    #endregion
  }
}
