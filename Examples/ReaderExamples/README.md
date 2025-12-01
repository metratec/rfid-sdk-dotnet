# MetraTec RFID Reader Examples

This directory contains comprehensive examples demonstrating how to use various MetraTec RFID readers with the MetraTecDevices .NET library. The examples cover both High Frequency (HF) and Ultra High Frequency (UHF) RFID technologies with different connectivity options and protocols.

## Reader Categories

### High Frequency (HF) Readers - 13.56 MHz
HF readers support ISO15693 and NFC standards, ideal for close-range applications.

### Ultra High Frequency (UHF) Readers - 860-960 MHz  
UHF readers support EPC Gen2 standard, ideal for longer range applications and logistics.

## Available Reader Examples

### HF Readers (13.56 MHz)

| Reader | Description | Connectivity | Examples |
|--------|-------------|--------------|----------|
| **DeskID_ISO** | Desktop HF reader for ISO15693 tags | Serial (USB) | Inventory, Read/Write, RF Interface Config |
| **DeskID_NFC** | Desktop NFC reader for Mifare tags | Serial (USB) | Inventory, Mifare Authentication, Data Access |
| **QuasarMX** | Network HF reader for industrial use | Ethernet | Inventory, Read/Write, Network Operations |
| **QuasarLR** | Long-range industrial HF reader | Ethernet/Serial | Long-Range Ops, Industrial Apps, Power Mgmt |
| **DMI15** | IoT HF reader with PoE | Ethernet (PoE) | Inventory, Read/Write, RF Configuration |
| **Dwarf15** | SMD HF module for embedded systems | Serial | Inventory, Read/Write, Integration Guide |
| **QR15** | HF module with integrated antenna | Serial/Ethernet | Inventory, Read/Write, Network Connectivity |
| **RR15** | HF module, no GPIO functionality | Serial/Ethernet | Inventory, Read/Write, Monitoring, Network Apps |

### UHF Readers (860-960 MHz)

| Reader | Description | Connectivity | Examples |
|--------|-------------|--------------|----------|
| **DeskID_UHF_v2** | Desktop UHF reader (AT protocol) | Serial (USB) | Inventory, Read/Write, Advanced Features |
| **DeskID_UHF** | Legacy desktop UHF reader (ASCII) | Serial (USB) | Inventory, Read/Write, Region Config |
| **PulsarLR** | Long-range UHF reader | Ethernet/Serial | Long-Range Ops, Impinj Features, Connectivity |
| **PulsarMX** | Mid-range UHF reader | Ethernet/Serial | Mid-Range Ops, GPIO Control, Connectivity |
| **DwarfG2** | Legacy UHF module (ASCII protocol) | Serial/Ethernet | Basic UHF Operations |
| **DwarfG2_v2** | UHF module with Impinj E310 frontend | Serial | Enhanced Features (0-21 dBm, 1.5m range) |
| **DwarfG2_XR_v2** | Extended range UHF module | Serial | Extended Range (0-27 dBm, 5m range) |
| **DwarfG2_Mini_v2** | Compact UHF module | Serial | Close Range (0-9 dBm, 0.5m range) |

## Getting Started

### Prerequisites

1. **.NET 8.0 SDK** or later
2. **MetraTecDevices NuGet package** (version 3.4.0-beta-2 or later)
3. **Appropriate RFID reader hardware**
4. **Correct drivers installed** for your reader
5. **RFID tags** compatible with your reader (HF tags for HF readers, UHF tags for UHF readers)

### Installation

1. Clone or download the MetraTecDevices library
2. Open the solution in Visual Studio
3. Restore NuGet packages
4. Update connection parameters in example files

### Basic Usage

1. **Update connection parameters** in the example files:
   - For **Serial**: Change `"COM8"` to your actual COM port
   - For **Ethernet**: Change IP address `"192.168.2.203"` and port `10001` to match your setup

2. **Choose your reader example** in `Program.cs`:
   ```csharp
   // Uncomment the example you want to run
   DeskidNFCExamples.InventoryExample();
   ```

3. **Build and run** the application:
   ```bash
   dotnet build
   dotnet run
   ```

## Example Categories

### Inventory Operations
Basic tag detection and scanning operations:
- Single inventory scans
- Continuous inventory monitoring  
- Event-driven tag detection
- Signal strength (RSSI) measurement

### Read/Write Operations
Tag memory access operations:
- User memory reading and writing
- Data verification after writes
- Multi-block operations
- Error handling and retry logic

### Advanced Configuration
Specialized features and settings:
- Power level optimization
- Regional compliance settings
- RF interface configuration
- GPIO control and monitoring

### Connectivity Examples
Different connection methods:
- Ethernet vs Serial comparison
- Network integration
- Mobile/portable applications
- Industrial automation integration

## Reader-Specific Features

### Desktop Readers (DeskID Series)
- **USB connectivity** with driver installation
- **Compact design** for desktop applications  
- **Integrated antennas** for ease of use
- **Different protocols**: ASCII (legacy) vs AT commands (newer)

### Network Readers (Pulsar/Quasar Series)
- **Ethernet connectivity** for industrial integration
- **Optional Serial connectivity** for development
- **GPIO inputs/outputs** for automation
- **Power over Ethernet (PoE)** support on some models
- **Extended range** capabilities

### Module Readers (Dwarf/QR/RR Series)
- **Compact form factor** for integration
- **Flexible connectivity** (Serial/Ethernet depending on model)
- **Embedded system integration**
- **Different power ranges** for various applications

## Application Scenarios

### Retail & Inventory
- **Point of Sale (POS)**: DeskID_NFC for payment cards
- **Inventory Management**: DeskID_UHF_v2 for product tags
- **Asset Tracking**: PulsarMX for store equipment

### Industrial Automation  
- **Conveyor Belt Systems**: PulsarLR for long-range detection
- **Container Tracking**: PulsarMX for mid-range coverage
- **Production Line**: QuasarLR for high-power HF applications
- **Warehouse Management**: DeskID_UHF_v2 for EPC tags

### Development & Testing
- **Prototyping**: Dwarf series for embedded integration
- **Research**: Desktop readers for controlled environments
- **Mobile Applications**: Serial connectivity options

### IoT & Network Integration
- **Remote Monitoring**: DMI15 with PoE
- **Distributed Systems**: Network readers with Ethernet
- **Cloud Integration**: Network protocols for data upload

## Example Structure

Each reader example typically includes:

### 1. Connection Setup
```csharp
// Ethernet connection
ReaderType reader = new ReaderType("192.168.1.100", 10001);

// Serial connection  
ReaderType reader = new ReaderType("COM8");
```

### 2. Event Subscription
```csharp
reader.StatusChanged += (s, e) => Console.WriteLine($"Status: {e.Message}");
reader.NewInventory += (s, e) => Console.WriteLine($"Tags found: {e.Tags.Count}");
```

### 3. Connection Management
```csharp
try 
{
    reader.Connect(2000);
    // Perform operations
}
finally 
{
    reader.Disconnect();
}
```

### 4. Error Handling
- Comprehensive troubleshooting guides
- Connection timeout handling
- Hardware-specific error messages
- Recovery procedures

## Connection Configuration

### Serial Connections
```csharp
// Standard COM port connection
ReaderType reader = new ReaderType("COM8");
```

**Troubleshooting Serial:**
- Verify COM port in Device Manager
- Check USB cable connection
- Ensure no other application uses the port
- Install correct drivers

### Ethernet Connections  
```csharp
// Network connection with IP and port
ReaderType reader = new ReaderType("192.168.1.100", 10001);
```

**Troubleshooting Ethernet:**
- Ping the reader IP address
- Check firewall settings (port 10001)
- Verify network cable connection
- Confirm IP address configuration

## Tag Compatibility

### HF Tags (13.56 MHz)
- **ISO15693** tags (most HF readers)
- **Mifare Classic** (DeskID_NFC)
- **NFC Type A/B** (DeskID_NFC)
- **Custom HF tags** with specific memory layouts

### UHF Tags (860-960 MHz)  
- **EPC Gen2** tags (all UHF readers)
- **Impinj tags** with special features (Pulsar series)
- **Custom UHF tags** with user memory

## ⚡ Performance Optimization

### Power Settings
- **HF Readers**: Usually fixed optimal power
- **UHF Readers**: Adjustable power levels
  - Desktop: -2 to 21 dBm
  - Industrial: Up to 27 dBm (DwarfG2_XR_v2)
  - Long-Range: 500-8000 mW (QuasarLR)

### Read Range Optimization
- **Close Range (< 10 cm)**: DeskID series, Dwarf_Mini
- **Medium Range (10-100 cm)**: Standard desktop readers
- **Long Range (> 1 m)**: Pulsar/Quasar series

### Inventory Settings
```csharp
InventorySettings settings = new InventorySettings();
settings.WithRssi = true;    // Include signal strength
settings.WithTid = true;     // Include Tag Identifier  
settings.OnlyNewTag = false; // Report all tags
reader.SetInventorySettings(settings);
```

## Troubleshooting Guide

### Common Issues

**Connection Failed**
- Check hardware connections (USB/Ethernet cables)
- Verify power supply to reader
- Confirm correct COM port or IP address
- Check if another application is using the reader

**No Tags Detected**
- Verify correct tag type (HF tags with HF readers, UHF with UHF)
- Check tag placement and orientation
- Adjust power settings if supported
- Try different tags to isolate issues

**Read/Write Errors**
- Check tag memory layout and permissions
- Verify sufficient power for write operations
- Ensure tag is stationary during operations
- Check for tag password protection

**Network Issues (Ethernet readers)**
- Ping reader IP address
- Check network cables and switches
- Verify firewall allows port 10001
- Confirm no IP address conflicts

### Performance Issues

**Slow Performance**
- Reduce inventory frequency
- Optimize tag placement
- Check for RF interference
- Use appropriate power settings

**Inconsistent Reads**
- Check antenna positioning
- Verify stable power supply
- Look for metal interference
- Adjust reader orientation

## Support & Resources

### Documentation
- **API Reference**: XML documentation in code
- **Hardware Manuals**: Check MetraTec website
- **Protocol Specifications**: AT commands vs ASCII protocols

### Development Support
- **Example Code**: This repository
- **Error Messages**: Detailed in exception handling
- **Best Practices**: Demonstrated in examples

### Hardware Support
- **Driver Installation**: Required for USB readers
- **Network Configuration**: For Ethernet readers
- **Power Requirements**: Check reader specifications

## Version Information

- **Library Version**: MetraTecDevices 3.4.0-beta-2
- **Target Framework**: .NET 8.0
- **Examples Updated**: 2024
- **Protocol Support**: AT Commands and ASCII protocols

## Quick Reference

### Most Common Use Cases

| Use Case | Recommended Reader | Example Method |
|----------|-------------------|----------------|
| **Desktop Development** | DeskID_UHF_v2 | `DeskidUhfExamples.InventoryExample()` |
| **NFC Payments** | DeskID_NFC | `DeskidNFCExamples.ReadWriteMifareData()` |
| **Industrial Automation** | PulsarMX | `PulsarMxExamples.ConnectivityComparisonExample()` |
| **Long-Range Tracking** | PulsarLR | `PulsarLrExamples.InventoryExample()` |
| **Embedded Integration** | Dwarf15 | `Dwarf15Examples.InventoryExample()` |
| **Network Applications** | DMI15 | `DMI15Examples.AdvancedConfigurationExample()` |

### Essential Example Methods

```csharp
// Basic inventory for any reader
ReaderExamples.InventoryExample();

// Read/write operations  
ReaderExamples.ReadWriteExample();

// Network vs Serial comparison (Pulsar series)
ReaderExamples.ConnectivityComparisonExample();

// Advanced configuration
ReaderExamples.AdvancedConfigurationExample();
```

---

**Start with the reader that matches your hardware, uncomment the appropriate example in Program.cs, update the connection parameters, and run the application!**

**For new projects, we recommend starting with DeskID_UHF_v2 for UHF applications or DeskID_NFC for NFC/HF applications, as these provide comprehensive desktop development platforms.**