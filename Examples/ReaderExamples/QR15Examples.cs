using System;
using System.Collections.Generic;
using MetraTecDevices;

namespace ReaderExamples
{
  /// <summary>
  /// Examples demonstrating QR15 reader operations for High Frequency (HF) RFID tags.
  /// This HF RFID module with integrated antenna is designed for easy integration into electronics.
  /// Supports both serial and Ethernet connectivity with ISO15693 tag operations using ASCII protocol.
  /// </summary>
  internal class QR15Examples
  {
    /// <summary>
    /// Demonstrates basic HF inventory operations using the QR15 module with integrated antenna.
    /// Shows how to detect ISO15693 tags with both serial and network connectivity options.
    /// </summary>
    public static void InventoryExample()
    {
      // Create the QR15 reader instance - supports both Serial and Ethernet connectivity
      // Option 1: Serial connection (USB/RS232)
      // Note: Update "/dev/ttyUSB0" to match your actual device path (Linux/Mac) or "COM#" for Windows
      String port = "/dev/ttyUSB0";
      QR15 reader = new QR15(port);
      
      // Option 2: Ethernet connection (for network-enabled variants)
      // QR15 reader = new QR15("192.168.1.100", 10001);

      // Subscribe to reader connection status changes (Connected/Disconnected)
      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} Reader status changed to {e.Message} ({e.Status})");
      
      // Subscribe to inventory events - triggered when tags are detected during continuous scanning
      reader.NewInventory += (s, e) =>
      {
        Console.WriteLine($"{e.Timestamp} New inventory event! {e.Tags.Count} HF Tag(s) found");
        foreach (HfTag tag in e.Tags)
        {
          Console.WriteLine($"  TID: {tag.TID}");
        }
      };
      
      // Establish connection to the reader with 2-second timeout
      try
      {
        Console.WriteLine("Connecting to QR15 HF RFID module...");
        reader.Connect(2000);
        Console.WriteLine("Connection established!");
        Console.WriteLine($"Reader Firmware: {reader.FirmwareVersion}");
        Console.WriteLine($"Protocol: ASCII (Integrated Antenna Module)");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- For Serial: Check USB cable connection and COM port");
        Console.WriteLine("- For Ethernet: Check network cable and IP configuration");
        Console.WriteLine("- Verify QR15 module is properly powered");
        Console.WriteLine("- Check if another application is using the device");
        Console.WriteLine("- Ensure integrated antenna is not obstructed");
        Console.WriteLine("- Verify this is QR15 (HF) module, not QR-series UHF");
        return;
      }
      
      try
      {
        // Perform a single inventory scan to detect currently present tags
        Console.WriteLine("\nPerforming single inventory scan...");
        List<HfTag> tags = reader.GetInventory();
        Console.WriteLine($"Current inventory: {tags.Count} HF Tag(s) found");
        
        foreach (HfTag tag in tags)
        {
          Console.WriteLine($"TID: {tag.TID}");
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
        Console.WriteLine("- No HF/ISO15693 tags in range (place tag closer to module)");
        Console.WriteLine("- Tag orientation issues (try different angles)");
        Console.WriteLine("- RF interference in HF band (13.56 MHz)");
        Console.WriteLine("- Integrated antenna coupling problems");
        Console.WriteLine("- Module positioning issues");
        Console.WriteLine("- ASCII protocol communication errors");
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

    /// <summary>
    /// Demonstrates reading and writing user data to HF tag memory using the QR15 module.
    /// Shows how to access ISO15693 tag memory blocks with integrated antenna optimization.
    /// </summary>
    public static void ReadWriteExample()
    {
      // Create the QR15 reader instance
      // Note: Update "/dev/ttyUSB0" to match your actual device path (Linux/Mac) or "COM#" for Windows
      String port = "/dev/ttyUSB0";
      QR15 reader = new QR15(port);
      
      // Subscribe to reader connection status changes
      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} Reader status changed to {e.Message} ({e.Status})");
      
      // Establish connection to the reader with 2-second timeout
      try
      {
        Console.WriteLine("Connecting to QR15 module for read/write operations...");
        reader.Connect(2000);
        Console.WriteLine("Connection established!");
        Console.WriteLine($"Using ASCII protocol with firmware: {reader.FirmwareVersion}");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- Check connection (Serial: COM port, Ethernet: IP address)");
        Console.WriteLine("- Verify QR15 module is properly powered");
        Console.WriteLine("- Ensure integrated antenna is functioning");
        Console.WriteLine("- Check for communication conflicts");
        return;
      }
      
      try
      {
        // Wait for an HF tag to be placed near the integrated antenna
        Console.WriteLine("Please place an HF/ISO15693 tag near the QR15 integrated antenna...");
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
              Console.WriteLine($"No tags found after {attempts} attempts. Continuing to search...");
              Console.WriteLine("Make sure you have an HF/ISO15693 compatible tag");
              Console.WriteLine("Try placing the tag directly over the integrated antenna");
              Console.WriteLine("QR15 integrated antenna provides consistent read zone");
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
        HfTag tag = tags[0];
        Console.WriteLine($"HF tag found: {tag.TID}");
        
        // Attempt to read data from block 4 (user memory area)
        // ISO15693 tags typically have user memory starting from block 4
        Console.WriteLine("\nReading user data from block 4...");
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
          Console.WriteLine($"Read operation failed: {ex.Message}");
          Console.WriteLine("Note: Some ISO15693 tags may have different memory layouts");
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