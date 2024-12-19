using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using CommunicationInterfaces;

namespace MetraTecDevices
{
  /// <summary>
  /// The right tool for the hardest UHF RFID applications. With the new Impinj E710 at the heart of the reader,
  /// this product can reach a reading distance of up to 12m with a modern UHF RFID transponder and easily scan a few hundred tags per second.
  /// 
  /// he four antenna ports give you the flexibility to build complex RFID devices, such as RFID gates and tunnels. 
  /// The number of antennas can further be extended using our multiplexers to up to 64 read points if you want to
  /// build an RFID smart shelve or a similar application.
  /// </summary>
  public class PulsarLR : UhfReaderATIO
  {
    

    #region Constructor
    /// <summary>Creates a new PulsarLR object</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public PulsarLR(string ipAddress, int tcpPort, ILogger logger = null!, string id = null!) : base(new EthernetInterface(ipAddress, tcpPort), logger, id) { }

    /// <summary>The constructor of the PulsarLR object</summary>
    /// <param name="portName">The device hardware information structure needed to connect to the device</param>
    /// <param name="logger">the logger</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public PulsarLR(string portName, ILogger logger = null!, string id = null!) : base(new SerialInterface(portName), logger, id) { }

    /// <summary>The constructor of the PulsarLR object</summary>
    /// <param name="connection">The connection interface</param>
    /// <param name="logger">The connection interface</param>
    /// <param name="id">The reader id. This is purely for identification within the software and can be anything.</param>
    public PulsarLR(ICommunicationInterface connection, ILogger logger = null!, string id = null!) : base(connection, logger, id) { }

    #endregion

  }


}