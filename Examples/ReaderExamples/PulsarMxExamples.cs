using System;
using System.Collections.Generic;
using MetraTecDevices;

namespace ReaderExamples
{
  /// <summary>
  /// Examples demonstrating PulsarMX reader operations for Ultra High Frequency (UHF) RFID tags.
  /// This reader provides mid-range UHF capabilities with GPIO control and dual connectivity options.
  /// Uses AT command protocol over both Ethernet and Serial communication with industrial-grade features.
  /// </summary>
  internal class PulsarMxExamples
  {
    /// <summary>
    /// Demonstrates basic UHF inventory operations with dual connectivity options and GPIO monitoring.
    /// Shows how to configure the reader for optimal performance in industrial environments using both Ethernet and Serial connections.
    /// </summary>
    public static void InventoryExample()
    {
      // Create the reader instance - PulsarMX supports both Ethernet and Serial communication

      // Option 1: Ethernet connection (recommended for permanent industrial installations)
      PulsarMX reader = new PulsarMX("192.168.2.203", 10001);

      // Option 2: Serial connection (useful for development, testing, or direct connection)
      // PulsarMX reader = new PulsarMX("COM8");

      // Subscribe to reader connection status changes (Connected/Disconnected)
      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} Reader status changed to {e.Message} ({e.Status})");

      // Subscribe to inventory events - triggered when tags are detected during continuous scanning
      reader.NewInventory += (s, e) =>
      {
        Console.WriteLine($"{e.Timestamp} New inventory event! {e.Tags.Count} UHF Tag(s) found");
        foreach (UhfTag tag in e.Tags)
        {
          // Display comprehensive tag information for UHF tags
          Console.WriteLine($"  EPC: {tag.EPC}" +
            (tag.RSSI != null ? $" | RSSI: {tag.RSSI} dBm" : ""));

        }
      };

      // Subscribe to GPIO input changes - useful for external triggers, sensors, or switches
      reader.InputChanged += (s, e) => Console.WriteLine($"Input Changed: {e.Pin} {e.IsHigh}");

      // Establish network connection to the reader with 2-second timeout
      try
      {
        Console.WriteLine("Connecting to PulsarMX...");
        reader.Connect(2000);
        Console.WriteLine("Connection established!");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- For Ethernet: Check network cable connection and IP configuration");
        Console.WriteLine("- For Serial: Check USB/RS232 cable and COM port settings");
        Console.WriteLine("- Verify PulsarMX is powered on and initialized");
        Console.WriteLine("- For Ethernet: Check firewall settings on port 10001");
        Console.WriteLine("- For Ethernet: Verify network connectivity (ping test)");
        Console.WriteLine("- For Serial: Ensure no other application is using the COM port");
        return;
      }

      try
      {
        // Set reader transmission power (1-25 dBm for PulsarMX, adjust based on application needs)
        // 14 dBm provides good balance between range and power consumption
        reader.SetPower(14);
        Console.WriteLine("Reader power set to 14 dBm");

        // Perform a single inventory scan to detect currently present tags
        // This also triggers the NewInventory event if listeners are registered
        Console.WriteLine("\nPerforming single inventory scan...");
        List<UhfTag> tags = reader.GetInventory();
        Console.WriteLine($"Current inventory: {tags.Count} UHF Tag(s) found");

        foreach (UhfTag tag in tags)
        {
          Console.WriteLine($"  EPC: {tag.EPC}" + (tag.RSSI != null ? $" | RSSI: {tag.RSSI} dBm" : ""));
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
        Console.WriteLine("- Network communication timeout");
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
    /// Demonstrates reading and writing user data to UHF tag memory with dual connectivity support.
    /// Shows how to access the User memory bank of EPC Gen2 tags for custom data storage with proper verification using both Ethernet and Serial connections.
    /// </summary>
    public static void ReadWriteExample()
    {
      // Create the reader instance - PulsarMX supports both Ethernet and Serial communication

      // Option 1: Ethernet connection (recommended for permanent industrial installations)
      PulsarMX reader = new PulsarMX("192.168.2.203", 10001);

      // Option 2: Serial connection (useful for development, testing, or direct connection)
      // PulsarMX reader = new PulsarMX("COM8");

      // Subscribe to reader connection status changes
      reader.StatusChanged += (s, e) => Console.WriteLine($"Reader status changed to {e.Message} ({e.Status})");

      // Establish network connection with 2-second timeout
      try
      {
        Console.WriteLine("Connecting to PulsarMX for read/write operations...");
        reader.Connect(2000);
        Console.WriteLine("Connection established!");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- For Ethernet: Check network cable connection and IP configuration");
        Console.WriteLine("- For Serial: Check USB/RS232 cable and COM port settings");
        Console.WriteLine("- Verify PulsarMX is powered on and initialized");
        Console.WriteLine("- For Ethernet: Check firewall settings on port 10001");
        Console.WriteLine("- For Serial: Ensure no other application is using the COM port");
        return;
      }

      try
      {
        // Set reader transmission power for optimal read/write performance
        reader.SetPower(14);
        Console.WriteLine("Reader power set to 14 dBm");

        // Wait for a UHF tag to be placed within reader range
        Console.WriteLine("Please place a UHF tag near the PulsarMX reader...");
        List<UhfTag> tags;
        int attempts = 0;
        do
        {
          tags = reader.GetInventory();
          if (tags.Count == 0)
          {
            attempts++;
            if (attempts % 5 == 0)
            {
              Console.WriteLine($"No tags found after {attempts} attempts. Continuing to search...");
              Console.WriteLine("Make sure you have a UHF/EPC Gen2 compatible tag");
              Console.WriteLine("Try placing the tag closer to the reader antenna");
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
        Console.WriteLine($"EPC: {tag.EPC}" +
            (tag.RSSI != null ? $" | RSSI: {tag.RSSI} dBm" : ""));


        // Attempt to read user data from address 0 in the User memory bank
        // Parameters: address (word offset), length (number of words to read)
        // Each word is 16 bits (2 bytes), so reading 2 words = 4 bytes
        Console.WriteLine("\nReading user data from address 0...");
        try
        {
          List<UhfTag> resp = reader.ReadTagUsrData(0, 2); // Read 2 words = 4 bytes

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
            Console.WriteLine("- Insufficient power for read operation");
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
        // Data format: hex string where each pair represents one byte (01020304 = 4 bytes)
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
            Console.WriteLine("- Tag memory is full or corrupted");
          }
          else
          {
            Console.WriteLine("Data written successfully!");
          }

          // Verify written data by reading it back
          Console.WriteLine("\nVerifying written data...");
          List<UhfTag> verifyResp = reader.ReadTagUsrData(0, 2);
          if (verifyResp.Count > 0 && !verifyResp[0].HasError)
          {
            Console.WriteLine($"Verification read: {verifyResp[0].Data}");
            if (verifyResp[0].Data?.ToUpper() == dataToWrite.ToUpper())
            {
              Console.WriteLine("✓ Data verification successful!");
            }
            else
            {
              Console.WriteLine("⚠ Data mismatch - write may have failed");
            }
          }
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"Write operation failed: {ex.Message}");
        }

        // Demonstrate TID reading for tag identification
        Console.WriteLine("\nReading Tag Identifier (TID)...");
        try
        {
          List<UhfTag> tidResp = reader.ReadTagTid(0, 6); // Read 6 words of TID

          if (tidResp.Count > 0 && !tidResp[0].HasError)
          {
            Console.WriteLine($"TID: {tidResp[0].TID}");
            Console.WriteLine("TID contains manufacturer and tag model information");
          }
          else if (tidResp.Count > 0 && tidResp[0].HasError)
          {
            Console.WriteLine($"Error reading TID: {tidResp[0].Message}");
          }
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"TID read failed: {ex.Message}");
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
  }
}