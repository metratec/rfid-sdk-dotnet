using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The DeskID ISO is a compact HF RFID Reader/Writer for RFID applications in the office or factory.
  /// Typical applications include customer management (e.g. in sports studios), the configuration of transponders
  /// in automation systems, and all other applications in which ISO15693 RFID tags need to be read with a PC or notebook computer. 
  /// </summary>
  public class DeskID_ISO : HfReaderAscii
  {
    #region Constructor
    /// <summary>The constructor of the DeskID_ISO object</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DeskID_ISO(string portName, ILogger logger = null!, string id = null!) : base(new SerialInterface(portName), logger, id) { }

    /// <summary>The constructor of the DeskID_ISO object</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DeskID_ISO(ICommunicationInterface connection, ILogger logger = null!, string id = null!) : base(connection, logger, id) { }
    #endregion

    #region Public Methods

    /// <summary>
    /// Not available for the DeskID_ISO reader. This method will throws a MetratecReaderException
    /// </summary>
    /// <param name="pin"></param>
    /// <returns></returns>
    /// <exception cref="MetratecReaderException"></exception>
    public override bool GetInput(int pin)
    {
      throw new MetratecReaderException($"The DeskID_ISO has no inputs");
    }
    /// <summary>
    /// Not available for the DeskID_ISO reader. This method will throws a MetratecReaderException
    /// </summary>
    /// <param name="pin"></param>
    /// <param name="value"></param>
    /// <exception cref="MetratecReaderException"></exception>
    public override void SetOutput(int pin, bool value)
    {
      throw new MetratecReaderException($"The DeskID_ISO has no outputs");
    }

    #endregion

    #region Protected Methods
    /// <summary>
    /// Input events not available
    /// </summary>
    /// <param name="enable"></param>
    protected override void EnableInputEvents(bool enable = true) { }
    #endregion
  }
}
