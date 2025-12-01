using System;
using System.Collections.Generic;
using System.Linq;
using MetraTecDevices;

namespace ReaderExamples
{
  /// <summary>
  /// Examples demonstrating PulsarLR (Long Range) reader operations for Ultra High Frequency (UHF) RFID tags.
  /// This reader supports advanced features like GPIO control, custom Impinj operations, and extended range.
  /// Uses AT command protocol over both Ethernet and Serial communication.
  /// </summary>
  internal class PulsarLrExamples
  {
    /// <summary>
    /// Demonstrates basic UHF inventory operations with advanced settings for long-range applications.
    /// Shows both Ethernet and Serial connectivity options, GPIO monitoring, power control, and inventory configuration.
    /// </summary>
    public static void InventoryExample()
    {
      // Create the reader instance - PulsarLR supports both Ethernet and Serial communication

      // Option 1: Ethernet connection (recommended for permanent installations)
      PulsarLR reader = new PulsarLR("plr-000143.local", 10001);

      // Option 2: Serial connection (useful for direct connection or mobile applications)
      // PulsarLR reader = new PulsarLR("COM8");

      // Subscribe to reader connection status changes (Connected/Disconnected)
      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} Reader status changed to {e.Message} ({e.Status})");

      // Subscribe to inventory events - triggered when tags are detected during continuous scanning
      reader.NewInventory += (s, e) =>
      {
        Console.WriteLine($"{e.Timestamp} New inventory event! {e.Tags.Count} UHF Tag(s) found");
        foreach (UhfTag tag in e.Tags)
        {
          Console.WriteLine(
            $"  EPC: {tag.EPC}" +
            $" | Ant: {tag.Antenna}" +
            (!string.IsNullOrEmpty(tag.TID) ? $" | TID: {tag.TID}" : "") +
            (tag.RSSI != null ? $" | RSSI: {tag.RSSI} dBm" : ""));
        }
      };

      // Subscribe to GPIO input changes - useful for trigger inputs or sensor monitoring
      reader.InputChanged += (s, e) => Console.WriteLine($"Input Changed: {e.Pin} {e.IsHigh}");

      // Establish network connection to the reader with 2-second timeout
      try
      {
        Console.WriteLine("Connecting to PulsarLR...");
        reader.Connect(2000);
        Console.WriteLine("Connection established!");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- For Ethernet: Check network cable connection and IP configuration");
        Console.WriteLine("- For Serial: Check USB/RS232 cable and port settings");
        Console.WriteLine("- Verify PulsarLR is powered on and initialized");
        Console.WriteLine("- For Ethernet: Check firewall settings on port 10001");
        Console.WriteLine("- For Ethernet: Verify network connectivity (ping test)");
        Console.WriteLine("- For Serial: Ensure no other application is using the COM port");
        return;
      }

      try
      {
        // Set reader transmission power (1-30 dBm for PulsarLR, higher values = longer range)
        // 14 dBm provides good balance between range and interference
        reader.SetPower(14);
        Console.WriteLine("Reader power set to 14 dBm");

        // Use antenna 1
        reader.SetAntenna(1);
        Console.WriteLine("Reader uses antenna 1");

        // Configure advanced inventory settings for optimal performance
        InventorySettings invSettings = reader.GetInventorySettings();
        invSettings.WithRssi = true;        // Include signal strength (RSSI) in responses
        invSettings.WithTid = true;         // Include Tag Identifier (TID) for additional tag info
        invSettings.OnlyNewTag = false;     // Report all tags, not just newly detected ones
        reader.SetInventorySettings(invSettings);
        Console.WriteLine("Advanced inventory settings configured (RSSI: ON, TID: ON)");

        // Perform a single inventory scan to detect currently present tags
        // This also triggers the NewInventory event if listeners are registered
        Console.WriteLine("\nPerforming single inventory scan...");
        List<UhfTag> tags = reader.GetSingleInventory();
        Console.WriteLine($"Current inventory: {tags.Count} UHF Tag(s) found");

        foreach (UhfTag tag in tags)
        {
          Console.WriteLine(
            $"  EPC: {tag.EPC}" +
            $" | Ant: {tag.Antenna}" +
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
        Console.WriteLine("\nPossible causes:");
        Console.WriteLine("- No UHF tags in range");
        Console.WriteLine("- RF interference in UHF band");
        Console.WriteLine("- Tag orientation or distance issues");
        Console.WriteLine("- Reader antenna configuration problems");
      }
      finally
      {
        // Always disconnect to free network resources
        if (reader.Connected)
        {
          reader.Disconnect();
          Console.WriteLine("Connection closed");
        }
      }
    }

    /// <summary>
    /// Demonstrates multi-antenna inventory operations with the PulsarLR reader.
    /// Shows how to use GetMultiInventory to scan multiple antennas simultaneously and analyze antenna-specific results.
    /// </summary>
    public static void MultiAntennaInventoryExample()
    {
      // Create the reader instance - PulsarLR supports both Ethernet and Serial communication

      // Option 1: Ethernet connection (recommended for permanent installations)
      PulsarLR reader = new PulsarLR("plr-000143.local", 10001);

      // Option 2: Serial connection (useful for direct connection or mobile applications)
      // PulsarLR reader = new PulsarLR("COM8");

      // Subscribe to reader connection status changes
      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} Multi-Antenna Reader status: {e.Message} ({e.Status})");

      // Subscribe to inventory events for multi-antenna scanning
      reader.NewInventory += (s, e) =>
      {
        Console.WriteLine($"{e.Timestamp} Multi-Antenna Inventory event! {e.Tags.Count} UHF Tag(s) found");

        foreach (UhfTag tag in e.Tags)
        {
          Console.WriteLine(
            $"  EPC: {tag.EPC}" +
            $" | Ant: {tag.Antenna}" +
            (!string.IsNullOrEmpty(tag.TID) ? $" | TID: {tag.TID}" : "") +
            (tag.RSSI != null ? $" | RSSI: {tag.RSSI} dBm" : ""));
        }
      };

      try
      {
        Console.WriteLine("Connecting to PulsarLR for multi-antenna operations...");
        reader.Connect(2000);
        Console.WriteLine("Multi-antenna connection established!");
        Console.WriteLine($"Reader Firmware: {reader.FirmwareVersion}");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to multi-antenna reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- For Ethernet: Check network cable connection and IP configuration");
        Console.WriteLine("- For Serial: Check USB/RS232 cable and COM port settings");
        Console.WriteLine("- Verify PulsarLR is powered on and all antennas are connected");
        Console.WriteLine("- Check antenna cable connections and impedance matching");
        Console.WriteLine("- Ensure antennas are not physically obstructed");
        return;
      }

      try
      {
        // Configure antenna sequence for multi-antenna operation (1,2,3,4)
        Console.WriteLine("Configuring antenna sequence and individual power levels...");

        // Set antenna sequence to 1,2,3,4
        reader.SetAntennaMultiplex(new List<int> { 1, 2, 3, 4 });
        Console.WriteLine("Antenna sequence set to: 1, 2, 3, 4");

        // Set individual antenna power levels to 15 dBm for each antenna
        reader.SetAntennaPower(1, 15);  // Antenna 1: 15 dBm
        reader.SetAntennaPower(2, 15);  // Antenna 2: 15 dBm
        reader.SetAntennaPower(3, 15);  // Antenna 3: 15 dBm
        reader.SetAntennaPower(4, 15);  // Antenna 4: 15 dBm
        Console.WriteLine("Individual antenna power levels configured:");
        Console.WriteLine("  Antenna 1: 15 dBm");
        Console.WriteLine("  Antenna 2: 15 dBm");
        Console.WriteLine("  Antenna 3: 15 dBm");
        Console.WriteLine("  Antenna 4: 15 dBm");

        // Configure advanced inventory settings for multi-antenna scanning
        InventorySettings invSettings = reader.GetInventorySettings();
        invSettings.WithRssi = true;        // Include signal strength for antenna comparison
        invSettings.WithTid = false;        // Disable TID for faster multi-antenna scanning
        invSettings.OnlyNewTag = false;     // Report all tags from all antennas
        reader.SetInventorySettings(invSettings);
        Console.WriteLine("Multi-antenna inventory settings configured");

        // Perform multi-antenna inventory scan
        Console.WriteLine("\nPerforming multi-antenna inventory scan...");
        Console.WriteLine("This scans all connected antennas simultaneously");

        List<UhfTag> allTags = reader.GetMultipleInventory();
        Console.WriteLine($"Multi-antenna inventory complete: {allTags.Count} UHF Tag(s) found across all antennas");

        if (allTags.Count == 0)
        {
          Console.WriteLine("No tags detected on any antenna");
          Console.WriteLine("Check:");
          Console.WriteLine("- Tag placement within antenna range");
          Console.WriteLine("- Antenna connections and orientations");
          Console.WriteLine("- Power levels and RF environment");
        }
        else
        {
          // Analyze results by antenna
          var tagsByAntenna = allTags.GroupBy(tag => tag.Antenna);
          Console.WriteLine("\n=== Multi-Antenna Results Analysis ===");

          foreach (var antennaGroup in tagsByAntenna.OrderBy(g => g.Key))
          {
            int antennaNumber = antennaGroup.Key;
            var antennaTags = antennaGroup.ToList();

            Console.WriteLine($"\nAntenna {antennaNumber}: {antennaTags.Count} tag(s) detected");

            if (antennaTags.Any())
            {
              // Calculate average RSSI for this antenna
              var rssiValues = antennaTags.Where(t => t.RSSI.HasValue).Select(t => t.RSSI!.Value);
              if (rssiValues.Any())
              {
                double avgRssi = rssiValues.Average();
                int minRssi = rssiValues.Min();
                int maxRssi = rssiValues.Max();
                Console.WriteLine($"  RSSI Range: {minRssi} to {maxRssi} dBm (Avg: {avgRssi:F1} dBm)");
              }

              // List all tags on this antenna
              foreach (UhfTag tag in antennaTags)
              {
                Console.WriteLine(
            $"  EPC: {tag.EPC}" +
            (!string.IsNullOrEmpty(tag.TID) ? $" | TID: {tag.TID}" : "") +
            (tag.RSSI != null ? $" | RSSI: {tag.RSSI} dBm" : ""));
              }
            }
          }

          // Provide antenna coverage analysis
          Console.WriteLine("=== Antenna Coverage Analysis ===");
          int totalAntennas = tagsByAntenna.Count();
          Console.WriteLine($"Active antennas: {totalAntennas}");

          if (totalAntennas > 1)
          {
            // Check for tags detected by multiple antennas
            var duplicateEpcs = allTags.GroupBy(t => t.EPC)
                                     .Where(g => g.Count() > 1)
                                     .Select(g => g.Key);

            if (duplicateEpcs.Any())
            {
              Console.WriteLine("\n=== Multi-Antenna Tag Detection ===");
              foreach (string duplicateEpc in duplicateEpcs)
              {
                var duplicateTags = allTags.Where(t => t.EPC == duplicateEpc).ToList();
                Console.WriteLine($"Tag {duplicateEpc} detected by {duplicateTags.Count} antennas:");
                foreach (var tag in duplicateTags)
                {
                  Console.WriteLine($"  Antenna {tag.Antenna}: RSSI {tag.RSSI} dBm");
                }
              }
            }
          }
          else
          {
            Console.WriteLine("Single antenna detection only...");
          }
        }

        // Demonstrate continuous multi-antenna scanning
        Console.WriteLine("\nStarting continuous multi-antenna inventory scan...");
        reader.StartInventory();
        Console.WriteLine("Multi-antenna continuous scan active - Press any key to stop");
        Console.ReadKey();

        reader.StopInventory();
        Console.WriteLine("Multi-antenna continuous inventory stopped");

      }
      catch (MetratecReaderException ex)
      {
        Console.WriteLine($"Error during multi-antenna operation: {ex.Message}");
        Console.WriteLine("\nPossible causes:");
        Console.WriteLine("- Antenna connection issues (check antennas 1-4)");
        Console.WriteLine("- RF interference between antennas");
        Console.WriteLine("- Power distribution problems (15 dBm per antenna)");
        Console.WriteLine("- Antenna impedance mismatch");
        Console.WriteLine("- Invalid antenna numbers in sequence (1,2,3,4)");
        Console.WriteLine("- Antenna multiplexing configuration error");
        Console.WriteLine("- Individual antenna power setting failure");
        Console.WriteLine("- Network communication timeout");
      }
      finally
      {
        // Always disconnect to free resources
        if (reader.Connected)
        {
          reader.Disconnect();
          Console.WriteLine("Multi-antenna reader connection closed");
        }
      }
    }

    /// <summary>
    /// Demonstrates reading and writing user data to UHF tag memory with long-range reader.
    /// Shows both Ethernet and Serial connectivity options with proper error handling and data verification.
    /// </summary>
    public static void ReadWriteExample()
    {
      // Create the reader instance - PulsarLR supports both Ethernet and Serial communication

      // Option 1: Ethernet connection (recommended for permanent installations)
      PulsarLR reader = new PulsarLR("plr-000143.local", 10001);

      // Option 2: Serial connection (useful for direct connection or mobile applications)
      // PulsarLR reader = new PulsarLR("COM8");

      // Subscribe to reader connection status changes
      reader.StatusChanged += (s, e) => Console.WriteLine($"Reader status changed to {e.Message} ({e.Status})");

      // Establish network connection with 2-second timeout
      try
      {
        Console.WriteLine("Connecting to PulsarLR for read/write operations...");
        reader.Connect(2000);
        Console.WriteLine("Connection established!");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- For Ethernet: Check network cable connection and IP configuration");
        Console.WriteLine("- For Serial: Check USB/RS232 cable and COM port settings");
        Console.WriteLine("- Verify PulsarLR is powered on and initialized");
        Console.WriteLine("- For Ethernet: Check firewall settings on port 10001");
        Console.WriteLine("- For Serial: Ensure no other application is using the COM port");
        return;
      }

      try
      {
        // Set reader transmission power for optimal performance
        reader.SetPower(14);
        Console.WriteLine("Reader power set to 14 dBm");

        // Use antenna 1
        reader.SetAntenna(1);
        Console.WriteLine("Reader uses antenna 1");

        // Wait for a UHF tag to be placed within reader range
        Console.WriteLine("Please place a UHF tag near the PulsarMX reader...");
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
              Console.WriteLine($"No tags found after {attempts} attempts. Continuing to search...");
              Console.WriteLine("Make sure you have a UHF/EPC Gen2 compatible tag");
            }
            System.Threading.Thread.Sleep(1000);
          }
        } while (tags.Count == 0 && attempts < 30);

        if (tags.Count == 0)
        {
          Console.WriteLine("No tags found after 30 seconds. Please check tag compatibility and placement.");
          return;
        }

        // Use the first detected tag for read/write operations
        UhfTag tag = tags[0];
        Console.WriteLine($"UHF tag found: {tag.EPC}");
        if (!string.IsNullOrEmpty(tag.TID))
          Console.WriteLine($"TID: {tag.TID}");

        // Attempt to read user data from address 0 in the User memory bank
        // Parameters: address (word offset), length (number of words to read)
        Console.WriteLine("\nReading user data from address 0...");
        try
        {
          List<UhfTag> resp = reader.ReadTagUsrData(0, 4); // Read 4 bytes

          if (resp.Count == 0)
          {
            Console.WriteLine("No tag found during read operation");
          }
          else if (resp[0].HasError)
          {
            Console.WriteLine($"Error reading user data: {resp[0].Message}");
            Console.WriteLine("Possible causes:");
            Console.WriteLine("- Tag doesn't support user memory");
            Console.WriteLine("- Access password required");
            Console.WriteLine("- Tag moved out of range during read");
          }
          else
          {
            Console.WriteLine($"Read data from address 0: {resp[0].Data}");
          }
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"Read operation failed: {ex.Message}");
        }

        // Attempt to write user data to address 0 in the User memory bank
        // Data format: hex string (each pair represents one byte)
        string dataToWrite = "01020304"; // 4 bytes as hex string
        Console.WriteLine($"\nWriting data '{dataToWrite}' to address 0...");
        try
        {
          List<UhfTag> resp = reader.WriteTagUsrData(0, dataToWrite);

          if (resp.Count == 0)
          {
            Console.WriteLine("No tag found during write operation");
          }
          else if (resp[0].HasError)
          {
            Console.WriteLine($"Error writing user data: {resp[0].Message}");
            Console.WriteLine("Possible causes:");
            Console.WriteLine("- Tag is read-only or write-protected");
            Console.WriteLine("- Access password required");
            Console.WriteLine("- Tag moved out of range during write");
            Console.WriteLine("- Insufficient power for write operation");
          }
          else
          {
            Console.WriteLine("Data written successfully!");
          }

          // Verify written data
          Console.WriteLine("\nVerifying written data...");
          List<UhfTag> verifyResp = reader.ReadTagUsrData(0, 4);
          if (verifyResp.Count > 0 && !verifyResp[0].HasError)
          {
            Console.WriteLine($"Verification read: {verifyResp[0].Data}");
            if (verifyResp[0].Data?.ToUpper() == dataToWrite.ToUpper())
            {
              Console.WriteLine("Data verification successful!");
            }
            else
            {
              Console.WriteLine("Data mismatch - write may have failed");
            }
          }
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"Write operation failed: {ex.Message}");
        }
      }
      catch (MetratecReaderException ex)
      {
        Console.WriteLine($"General reader error: {ex.Message}");
      }
      finally
      {
        // Always disconnect to free network resources
        if (reader.Connected)
        {
          reader.Disconnect();
          Console.WriteLine("Connection closed");
        }
      }
    }

    /// <summary>
    /// Demonstrates advanced Impinj-specific authentication and security features.
    /// Shows how to use custom Impinj settings and authentication services for enhanced security.
    /// </summary>
    public static void CustomImpinjExample()
    {
      // Create the reader instance - PulsarLR supports both Ethernet and Serial communication

      // Option 1: Ethernet connection (recommended for permanent installations)
      PulsarLR reader = new PulsarLR("plr-000143.local", 10001);

      // Option 2: Serial connection (useful for direct connection or mobile applications)
      // PulsarLR reader = new PulsarLR("COM8");

      // Subscribe to reader connection status changes
      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} Reader status changed to {e.Message} ({e.Status})");

      // Subscribe to inventory events for authenticated tags
      reader.NewInventory += (s, e) =>
      {
        Console.WriteLine($"{e.Timestamp} New inventory event! {e.Tags.Count} Tag(s) found");
        foreach (UhfTag tag in e.Tags)
        {
          Console.WriteLine($"  EPC: {tag.EPC}");
        }
      };

      // Subscribe to GPIO input changes
      reader.InputChanged += (s, e) => Console.WriteLine($"Input Changed: {e.Pin} {e.IsHigh}");

      // Establish network connection with 2-second timeout
      try
      {
        Console.WriteLine("Connecting to PulsarLR for Impinj features...");
        reader.Connect(2000);
        Console.WriteLine("Connection established!");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- Check network cable connection");
        Console.WriteLine("- Verify PulsarLR IP address configuration");
        Console.WriteLine("- Ensure reader is powered on");
        return;
      }

      try
      {
        // Set reader transmission power for long-range operations
        reader.SetPower(14);
        Console.WriteLine("Reader power set to 14 dBm for long-range operation");

        // Use antenna 1
        reader.SetAntenna(1);
        Console.WriteLine("Reader uses antenna 1");

        // Configure Impinj-specific custom settings for enhanced performance
        CustomImpinjSettings impinjSettings = new CustomImpinjSettings();
        impinjSettings.FastId = false;      // Disable FastID for better compatibility
        impinjSettings.TagFocus = false;    // Disable TagFocus to read all tags in field
        reader.SetCustomImpinjSettings(impinjSettings);
        Console.WriteLine("Impinj custom settings configured (FastID: OFF, TagFocus: OFF)");

        // Call Impinj Authentication Service for secure tag operations
        // This service provides cryptographic authentication for compatible Impinj tags
        Console.WriteLine("\n=== Calling Impinj Authentication Service ===");
        List<UhfTagAuth> tags = reader.CallImpinjAuthenticationService();
        Console.WriteLine($"Authentication scan completed: {tags.Count} Tag(s) processed");

        foreach (UhfTagAuth tag in tags)
        {
          Console.WriteLine($"\nTag EPC: {tag.EPC}");
          if (tag.HasError)
          {
            Console.WriteLine($"Authentication Error: {tag.Message}");
            Console.WriteLine("  Possible causes:");
            Console.WriteLine("  - Tag doesn't support Impinj authentication");
            Console.WriteLine("  - Communication error during challenge-response");
            Console.WriteLine("  - Wrong Impinj configuration settings");
          }
          else
          {
            Console.WriteLine("Authentication successful!");
            Console.WriteLine($"  Short TID: {tag.ShortTID}");
            Console.WriteLine($"  Response: {tag.Response}");
            Console.WriteLine($"  Challenge: {tag.Challenge}");
            Console.WriteLine("  This data can be used with Impinj Authentication Service");
          }
        }

        Console.WriteLine("\nImpinj features demonstration completed - Press any key to exit");
        Console.ReadKey();

        // Stop any ongoing operations
        reader.StopInventory();
      }
      catch (MetratecReaderException ex)
      {
        Console.WriteLine($"Error during Impinj operations: {ex.Message}");
        Console.WriteLine("\nThis feature requires:");
        Console.WriteLine("- Compatible Impinj tags (M775, etc.)");
        Console.WriteLine("- Reader firmware with Impinj support");
        Console.WriteLine("- Proper Impinj settings configuration");
      }
      finally
      {
        // Always disconnect to free network resources
        if (reader.Connected)
        {
          reader.Disconnect();
          Console.WriteLine("Connection closed");
        }
      }
    }

  }
}