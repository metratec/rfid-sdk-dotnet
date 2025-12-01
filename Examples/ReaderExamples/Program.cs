using ReaderExamples;

namespace Examples
{
  /// <summary>
  /// Main program demonstrating usage of various Metratec RFID readers.
  /// This example project showcases different reader types and their capabilities:
  /// 
  /// Reader Types Supported:
  /// 
  /// HF Readers (13.56 MHz):
  /// - DeskID_ISO: HF reader for ISO15693 tags (serial connection)
  /// - DeskID_NFC: NFC reader for Mifare and other NFC tags (serial connection)
  /// - QuasarMX: Network HF reader for ISO15693 tags (Ethernet connection)
  /// - QuasarLR: Long-range HF reader for industrial applications (Ethernet/Serial)
  /// - DMI15: IoT HF reader with integrated antenna and PoE (Ethernet connection)
  /// - Dwarf15: SMD HF module for embedded systems (Serial connection)
  /// - QR15: HF module with integrated antenna (Serial/Ethernet)
  /// - RR15: HF module with integrated antenna, no GPIO (Serial/Ethernet)
  /// 
  /// UHF Readers (860-960 MHz):
  /// - DeskID_UHF: Legacy UHF reader for EPC Gen2 tags (ASCII protocol, serial)
  /// - DeskID_UHF_v2: UHF reader for EPC Gen2 tags (AT protocol, serial)
  /// - PulsarLR: Long-range UHF reader (Ethernet connection)
  /// - PulsarMX: Mid-range UHF reader (Ethernet connection)
  /// - DwarfG2: UHF module family (ASCII protocol, Serial/Ethernet)
  /// - DwarfG2_v2: UHF module with Impinj E310 frontend (AT protocol, 0-21 dBm)
  /// - DwarfG2_XR_v2: Extended range UHF module (AT protocol, 0-27 dBm, 5m range)
  /// - DwarfG2_Mini_v2: Compact UHF module (AT protocol, 0-9 dBm, 0.5m range)
  /// 
  /// To run examples:
  /// 1. Update connection strings (COM ports, IP addresses) in the example files
  /// 2. Uncomment the desired example method below
  /// 3. Ensure your reader is connected and powered on
  /// 4. Run the application
  /// </summary>
  class UsageExample
  {
    static void Main(string[] args)
    {
      // NFC Examples - Near Field Communication (13.56 MHz)
      // Demonstrates basic NFC inventory and advanced Mifare Classic operations
      // DeskidNFCExamples.InventoryExample();
      // DeskidNFCExamples.ReadWriteMifareData();  // Advanced Mifare authentication & data access
      
      // HF Examples - High Frequency ISO15693 (13.56 MHz)
      // Shows serial and network-based HF tag operations
      // DeskidIsoExamples.InventoryExample();     // Serial connection HF inventory
      // DeskidIsoExamples.ReadWriteExample();     // Serial connection HF read/write
      // QuasarMxExamples.InventoryExample();      // Network connection HF inventory
      // QuasarMxExamples.ReadWriteExample();      // Network connection HF read/write
      // QuasarLRExamples.LongRangeInventoryExample(); // Long-range industrial HF inventory
      // QuasarLRExamples.LongRangeReadWriteExample(); // Long-range HF read/write operations
      // DMI15Examples.InventoryExample();         // IoT HF reader with PoE
      // DMI15Examples.ReadWriteExample();         // IoT HF reader read/write operations
      // Dwarf15Examples.InventoryExample();       // SMD HF module for embedded systems
      // Dwarf15Examples.ReadWriteExample();       // SMD HF module read/write operations
      // QR15Examples.InventoryExample();          // HF module with integrated antenna
      // QR15Examples.ReadWriteExample();          // HF module read/write operations
      // RR15Examples.InventoryExample();          // HF module without GPIO functionality
      // RR15Examples.ReadWriteExample();          // HF module read/write operations
      
      // UHF Examples - Ultra High Frequency EPC Gen2 (860-960 MHz)
      // Demonstrates both serial and network UHF reader operations
      DeskidUhfExamples.InventoryExample();     // Serial connection UHF inventory (AT protocol)
      // DeskidUhfExamples.ReadWriteExample();     // Serial connection UHF read/write (AT protocol)
      // DeskidUhfLegacyExamples.InventoryExample(); // Legacy UHF reader (ASCII protocol)
      // DeskidUhfLegacyExamples.ReadWriteExample(); // Legacy UHF read/write operations
      // PulsarMxExamples.InventoryExample();      // Mid-range UHF inventory (Ethernet/Serial)
      // PulsarMxExamples.ReadWriteExample();      // Mid-range UHF read/write (Ethernet/Serial)
      // PulsarLrExamples.InventoryExample();      // Long-range UHF inventory (Ethernet/Serial)
      // PulsarLrExamples.MultiAntennaInventoryExample(); // Multi-antenna inventory with GetMultiInventory
      // PulsarLrExamples.ReadWriteExample();      // Long-range UHF read/write (Ethernet/Serial)
      // PulsarLrExamples.CustomImpinjExample();   // Advanced Impinj authentication features
      // DwarfG2Examples.DwarfG2v2InventoryExample(); // DwarfG2_v2 enhanced features (AT protocol)
      // DwarfG2Examples.DwarfG2XRInventoryExample(); // Extended range variant (0-27 dBm, 5m range)
      // DwarfG2Examples.DwarfG2MiniInventoryExample(); // Compact variant (0-9 dBm, 0.5m range)
      // DwarfG2Examples.ReadWriteExample();       // UHF read/write operations across variants
      // DwarfG2Examples.DwarfG2InventoryExample(); // Legacy DwarfG2 UHF operations (ASCII)
      
      // Notes:
      // - Each example includes comprehensive error handling and resource cleanup
      // - Update connection parameters (COM ports, IP addresses) before running
      // - Ensure proper tag types are used with each reader (HF tags with HF readers, etc.)
      // - Network readers require proper network configuration and connectivity
      // - Some advanced features may require specific tag types or firmware versions
    }
  }
}