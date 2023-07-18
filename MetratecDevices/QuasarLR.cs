using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The QuasarLR is an HF long-range RFID reader/writer for demanding industrial applications,
  /// where high reading reliability, high read ranges, and extensive special tag features are needed.
  /// </summary>
  public class QuasarLR : HfReaderGen1
  {
    #region Internal Variables
    internal int _minPower = 500;
    internal int _maxPower = 8000;
    #endregion

    #region Constructor
    /// <summary>Creates a new MetraTecDevices.QuasarLR instance</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    public QuasarLR(string ipAddress, int tcpPort) : base(new EthernetInterface(ipAddress, tcpPort)) { }
    /// <summary>Creates a new MetraTecDevices.QuasarLR instance</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    public QuasarLR(string portName) : base(new SerialInterface(115200, portName)) { }
    /// <summary>Creates a new MetraTecDevices.QuasarLR instance</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    /// <param name="logger">the logger</param>
    public QuasarLR(string ipAddress, int tcpPort, ILogger logger) : base(new EthernetInterface(ipAddress, tcpPort), logger) { }
    /// <summary>Creates a new MetraTecDevices.QuasarLR instance</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="logger">the logger</param>
    public QuasarLR(string portName, ILogger logger) : base(new SerialInterface(115200, portName), logger) { }
    #endregion

    #region Public Methods
    /// <summary>
    /// Set the reader power
    /// </summary>
    /// <param name="power">the reader power (500 to 4000 in 250 steps)</param>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    /// <exception cref="T:System.TimeoutException">
    /// Thrown if the reader does not responding in time
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    /// If the reader is not connected or the connection is lost
    /// </exception>
    public override void SetPower(int power)
    {
      base.SetPower(power);
    }
    #endregion
  }
}
