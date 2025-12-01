using System;
using System.Collections.Generic;
using MetraTecDevices;

namespace ReaderExamples
{
  /// <summary>
  /// Examples demonstrating QuasarLR reader operations for High Frequency (HF) RFID tags.
  /// This HF long-range reader is designed for demanding industrial applications requiring
  /// high reading reliability, extended read ranges, and extensive special tag features.
  /// Supports both Ethernet and serial connectivity with high-power operations (500-8000mW).
  /// </summary>
  internal class QuasarLRExamples
  {
    /// <summary>
    /// Demonstrates basic HF long-range inventory operations using the QuasarLR reader.
    /// Shows how to detect ISO15693 tags at extended distances with high-power operations.
    /// </summary>
    public static void LongRangeInventoryExample()
    {
      // Create the QuasarLR reader instance - supports both Ethernet and Serial connectivity
      // Option 1: Ethernet connection (recommended for industrial applications)
      QuasarLR reader = new QuasarLR("192.168.1.100", 10001);

      // Option 2: Serial connection (for direct connection applications)
      // QuasarLR reader = new QuasarLR("COM8");

      // Subscribe to reader connection status changes (Connected/Disconnected)
      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} QuasarLR status changed to {e.Message} ({e.Status})");

      // Subscribe to inventory events - triggered when tags are detected during continuous scanning
      reader.NewInventory += (s, e) =>
      {
        Console.WriteLine($"{e.Timestamp} Long-range inventory event! {e.Tags.Count} HF Tag(s) found");
        foreach (HfTag tag in e.Tags)
        {
          // Display comprehensive HF tag information with range estimation
          Console.WriteLine($"  TID: {tag.TID}");
        }
      };

      // Establish connection to the reader with 3-second timeout (longer for network)
      try
      {
        Console.WriteLine("Connecting to QuasarLR long-range HF reader...");
        reader.Connect(3000);
        Console.WriteLine("Connection established!");
        Console.WriteLine($"Reader Firmware: {reader.FirmwareVersion}");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- For Ethernet: Check network cable and IP configuration");
        Console.WriteLine("- For Serial: Check USB cable connection and COM port");
        Console.WriteLine("- Verify QuasarLR is properly powered (industrial power supply)");
        Console.WriteLine("- Check if another application is using the device");
        Console.WriteLine("- Ensure external antenna is properly connected");
        Console.WriteLine("- Verify this is QuasarLR (HF long-range), not QuasarMX");
        Console.WriteLine("- Check firewall settings for network connection");
        return;
      }

      try
      {
        // Set high power for long-range operations (500-8000 mW)
        // QuasarLR supports much higher power levels than standard readers
        int powerLevel = 4000; // 4000 mW = 5W for long-range operations
        reader.SetPower(powerLevel);
        Console.WriteLine($"QuasarLR power set to {powerLevel} mW (long-range mode)");

        // Perform a single long-range inventory scan
        Console.WriteLine("\nPerforming long-range inventory scan...");
        List<HfTag> tags = reader.GetInventory();
        Console.WriteLine($"Long-range inventory: {tags.Count} HF Tag(s) found");

        foreach (HfTag tag in tags)
        {
          Console.WriteLine($"TID: {tag.TID}");
          Console.WriteLine();
        }

        // Start continuous long-range scanning
        Console.WriteLine("Starting continuous long-range inventory scan...");
        reader.StartInventory();
        Console.WriteLine("Long-range scan active - monitoring extended detection zone");
        Console.WriteLine("Press any key to stop long-range scanning");
        Console.ReadKey();

        // Stop the continuous scanning
        reader.StopInventory();
        Console.WriteLine("Long-range inventory stopped");
      }
      catch (MetratecReaderException ex)
      {
        Console.WriteLine($"Error during long-range operation: {ex.Message}");
        Console.WriteLine("\nPossible causes:");
        Console.WriteLine("- No HF/ISO15693 tags in extended range");
        Console.WriteLine("- External antenna connection issues");
        Console.WriteLine("- RF interference in HF band (13.56 MHz)");
        Console.WriteLine("- High power operation requires proper setup");
        Console.WriteLine("- Tag orientation critical at long range");
        Console.WriteLine("- Industrial environment RF noise");
      }
      finally
      {
        // Always disconnect to free resources and reduce power
        if (reader.Connected)
        {
          reader.Disconnect();
          Console.WriteLine("Long-range reader connection closed");
        }
      }
    }

    /// <summary>
    /// Demonstrates advanced read/write operations with the QuasarLR reader.
    /// Shows how to access tag memory at extended distances with high-power operations.
    /// </summary>
    public static void LongRangeReadWriteExample()
    {
      // Create the QuasarLR reader instance for read/write operations
      QuasarLR reader = new QuasarLR("192.168.1.100", 10001);

      // Subscribe to reader connection status changes
      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} QuasarLR status: {e.Message} ({e.Status})");

      // Establish connection to the reader
      try
      {
        Console.WriteLine("Connecting to QuasarLR for long-range read/write operations...");
        reader.Connect(3000);
        Console.WriteLine("Connection established!");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- Check network connection (Ethernet recommended for QuasarLR)");
        Console.WriteLine("- Verify IP address and port configuration");
        Console.WriteLine("- Ensure industrial power supply is connected");
        Console.WriteLine("- Check external antenna connections");
        Console.WriteLine("- Verify QuasarLR is powered on and initialized");
        return;
      }

      try
      {
        // Set high power for reliable read/write operations at distance
        int powerLevel = 4000; // 4W for reliable read/write operations
        reader.SetPower(powerLevel);
        Console.WriteLine($"QuasarLR power set to {powerLevel} mW for read/write operations");

        // Wait for an HF tag to be placed within the extended detection range
        Console.WriteLine("\nPlace an HF/ISO15693 tag within the QuasarLR detection zone...");
        Console.WriteLine("Note: QuasarLR can detect tags at significantly greater distances");

        List<HfTag> tags;
        int attempts = 0;
        do
        {
          tags = reader.GetInventory();
          if (tags.Count == 0)
          {
            attempts++;
            if (attempts % 5 == 0)
            {
              Console.WriteLine($"Scanning for tags... ({attempts} attempts)");
              Console.WriteLine("QuasarLR is scanning extended range - tags can be further away");
              Console.WriteLine("Try placing tag near external antenna");
              Console.WriteLine($"Current power: {powerLevel} mW provides extended range");
            }
            System.Threading.Thread.Sleep(1000);
          }
        } while (tags.Count == 0 && attempts < 30);

        if (tags.Count == 0)
        {
          Console.WriteLine("No tags found after 30 seconds. Check tag placement and antenna connection.");
          return;
        }

        // Use the first detected tag for read/write operations
        HfTag tag = tags[0];
        Console.WriteLine($"  TID: {tag.TID}");

        // Attempt to read data from memory block 0 (usually contains tag info)
        Console.WriteLine("\nReading data from memory block 0...");
        try
        {
          // Read 4 bytes from block 0 using the tag's UID/TID
          string resp = reader.ReadBlock(0, tag.TID);
          Console.WriteLine($"Read data from block 0: {resp}");
        }
        catch (TransponderException e)
        {
          Console.WriteLine($"Error reading block 0: {e.Message}");
          Console.WriteLine("Possible causes:");
          Console.WriteLine("- Block is protected or locked");
          Console.WriteLine("- Tag moved out of range during read");
          Console.WriteLine("- Unsupported block address");
          Console.WriteLine("- Tag communication error");
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"Reader error during read: {ex.Message}");
        }

        // Attempt to write new data to memory block 1 (avoiding block 0 which may contain system data)
        string dataToWrite = "01020304"; // 4 bytes as hex string
        Console.WriteLine($"\nWriting data '{dataToWrite}' to memory block 1...");
        try
        {
          // Write 4 bytes to block 1
          reader.WriteBlock(1, dataToWrite, tag.TID);
          Console.WriteLine("Data written successfully to block 1!");
        }
        catch (TransponderException e)
        {
          Console.WriteLine($"Error writing to block 1: {e.Message}");
          Console.WriteLine("Possible causes:");
          Console.WriteLine("- Block is write-protected or read-only");
          Console.WriteLine("- Tag moved out of range during write");
          Console.WriteLine("- Insufficient power for write operation");
          Console.WriteLine("- Tag doesn't support writes to this block");
          Console.WriteLine("- Authentication required");
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"Reader error during write: {ex.Message}");
        }

        // Verify written data by reading it back
        Console.WriteLine("\nVerifying written data - reading block 1...");
        try
        {
          string verifyData = reader.ReadBlock(1, tag.TID);
          Console.WriteLine($"Verification read from block 1: {verifyData}");

          if (verifyData?.ToUpper() == dataToWrite.ToUpper())
          {
            Console.WriteLine("Data verification successful!");
          }
          else
          {
            Console.WriteLine("Data mismatch - write may have been partial or failed");
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Verification read failed: {ex.Message}");
        }

        // Demonstrate reading multiple blocks
        Console.WriteLine("\nReading multiple blocks (0-3)...");
        try
        {
          for (int block = 0; block < 4; block++)
          {
            try
            {
              string blockData = reader.ReadBlock(block, tag.TID);
              Console.WriteLine($"  Block {block}: {blockData}");
            }
            catch (TransponderException ex)
            {
              Console.WriteLine($"  Block {block}: Error - {ex.Message}");
            }
          }
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"Multi-block read failed: {ex.Message}");
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