using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The DeskID UHF is a compact and well-priced RFID reader/writer working at 868 MHz (UHF RFID, EU) or 902 – 928 MHz (FCC, USA).
  /// Its main use is to read and write data to EPC Gen 2 transponders directly from your PC or laptop. Thus, the device is a handy
  /// tool for all UHF applications for testing tags, writing an EPC, or just debugging your UHF gate.
  /// </summary>
  public class DeskID_UHF : UhfReaderGen1
  {
    #region Constructor
    /// <summary>The constructor of the DeskID_UHF object</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    public DeskID_UHF(string portName) : base(new SerialInterface(115200, portName)) { }
    /// <summary>The constructor of the DeskID_UHF object</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="logger">the logger</param>
    public DeskID_UHF(string portName, ILogger logger) : base(new SerialInterface(115200, portName), logger) { }
    #endregion

    #region Public Methods
    /// <summary>
    /// Set the reader power
    /// </summary>
    /// <param name="power">the reader power [-2, 17]</param>
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
