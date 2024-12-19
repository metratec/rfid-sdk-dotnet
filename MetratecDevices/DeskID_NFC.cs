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
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DeskID_NFC(string portName, ILogger logger = null!, string id = null!) : base(new SerialInterface(portName), logger, id) { }

    /// <summary>The constructor of the DeskID_NFC object</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public DeskID_NFC(ICommunicationInterface connection, ILogger logger = null!, string id = null!) : base(connection, logger, id) { }
    #endregion

    #region Public Methods
    /// <summary>
    /// Not available for the DeskID_NFC reader. This method will throws a MetratecReaderException
    /// </summary>
    /// <param name="pin"></param>
    /// <returns></returns>
    /// <exception cref="MetratecReaderException"></exception>
    public override bool GetInput(int pin)
    {
      throw new MetratecReaderException($"The DeskID_NFC has no inputs");
    }
    /// <summary>
    /// Not available for the DeskID_NFC reader. This method will throws a MetratecReaderException
    /// </summary>
    /// <param name="pin"></param>
    /// <param name="value"></param>
    /// <exception cref="MetratecReaderException"></exception>
    public override void SetOutput(int pin, bool value)
    {
      throw new MetratecReaderException($"The DeskID_NFC has no outputs");
    }
    #endregion
  }
}
