using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The Dwarf15 SMD module is a RFID module which can be integrated into your electronics
  /// </summary>
  public class Dwarf15 : HfReaderAscii
  {
    #region Constructor
    /// <summary>The constructor of the Dwarf15 object</summary>
    /// <param name="serialPort">The device IP address</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public Dwarf15(string serialPort, ILogger logger = null!, string id = null!) : base(new SerialInterface(serialPort), logger, id) { }

    /// <summary>The constructor of the Dwarf15 object</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public Dwarf15(ICommunicationInterface connection, ILogger logger = null!, string id = null!) : base(connection, logger, id) { }
    #endregion

    #region Protected Methods
    /// <inheritdoc/>
    protected override void EnableInputEvents(bool enable = true)
    {
      if (FirmwareMajorVersion != 3 || FirmwareMinorVersion < 14)
      {
        Logger.LogInformation("Input events disabled, minimum firmware version 3.14 required.");
        return;
      }
      base.EnableInputEvents(enable);
    }
    #endregion
  }
}
