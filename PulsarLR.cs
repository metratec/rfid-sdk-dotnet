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
  public class PulsarLR : UhfReaderGen2
  {
    #region Internal Variables
    private List<int> currentAntennaPowers = new();
    private List<int> currentConnectedMultiplexer = new();
    #endregion

    #region Constructor
    /// <summary>Creates a new MetraTecDevices.PulsarLR instance</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    public PulsarLR(string ipAddress, int tcpPort) : base(new EthernetInterface(ipAddress, tcpPort)) { }
    /// <summary>Creates a new MetraTecDevices.PulsarLR instance</summary>
    /// <param name="ipAddress">The device IP address</param>
    /// <param name="tcpPort">The device TCP port used</param>
    /// <param name="logger">the logger</param>
    public PulsarLR(string ipAddress, int tcpPort, ILogger logger) : base(new EthernetInterface(ipAddress, tcpPort), logger) { }

    #endregion

    #region Methods
    /// <summary>
    /// Configure the reader.
    /// The base implementation must be called after success.
    /// </summary>
    protected override void ConfigureReader()
    {
      base.ConfigureReader();
      GetCurrentAntennaPowers();
      GetCurrentConnectedMultiplexer();
    }

    /// <summary>
    /// Gets the number of antennas to be multiplexed
    /// </summary>
    /// <returns>the number of antennas to be multiplexed</returns>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    public override int GetAntennaMultiplex()
    {
      String[] split = SplitLine(GetCommand("AT+MUX?")[6..]);
      if (split.Length > 1)
      {
        throw new InvalidOperationException("A multiplex sequence is activated, please use 'getAntennaMultiplexSequence' command");
      }
      return int.Parse(split[0]);
    }
    /// <summary>
    /// Gets the multiplex antenna sequence
    /// </summary>
    /// <returns>List with the antenna sequence</returns>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    public List<int> GetAntennaMultiplexSequence()
    {
      String[] split = SplitLine(GetCommand("AT+MUX?")[6..]);
      if (split.Length == 1)
      {
        throw new InvalidOperationException("No multiplex sequence activated, please use 'GetAntennaMultiplex' command");
      }
      List<int> sequence = new();
      foreach (String value in split)
      {
        sequence.Add(int.Parse(value));
      }
      return sequence;
    }
    /// <summary>
    /// Sets the multiplex antenna sequence
    /// </summary>
    /// <param name="sequence">the antenna sequence</param>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    public void SetAntennaMultiplexSequence(List<int> sequence)
    {
      SetCommand("AT+MUX=" + string.Join(",", sequence.Select(s => $"{s}")));
    }
    /// <summary>
    /// the power value per antenna (index 0 == antenna 1)
    /// </summary>
    /// <returns>List with the power values</returns>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    protected List<int> GetCurrentAntennaPowers()
    {
      String[] split = SplitLine(GetCommand("AT+PWR?")[6..]);
      List<int> antennaPowers = split.Select(x => int.Parse(x)).ToList();
      this.currentAntennaPowers = antennaPowers;
      return new List<int>(antennaPowers);
    }
    /// <summary>
    /// set the power values for the antennas
    /// </summary>
    /// <param name="antennaPowers">list with the multiplexer size for each antenna</param>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    protected void SetCurrentAntennaPowers(List<int> antennaPowers)
    {
      SetCommand("AT+PWR=" + string.Join(",", antennaPowers.Select(s => $"{s}")));
      this.currentAntennaPowers = new List<int>(antennaPowers);
    }
    /// <summary>
    /// Gets the current antenna power
    /// </summary>
    /// <param name="antenna">the antenna</param>
    /// <returns>the current antenna power</returns>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    public int GetAntennaPower(int antenna)
    {
      if (antenna <= 0)
      {
        throw new InvalidOperationException($"Antenna {antenna} is not available");
      }
      List<int> antennaPowers = GetCurrentAntennaPowers();
      try
      {
        return antennaPowers[antenna - 1];
      }
      catch (IndexOutOfRangeException)
      {
        throw new InvalidOperationException($"Antenna {antenna} is not available");
      }
    }
    /// <summary>
    /// Sets the antenna power
    /// </summary>
    /// <param name="antenna">the antenna</param>
    /// <param name="power">the rfid power to set</param>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    public void SetAntennaPower(int antenna, int power)
    {
      if (antenna <= 0)
      {
        throw new InvalidOperationException($"Antenna {antenna} is not available");
      }
      try
      {
#pragma warning disable IDE0028
        List<int> antennaPowers = new(this.currentAntennaPowers);
#pragma warning restore IDE0028 
        antennaPowers[antenna - 1] = power;
        SetCurrentAntennaPowers(antennaPowers);
      }
      catch (IndexOutOfRangeException)
      {
        throw new InvalidOperationException($"Antenna {antenna} is not available");
      }
    }
    /// <summary>
    /// Gets the configured multiplexer size per antenna (index 0 == antenna 1)
    /// </summary>
    /// <returns>List with the configured multiplexer size</returns>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    protected List<int> GetCurrentConnectedMultiplexer()
    {
      String[] split = SplitLine(GetCommand("AT+EMX?")[6..]);
      List<int> multiplexer = split.Select(x => int.Parse(x)).ToList();
      this.currentConnectedMultiplexer = multiplexer;
      return new List<int>(multiplexer);
    }
    /// <summary>
    /// set the power values for the antennas
    /// </summary>
    /// <param name="connectedMultiplexer">list with the multiplexer size for each antenna</param>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    protected void SetCurrentConnectedMultiplexer(List<int> connectedMultiplexer)
    {
      SetCommand("AT+EMX=" + string.Join(",", connectedMultiplexer.Select(s => $"{s}")));
      this.currentConnectedMultiplexer = new List<int>(connectedMultiplexer);
      // update antenna power values
    }
    /// <summary>
    /// Get the connected multiplexer (connected antennas per antenna port)
    /// </summary>
    /// <param name="antennaPort">the antenna port to which the multiplexer is connected</param>
    /// <returns>the multiplexer size</returns>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    public int GetMultiplexer(int antennaPort)
    {
      if (1 > antennaPort || antennaPort > 4)
      {
        throw new InvalidOperationException($"Antenna {antennaPort} is not available");
      }
      List<int> multiplexer = GetCurrentConnectedMultiplexer();
      return multiplexer[antennaPort - 1];
    }
    /// <summary>
    /// Sets the connected multiplexer (connected antennas per antenna port)
    /// </summary>
    /// <param name="antennaPort">the antenna port to which the multiplexer is connected</param>
    /// <param name="multiplexer">the multiplexer size</param>
    /// <exception cref="T:System.InvalidOperationException">
    /// If the reader return an error
    /// </exception>
    public void SetMultiplexer(int antennaPort, int multiplexer)
    {
      if (1 > antennaPort || antennaPort > 4)
      {
        throw new InvalidOperationException($"Antenna {antennaPort} is not available");
      }
#pragma warning disable IDE0028
      List<int> connectedMultiplexer = new(this.currentConnectedMultiplexer);
#pragma warning restore IDE0028
      connectedMultiplexer[antennaPort - 1] = multiplexer;
      SetCurrentAntennaPowers(connectedMultiplexer);
      // update antennas power values
      GetCurrentAntennaPowers();
    }
    #endregion
  }
}