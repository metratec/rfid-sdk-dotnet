using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The RR15 HF RFID Module with integrated antenna and is an easy to use RFID module
  /// which can be integrated into your electronics without big effort 
  /// </summary>
  public class RR15 : MetratecReaderAsciiHf
  {
    #region Constructor
    /// <summary>Creates a new RR15 instance</summary>
    /// <param name="serialPort">The device IP address</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public RR15(string serialPort, ILogger? logger = null, string? id = null) : base(new SerialInterface(serialPort), logger, id) { }

    /// <summary>The constructor of the RR15 object</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public RR15(string ipAddress, int tcpPort, ILogger? logger = null, string? id = null) : base(new EthernetInterface(ipAddress, tcpPort), logger, id) { }

    /// <summary>The constructor of the RR15 object</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public RR15(ICommunicationInterface connection, ILogger? logger = null, string? id = null) : base(connection, logger, id) { }

    #endregion

    #region Public Methods

    /// <summary>
    /// Not available for the RR15 reader. This method will throws a MetratecReaderException
    /// </summary>
    /// <param name="pin"></param>
    /// <returns></returns>
    /// <exception cref="MetratecReaderException"></exception>
    public override bool GetInput(int pin)
    {
      throw new MetratecReaderException($"The RR15 has no inputs");
    }
    /// <summary>
    /// Not available for the RR15 reader. This method will throws a MetratecReaderException
    /// </summary>
    /// <param name="pin"></param>
    /// <param name="value"></param>
    /// <exception cref="MetratecReaderException"></exception>
    public override void SetOutput(int pin, bool value)
    {
      throw new MetratecReaderException($"The RR15 has no outputs");
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
