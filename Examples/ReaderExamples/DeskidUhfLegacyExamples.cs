using System;
using System.Collections.Generic;
using MetraTecDevices;

namespace ReaderExamples
{
  /// <summary>
  /// Examples demonstrating DeskID_UHF (Legacy) reader operations for Ultra High Frequency (UHF) RFID tags.
  /// This is the legacy version that uses ASCII protocol, different from the newer DeskID_UHF_v2 (AT protocol).
  /// Shows serial connection setup, inventory operations, and tag memory access for EPC Gen2 tags.
  /// </summary>
  internal class DeskidUhfLegacyExamples
  {
    /// <summary>
    /// Demonstrates basic UHF inventory operations using the legacy DeskID_UHF reader with ASCII protocol.
    /// Shows how to detect EPC Gen2 tags and handle inventory events with proper error handling.
    /// </summary>
    public static void InventoryExample()
    {
      // Create the legacy reader instance using serial communication (ASCII protocol)
      // Note: Update "/dev/ttyUSB0" to match your actual device path (Linux/Mac) or "COM#" for Windows
      String port = "/dev/ttyUSB0";
      DeskID_UHF reader = new DeskID_UHF(port, REGION.ETS); // ETSI for Europe

      // Subscribe to reader connection status changes (Connected/Disconnected)
      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} Reader status changed to {e.Message} ({e.Status})");

      // Subscribe to inventory events - triggered when tags are detected during continuous scanning
      reader.NewInventory += (s, e) =>
      {
        Console.WriteLine($"{e.Timestamp} New inventory event! {e.Tags.Count} UHF Tag(s) found");
        foreach (UhfTag tag in e.Tags)
        {
          // Display comprehensive UHF tag information
          Console.WriteLine($"  EPC: {tag.EPC}" + 
            (tag.RSSI != null ? $" | RSSI: {tag.RSSI} dBm" : ""));
        }
      };

      // Establish connection to the reader with 2-second timeout
      try
      {
        Console.WriteLine("Connecting to DeskID_UHF (Legacy)...");
        reader.Connect(2000);
        Console.WriteLine("Connection established!");
        Console.WriteLine($"Reader Firmware: {reader.FirmwareVersion}");
        Console.WriteLine($"Protocol: ASCII (Legacy)");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- Check USB cable connection");
        Console.WriteLine($"- Verify COM port (currently set to {port})");
        Console.WriteLine("- Ensure DeskID_UHF driver is installed");
        Console.WriteLine("- Check if another application is using the COM port");
        Console.WriteLine("- Verify reader is powered on");
        Console.WriteLine("- Note: This is for legacy DeskID_UHF, not DeskID_UHF_v2");
        return;
      }

      try
      {
        // Set reader transmission power (-2 to 17 dBm for legacy DeskID_UHF)
        // Lower range compared to newer readers, but sufficient for desktop use
        reader.SetPower(1);
        Console.WriteLine("Reader power set to 1 dBm");

        // Note: Legacy DeskID_UHF has simpler inventory settings compared to v2
        // No advanced inventory settings like RSSI/TID control available

        // Perform a single inventory scan to detect currently present tags
        Console.WriteLine("\nPerforming single inventory scan...");
        List<UhfTag> tags = reader.GetInventory();
        Console.WriteLine($"Current inventory: {tags.Count} UHF Tag(s) found");

        foreach (UhfTag tag in tags)
        {
          Console.WriteLine($"EPC: {tag.EPC}" + 
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
        Console.WriteLine("- No UHF tags in range (place tag closer to reader)");
        Console.WriteLine("- Tag orientation issues (try different angles)");
        Console.WriteLine("- RF interference in UHF band");
        Console.WriteLine("- Reader antenna problems");
        Console.WriteLine("- Legacy firmware limitations");
      }
      finally
      {
        // Always disconnect to free resources and close serial port
        if (reader.Connected)
        {
          reader.Disconnect();
          Console.WriteLine("Connection closed");
        }
      }
    }

    /// <summary>
    /// Demonstrates reading and writing user data to UHF tag memory using the legacy DeskID_UHF reader.
    /// Shows how to access the User memory bank of EPC Gen2 tags with ASCII protocol commands.
    /// </summary>
    public static void ReadWriteExample()
    {
      // Create the legacy reader instance using serial communication
      // Note: Update "/dev/ttyUSB0" to match your actual device path (Linux/Mac) or "COM#" for Windows
      String port = "/dev/ttyUSB0";
      DeskID_UHF reader = new DeskID_UHF(port, REGION.ETS);

      // Subscribe to reader connection status changes
      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} Reader status changed to {e.Message} ({e.Status})");

      // Establish connection to the reader with 2-second timeout
      try
      {
        Console.WriteLine("Connecting to DeskID_UHF (Legacy) for read/write operations...");
        reader.Connect(2000);
        Console.WriteLine("Connection established!");
        Console.WriteLine($"Using ASCII protocol with firmware: {reader.FirmwareVersion}");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- Check USB cable connection");
        Console.WriteLine($"- Verify COM port (currently set to {port})");
        Console.WriteLine("- Ensure DeskID_UHF driver is installed");
        Console.WriteLine("- Check if another application is using the COM port");
        Console.WriteLine("- Verify this is legacy DeskID_UHF (not v2)");
        return;
      }

      try
      {
        // Set reader transmission power for optimal read/write performance
        reader.SetPower(5);
        Console.WriteLine("Reader power set to 5 dBm for read/write operations");

        // Wait for a UHF tag to be placed on the reader
        Console.WriteLine("Please place a UHF tag on the DeskID_UHF reader...");
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
              Console.WriteLine("Legacy DeskID_UHF has shorter range than newer models");
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
        Console.WriteLine($"UHF tag found: {tag.EPC} " + (tag.RSSI != null ? $" | RSSI: {tag.RSSI} dBm" : ""));

        // Attempt to read user data from address 0 in the User memory bank
        // Legacy DeskID_UHF uses ASCII protocol for read/write operations
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
            Console.WriteLine("- ASCII protocol limitations");
            Console.WriteLine("- Legacy firmware restrictions");
          }
          else
          {
            Console.WriteLine($"Read data from address 0: {resp[0].Data}");
          }
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"Read operation failed: {ex.Message}");
          Console.WriteLine("Note: Legacy DeskID_UHF may have limited read capabilities");
        }

        // Attempt to write user data to address 0 in the User memory bank
        string dataToWrite = "ABCDEF01"; // 4 bytes as hex string
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
            Console.WriteLine("- Tag doesn't support user memory writes");
            Console.WriteLine("- ASCII protocol limitations");
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

        // Attempt to read TID (if supported by legacy firmware)
        Console.WriteLine("\nAttempting TID read...");
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
            Console.WriteLine($"TID read not supported or failed: {tidResp[0].Message}");
            Console.WriteLine("This is normal for legacy DeskID_UHF firmware");
          }
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"TID read not available: {ex.Message}");
          Console.WriteLine("TID reading may not be supported in legacy firmware");
        }

      }
      catch (MetratecReaderException ex)
      {
        Console.WriteLine($"General reader error: {ex.Message}");
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