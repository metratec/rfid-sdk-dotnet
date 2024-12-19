using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The QuasarLR is an HF long-range RFID reader/writer for demanding industrial applications,
  /// where high reading reliability, high read ranges, and extensive special tag features are needed.
  /// </summary>
  public class QuasarLR : HfReaderAscii
  {
    #region Internal Variables
    internal int _minPower = 500;
    internal int _maxPower = 8000;
    #endregion

    #region Constructor
    /// <summary>Creates a new QuasarLR instance</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public QuasarLR(string ipAddress, int tcpPort, ILogger logger = null!, string id = null!) : base(new EthernetInterface(ipAddress, tcpPort), logger, id) { }

    /// <summary>Creates a new QuasarLR instance</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public QuasarLR(string portName, ILogger logger = null!, string id = null!) : base(new SerialInterface(portName), logger, id) { }

    /// <summary>Creates a new QuasarLR instance</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public QuasarLR(ICommunicationInterface connection, ILogger logger = null!, string id = null!) : base(connection, logger, id) { }
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
