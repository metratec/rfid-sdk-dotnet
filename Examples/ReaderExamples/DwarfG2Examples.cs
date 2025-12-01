using System;
using System.Collections.Generic;
using MetraTecDevices;

namespace ReaderExamples
{
  /// <summary>
  /// Examples demonstrating DwarfG2 family reader operations for Ultra High Frequency (UHF) RFID tags.
  /// This family includes DwarfG2 (ASCII protocol), DwarfG2_v2 (AT protocol), DwarfG2_XR_v2 (extended range), 
  /// and DwarfG2_Mini_v2 (compact version). All support EPC Gen2 tags with different power levels and ranges.
  /// </summary>
  internal class DwarfG2Examples
  {
    /// <summary>
    /// Demonstrates the newer DwarfG2_v2 reader with AT command protocol and enhanced features.
    /// Shows improved performance and Impinj E310 frontend capabilities with up to 21 dBm power.
    /// </summary>
    public static void DwarfG2v2InventoryExample()
    {
      // Create the DwarfG2_v2 reader instance using AT command protocol
      // Note: Update "/dev/ttyACM0" to match your actual device path (Linux/Mac) or "COM#" for Windows
      String port = "/dev/ttyACM0";
      DwarfG2_v2 reader = new DwarfG2_v2(port);

      // Subscribe to reader connection status changes
      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} Reader status changed to {e.Message} ({e.Status})");

      // Subscribe to inventory events with enhanced features
      reader.NewInventory += (s, e) =>
      {
        Console.WriteLine($"{e.Timestamp} New inventory event! {e.Tags.Count} UHF Tag(s) found");
        foreach (UhfTag tag in e.Tags)
        {
          Console.WriteLine(
            $"  EPC: {tag.EPC}" +
            (!string.IsNullOrEmpty(tag.TID) ? $" | TID: {tag.TID}" : "") +
            (tag.RSSI != null ? $" | RSSI: {tag.RSSI} dBm" : ""));
        }
      };

      try
      {
        Console.WriteLine($"Connecting to DwarfG2_v2 on port {port}...");
        reader.Connect(2000);
        Console.WriteLine("Connection established!");
        Console.WriteLine($"Reader Firmware: {reader.FirmwareVersion}");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- Check USB cable connection");
        Console.WriteLine($"- Verify COM port (currently set to {port})");
        Console.WriteLine("- Ensure DwarfG2_v2 driver is installed");
        Console.WriteLine("- Check if another application is using the COM port");
        Console.WriteLine("- Verify this is DwarfG2_v2 (not legacy DwarfG2)");
        return;
      }

      try
      {
        // Set reader transmission power (0-21 dBm for DwarfG2_v2)
        // Higher power range compared to legacy DwarfG2
        reader.SetPower(5);
        Console.WriteLine("Reader power set to 5 dBm");

        // Configure advanced inventory settings available with AT protocol
        InventorySettings invSettings = reader.GetInventorySettings();
        invSettings.WithRssi = true;        // Include signal strength
        invSettings.WithTid = true;         // Include Tag Identifier
        invSettings.OnlyNewTag = false;     // Report all tags
        reader.SetInventorySettings(invSettings);
        Console.WriteLine("Advanced inventory settings configured (RSSI: ON, TID: ON)");

        // Perform single inventory with enhanced capabilities
        Console.WriteLine("\nPerforming enhanced inventory scan...");
        List<UhfTag> tags = reader.GetSingleInventory();
        Console.WriteLine($"Current inventory: {tags.Count} UHF Tag(s) found");

        foreach (UhfTag tag in tags)
        {
          Console.WriteLine(
            $"  EPC: {tag.EPC}" +
            (!string.IsNullOrEmpty(tag.TID) ? $" | TID: {tag.TID}" : "") +
            (tag.RSSI != null ? $" | RSSI: {tag.RSSI} dBm" : ""));
        }

        // Demonstrate continuous scanning with enhanced performance
        Console.WriteLine("Starting continuous inventory scan...");
        reader.StartInventory();
        Console.WriteLine("Enhanced inventory scan started - Press any key to stop");
        Console.ReadKey();

        reader.StopInventory();
        Console.WriteLine("Enhanced inventory stopped");

        // Display DwarfG2_v2 capabilities
        Console.WriteLine("\n=== DwarfG2_v2 Enhanced Features ===");
        Console.WriteLine("✓ Impinj E310 frontend IC");
        Console.WriteLine("✓ AT command protocol");
        Console.WriteLine("✓ Power range: 0-21 dBm");
        Console.WriteLine("✓ Read range: up to 1.5 meters");
        Console.WriteLine("✓ EPC Gen2 v2 features");
        Console.WriteLine("✓ Proprietary Impinj features (FastID, TagFocus)");
        Console.WriteLine("✓ No measurable heat development");
        Console.WriteLine("✓ Worldwide frequency range support");
      }
      catch (MetratecReaderException ex)
      {
        Console.WriteLine($"Error during operation: {ex.Message}");
      }
      finally
      {
        if (reader.Connected)
        {
          reader.Disconnect();
          Console.WriteLine("Connection closed");
        }
      }
    }

    /// <summary>
    /// Demonstrates the DwarfG2_XR_v2 extended range variant with up to 27 dBm power and 5 meter range.
    /// Shows high-performance applications requiring longer read distances.
    /// </summary>
    public static void DwarfG2XRInventoryExample()
    {
      // Create the DwarfG2_XR_v2 extended range reader instance
      // Note: Update "/dev/ttyACM0" to match your actual device path (Linux/Mac) or "COM#" for Windows
      String port = "/dev/ttyACM0";
      DwarfG2_XR_v2 reader = new DwarfG2_XR_v2(port);

      // Subscribe to events
      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} Reader status changed to {e.Message} ({e.Status})");
      reader.NewInventory += (s, e) =>
      {
        Console.WriteLine($"{e.Timestamp} New inventory event! {e.Tags.Count} UHF Tag(s) found");
        foreach (UhfTag tag in e.Tags)
        {
          // Display EPC which is the primary identifier for UHF tags
          Console.WriteLine(
            $"  EPC: {tag.EPC}" +
            (!string.IsNullOrEmpty(tag.TID) ? $" | TID: {tag.TID}" : "") +
            (tag.RSSI != null ? $" | RSSI: {tag.RSSI} dBm" : ""));
        }
      };

      try
      {
        Console.WriteLine("Connecting to DwarfG2_XR_v2...");
        reader.Connect(2000);
        Console.WriteLine("Connection established!");
        Console.WriteLine($"Extended Range Reader - Firmware: {reader.FirmwareVersion}");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to extended range reader ({e.Message})");
        return;
      }

      try
      {
        // Set high power for extended range operations (0-27 dBm)
        reader.SetPower(20);
        Console.WriteLine("Extended range power set to 20 dBm");

        // Configure for long-range detection
        InventorySettings invSettings = reader.GetInventorySettings();
        invSettings.WithRssi = true;
        invSettings.WithTid = false;        // May reduce performance at long range
        invSettings.OnlyNewTag = true;      // Focus on new detections
        reader.SetInventorySettings(invSettings);
        Console.WriteLine("Extended range inventory settings configured");

        Console.WriteLine("\nPerforming extended range inventory scan...");
        List<UhfTag> tags = reader.GetSingleInventory();
        Console.WriteLine($"Extended range inventory: {tags.Count} UHF Tag(s) found");

        foreach (UhfTag tag in tags)
        {
          Console.WriteLine(
            $"  EPC: {tag.EPC}" +
            (!string.IsNullOrEmpty(tag.TID) ? $" | TID: {tag.TID}" : "") +
            (tag.RSSI != null ? $" | RSSI: {tag.RSSI} dBm" : ""));
        }
      }
      catch (MetratecReaderException ex)
      {
        Console.WriteLine($"Extended range operation error: {ex.Message}");
      }
      finally
      {
        if (reader.Connected)
        {
          reader.Disconnect();
          Console.WriteLine("Extended range connection closed");
        }
      }
    }

    /// <summary>
    /// Demonstrates the DwarfG2_Mini_v2 compact variant optimized for close-range applications.
    /// Shows low-power operations with up to 9 dBm and 50 cm range for desktop use.
    /// </summary>
    public static void DwarfG2MiniInventoryExample()
    {
      // Create the DwarfG2_Mini_v2 compact reader instance
      // Note: Update "/dev/ttyACM0" to match your actual device path (Linux/Mac) or "COM#" for Windows
      String port = "/dev/ttyACM0";
      DwarfG2_Mini_v2 reader = new DwarfG2_Mini_v2(port);

      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} Mini reader status: {e.Message} ({e.Status})");
      reader.NewInventory += (s, e) =>
      {
        Console.WriteLine($"{e.Timestamp} New inventory event! {e.Tags.Count} UHF Tag(s) found");
        foreach (UhfTag tag in e.Tags)
        {
          Console.WriteLine(
            $"  EPC: {tag.EPC}" +
            (!string.IsNullOrEmpty(tag.TID) ? $" | TID: {tag.TID}" : "") +
            (tag.RSSI != null ? $" | RSSI: {tag.RSSI} dBm" : ""));
        }
      };

      try
      {
        Console.WriteLine("Connecting to DwarfG2_Mini_v2)...");
        reader.Connect(2000);
        Console.WriteLine("Connection established!");
        Console.WriteLine($"Compact Reader - Firmware: {reader.FirmwareVersion}");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to compact reader ({e.Message})");
        return;
      }

      try
      {
        // Set low power for close-range operations (0-9 dBm)
        reader.SetPower(5);
        Console.WriteLine("Compact reader power set to 5 dBm (optimized for close range)");

        // Configure advanced inventory settings
        InventorySettings invSettings = reader.GetInventorySettings();
        invSettings.WithRssi = true;        // Include signal strength (RSSI) in responses
        invSettings.WithTid = true;         // Include Tag Identifier (TID) for additional tag info
        invSettings.OnlyNewTag = false;     // Report all tags, not just newly detected ones
        reader.SetInventorySettings(invSettings);

        Console.WriteLine("\nPerforming close-range inventory scan...");
        List<UhfTag> tags = reader.GetSingleInventory();
        Console.WriteLine($"Close-range inventory: {tags.Count} UHF Tag(s) found");

        foreach (UhfTag tag in tags)
        {
          Console.WriteLine(
            $"  EPC: {tag.EPC}" +
            (!string.IsNullOrEmpty(tag.TID) ? $" | TID: {tag.TID}" : "") +
            (tag.RSSI != null ? $" | RSSI: {tag.RSSI} dBm" : ""));
        }

     }
      catch (MetratecReaderException ex)
      {
        Console.WriteLine($"Compact reader operation error: {ex.Message}");
      }
      finally
      {
        if (reader.Connected)
        {
          reader.Disconnect();
          Console.WriteLine("Compact reader connection closed");
        }
      }
    }

    /// <summary>
    /// Demonstrates read/write operations across different DwarfG2 variants.
    /// Shows how to access tag memory with appropriate power levels for each variant.
    /// </summary>
    public static void ReadWriteExample()
    {
      // For this example, we'll use DwarfG2_v2 as it offers the best balance
      String port = "/dev/ttyUSB0";
      DwarfG2_v2 reader = new DwarfG2_v2(port);

      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} Reader status: {e.Message} ({e.Status})");

      try
      {
        Console.WriteLine("Connecting DwarfG2_v2 for read/write operations...");
        reader.Connect(2000);
        Console.WriteLine("Connection established for data operations!");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect for read/write operations ({e.Message})");
        return;
      }

      try
      {
        // Set optimal power for read/write operations
        reader.SetPower(15);
        Console.WriteLine("Power optimized for read/write operations (15 dBm)");

        // Wait for a tag
        Console.WriteLine("Place a UHF tag near the DwarfG2_v2 reader...");
        List<UhfTag> tags;
        int attempts = 0;
        do
        {
          tags = reader.GetSingleInventory();
          if (tags.Count == 0)
          {
            attempts++;
            if (attempts % 5 == 0)
            {
              Console.WriteLine($"Searching for tags... ({attempts} attempts)");
            }
            System.Threading.Thread.Sleep(1000);
          }
        } while (tags.Count == 0 && attempts < 30);

        if (tags.Count == 0)
        {
          Console.WriteLine("No tags found. Please place a UHF tag closer to the reader.");
          return;
        }

        UhfTag tag = tags[0];
        Console.WriteLine($"Tag found for read/write: {tag.EPC}");

        // Read user memory
        Console.WriteLine("\nReading user data from address 0...");
        try
        {
          List<UhfTag> resp = reader.ReadTagUsrData(0, 2);
          if (resp.Count > 0 && !resp[0].HasError)
          {
            Console.WriteLine($"Read data: {resp[0].Data}");
          }
          else if (resp.Count > 0)
          {
            Console.WriteLine($"Read error: {resp[0].Message}");
          }
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"Read operation failed: {ex.Message}");
        }

        // Write user data
        string dataToWrite = "01020304";
        Console.WriteLine($"\nWriting data '{dataToWrite}' to user memory...");
        try
        {
          List<UhfTag> resp = reader.WriteTagUsrData(0, dataToWrite);
          if (resp.Count > 0 && !resp[0].HasError)
          {
            Console.WriteLine("Write operation successful!");

            // Verify
            List<UhfTag> verifyResp = reader.ReadTagUsrData(0, 2);
            if (verifyResp.Count > 0 && !verifyResp[0].HasError)
            {
              Console.WriteLine($"Verification: {verifyResp[0].Data}");
            }
          }
          else if (resp.Count > 0)
          {
            Console.WriteLine($"Write error: {resp[0].Message}");
          }
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"Write operation failed: {ex.Message}");
        }

        Console.WriteLine("\n=== DwarfG2 Family Summary ===");
        Console.WriteLine("DwarfG2 (Legacy):      ASCII protocol, basic features");
        Console.WriteLine("DwarfG2_v2:           AT commands, 0-21 dBm, 1.5m range");
        Console.WriteLine("DwarfG2_XR_v2:        AT commands, 0-27 dBm, 5m range");
        Console.WriteLine("DwarfG2_Mini_v2:      AT commands, 0-9 dBm, 0.5m range");
        Console.WriteLine("\nAll v2 variants feature Impinj E310 frontend IC");
      }
      catch (MetratecReaderException ex)
      {
        Console.WriteLine($"Read/write operation error: {ex.Message}");
      }
      finally
      {
        if (reader.Connected)
        {
          reader.Disconnect();
          Console.WriteLine("Read/write session closed");
        }
      }
    }

    /// <summary>
    /// Demonstrates basic UHF inventory operations using the legacy DwarfG2 reader with ASCII protocol.
    /// Shows how to detect EPC Gen2 tags with both Ethernet and serial connectivity options.
    /// </summary>
    public static void DwarfG2InventoryExample()
    {
      // Create the DwarfG2 reader instance - supports both Ethernet and Serial
      // Option 1: Ethernet connection (for network-enabled variants)
      // DwarfG2 reader = new DwarfG2("192.168.1.100", 10001, REGION.ETS);

      // Option 2: Serial connection (for USB/RS232 variants)
      // Note: Update "/dev/ttyUSB0" to match your actual device path (Linux/Mac) or "COM#" for Windows
      String port = "/dev/ttyUSB0";
      DwarfG2 reader = new DwarfG2(port, REGION.ETS); // ETSI for Europe

      // Subscribe to reader connection status changes (Connected/Disconnected)
      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} Reader status changed to {e.Message} ({e.Status})");

      // Subscribe to inventory events - triggered when tags are detected during continuous scanning
      reader.NewInventory += (s, e) =>
      {
        Console.WriteLine($"{e.Timestamp} New inventory event! {e.Tags.Count} UHF Tag(s) found");
        foreach (UhfTag tag in e.Tags)
        {
          // Display comprehensive UHF tag information
          Console.WriteLine(
            $"  EPC: {tag.EPC}" +
            (!string.IsNullOrEmpty(tag.TID) ? $" | TID: {tag.TID}" : "") +
            (tag.RSSI != null ? $" | RSSI: {tag.RSSI} dBm" : ""));
        }
      };

      // Establish connection to the reader with 2-second timeout
      try
      {
        Console.WriteLine("Connecting to DwarfG2 (ASCII Protocol)...");
        reader.Connect(2000);
        Console.WriteLine("Connection established!");
        Console.WriteLine($"Reader Firmware: {reader.FirmwareVersion}");
        Console.WriteLine($"Protocol: ASCII (Legacy)");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- For Serial: Check USB cable connection and COM port");
        Console.WriteLine("- For Ethernet: Check network cable and IP address");
        Console.WriteLine("- Verify DwarfG2 driver installation");
        Console.WriteLine("- Check if another application is using the device");
        Console.WriteLine("- Verify reader is powered on");
        Console.WriteLine("- Note: This is legacy DwarfG2, consider DwarfG2_v2 for new projects");
        return;
      }

      try
      {
        // Set reader transmission power (specific range depends on DwarfG2 variant)
        // Legacy DwarfG2 typically supports lower power ranges
        reader.SetPower(2);
        Console.WriteLine("Reader power set to 10 dBm");

        // Perform a single inventory scan to detect currently present tags
        Console.WriteLine("\nPerforming single inventory scan...");
        List<UhfTag> tags = reader.GetInventory();
        Console.WriteLine($"Current inventory: {tags.Count} UHF Tag(s) found");

        foreach (UhfTag tag in tags)
        {
          Console.WriteLine(
            $"  EPC: {tag.EPC}" +
            (!string.IsNullOrEmpty(tag.TID) ? $" | TID: {tag.TID}" : "") +
            (tag.RSSI != null ? $" | RSSI: {tag.RSSI} dBm" : ""));
        }

        // Start continuous inventory scanning in the background
        Console.WriteLine("Starting continuous inventory scan...");
        reader.StartInventory();
        Console.WriteLine("Continuous inventory scan started - Press any key to stop");
        Console.ReadKey();

        // Stop the continuous scanning
        reader.StopInventory();
        Console.WriteLine("Continuous inventory stopped");
      }
      catch (MetratecReaderException ex)
      {
        Console.WriteLine($"Error during operation: {ex.Message}");
      }
      finally
      {
        // Always disconnect to free resources
        if (reader.Connected)
        {
          reader.Disconnect();
          Console.WriteLine("Connection closed");
        }
      }
    }
  }
}