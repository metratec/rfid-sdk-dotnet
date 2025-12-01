using System;
using System.Collections.Generic;
using MetraTecDevices;

namespace ReaderExamples
{
  /// <summary>
  /// Examples demonstrating Dwarf15 reader operations for High Frequency (HF) RFID tags.
  /// This SMD module is designed for integration into custom electronics and supports ISO15693 tags.
  /// Uses ASCII protocol over serial communication for embedded applications and IoT devices.
  /// </summary>
  internal class Dwarf15Examples
  {
    /// <summary>
    /// Demonstrates basic HF inventory operations using the Dwarf15 SMD module.
    /// Shows how to detect ISO15693 tags and handle inventory events with proper error handling.
    /// </summary>
    public static void InventoryExample()
    {
      // Create the Dwarf15 SMD module instance using serial communication
      // Note: Update "/dev/ttyUSB0" to match your actual device path (Linux/Mac) or "COM#" for Windows
      String port = "/dev/ttyUSB0";
      Dwarf15 reader = new Dwarf15(port);

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
        Console.WriteLine("Connecting to Dwarf15 SMD module...");
        reader.Connect(2000);
        Console.WriteLine("Connection established!");
        Console.WriteLine($"Reader Firmware: {reader.FirmwareVersion}");
        Console.WriteLine($"Protocol: ASCII (SMD Module)");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- Check USB to serial adapter connection");
        Console.WriteLine($"- Verify COM port (currently set to {port})");
        Console.WriteLine("- Ensure Dwarf15 module is properly powered");
        Console.WriteLine("- Check if another application is using the COM port");
        Console.WriteLine("- Verify serial cable wiring (TX/RX, GND)");
        Console.WriteLine("- Check power supply voltage (3.3V or 5V depending on variant)");
        return;
      }

      try
      {
        // Set reader transmission power (100 or 200 mW)
        try
        {
          reader.SetPower(100);
          Console.WriteLine("Reader power set to 100");
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"Power setting not supported: {ex.Message}");
          Console.WriteLine("Continuing with default power settings...");
        }

        // Perform a single inventory scan to detect currently present tags
        Console.WriteLine("\nPerforming single inventory scan...");
        List<HfTag> tags = reader.GetInventory();
        Console.WriteLine($"Current inventory: {tags.Count} HF Tag(s) found");

        foreach (HfTag tag in tags)
        {
          Console.WriteLine($"  TID: {tag.TID}");
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
        Console.WriteLine("- Module antenna coupling problems");
        Console.WriteLine("- Power supply instability in embedded system");
        Console.WriteLine("- Serial communication issues");
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
    /// Demonstrates reading and writing user data to HF tag memory using the Dwarf15 SMD module.
    /// Shows how to access ISO15693 tag memory blocks for custom data storage in embedded applications.
    /// </summary>
    public static void ReadWriteExample()
    {
      // Create the Dwarf15 SMD module instance using serial communication
      // Note: Update "/dev/ttyUSB0" to match your actual device path (Linux/Mac) or "COM#" for Windows
      String port = "/dev/ttyUSB0";
      Dwarf15 reader = new Dwarf15(port);

      // Subscribe to reader connection status changes
      reader.StatusChanged += (s, e) => Console.WriteLine($"Reader status changed to {e.Message} ({e.Status})");

      // Establish connection to the reader with 2-second timeout
      try
      {
        Console.WriteLine("Connecting to Dwarf15 SMD module for read/write operations...");
        reader.Connect(2000);
        Console.WriteLine("Connection established!");
        Console.WriteLine($"Using ASCII protocol with firmware: {reader.FirmwareVersion}");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- Check USB to serial adapter connection");
        Console.WriteLine($"- Verify COM port (currently set to {port})");
        Console.WriteLine("- Ensure Dwarf15 module is properly powered");
        Console.WriteLine("- Check serial communication settings (baud rate, parity)");
        Console.WriteLine("- Verify this is a Dwarf15 (HF) module, not DwarfG2 (UHF)");
        return;
      }

      try
      {
        // Wait for an HF tag to be placed near the reader
        Console.WriteLine("Please place an HF/ISO15693 tag near the Dwarf15 module...");
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
              Console.WriteLine("Try placing the tag closer to the module antenna");
              Console.WriteLine("SMD modules typically have shorter range than desktop readers");
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
          Console.WriteLine("\nReading data from memory block 0...");
          try
          {
            String response = reader.ReadBlock(0, tag.TID); // Read 1 block
            Console.WriteLine($"Read data from block 0: {response}");
          }
          catch (MetratecReaderException ex)
          {
            Console.WriteLine($"Read operation failed: {ex.Message}");
            Console.WriteLine("Possible causes:");
            Console.WriteLine("- Block is protected or locked");
            Console.WriteLine("- Tag moved out of range during read");
            Console.WriteLine("- Unsupported memory layout");
          }
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"Read operation failed: {ex.Message}");
          Console.WriteLine("Note: Some ISO15693 tags may have different memory layouts");
        }

        // Write data to tag memory block 1 (avoiding block 0 which may contain system data)
        string dataToWrite = "12345678"; // 4 bytes as hex string (8 hex characters)
        Console.WriteLine($"\nWriting data '{dataToWrite}' to memory block 1...");
        try
        {
          reader.WriteBlock(1, dataToWrite, tag.TID);
          Console.WriteLine("Data written successfully to block 1!");
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"Write operation failed: {ex.Message}");
          Console.WriteLine("Possible causes:");
          Console.WriteLine("- Block is write-protected");
          Console.WriteLine("- Tag moved out of range during write");
          Console.WriteLine("- Insufficient power for write operation");
          Console.WriteLine("- Tag memory is full or corrupted");

        }

        // Read back the written data for verification
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
        catch (MetratecReaderException ex)
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