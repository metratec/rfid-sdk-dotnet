using System;
using System.Collections.Generic;
using System.Threading;
using MetraTecDevices;
using Microsoft.Extensions.Logging;

namespace ReaderExamples
{
  /// <summary>
  /// Examples demonstrating DeskID_NFC reader operations for Near Field Communication (NFC) tags.
  /// This reader uses AT command protocol and supports multiple NFC standards including ISO14A (Mifare).
  /// Shows serial connection setup, tag detection, authentication, and memory access.
  /// </summary>
  internal class DeskidNFCExamples
  {
    /// <summary>
    /// Demonstrates basic NFC inventory operations including single scan and continuous scanning.
    /// Shows how to detect various NFC tag types and handle inventory events with proper error handling.
    /// </summary>
    public static void InventoryExample()
    {
      // Create the reader instance using serial communication
      // Note: Update "/dev/ttyUSB0" to match your actual device path (Linux/Mac) or "COM#" for Windows
      String port = "/dev/ttyACM0";
      DeskID_NFC reader = new DeskID_NFC(port);

      // Subscribe to reader connection status changes (Connected/Disconnected)
      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} Reader status changed to {e.Message} ({e.Status})");
      
      // Subscribe to inventory events - triggered when tags are detected during continuous scanning
      reader.NewInventory += (s, e) =>
      {
        Console.WriteLine($"{e.Timestamp} New inventory event! {e.Tags.Count} Tag(s) found");
        foreach (HfTag tag in e.Tags)
        {
          // Display both TID (Tag Identifier) and tag type for NFC tags
          Console.WriteLine($"  TID: {tag.TID}  Type: {tag.Type}");
        }
      };
      
      // Establish connection to the reader with 5-second timeout (NFC may need longer)
      try
      {
        Console.WriteLine($"Connecting to DeskID_NFC on port {port}");
        reader.Connect(5000);
        Console.WriteLine("Connection established!");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- Check USB cable connection");
        Console.WriteLine("- Verify device path (currently set to /dev/ttyACM0)");
        Console.WriteLine("- On Windows, use COM port (e.g., COM3)");
        Console.WriteLine("- Ensure DeskID_NFC driver is installed");
        Console.WriteLine("- Check if another application is using the device");
        Console.WriteLine("- Verify reader is powered on");
        return;
      }
      
      try
      {
        // Configure reader for NFC operation
        try
        {
          reader.SetMode(NfcReaderMode.AUTO);
          Console.WriteLine("Reader configured for Auto mode (ISO15 and ISO14A)");
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"Mode configuration failed: {ex.Message}");
          Console.WriteLine("Continuing with default NFC settings...");
        }

        // Perform a single inventory scan to detect currently present tags
        // This also triggers the NewInventory event if listeners are registered
        Console.WriteLine("\nPerforming single inventory scan...");
        List<HfTag> tags = reader.GetInventory();
        Console.WriteLine($"Current inventory: {tags.Count} NFC Tag(s) found");
        
        foreach (HfTag tag in tags)
        {
          Console.WriteLine($"  TID: {tag.TID} Type: {tag.Type}");
        }

        // Demonstrate tag type detection for more detailed information
        Console.WriteLine("Performing detailed tag type detection...");
        try
        {
          List<HfTag> detectedTags = reader.DetectTagTypes();
          Console.WriteLine($"Detected {detectedTags.Count} tag(s) with detailed type information:");
          
          foreach (HfTag tag in detectedTags)
          {
            Console.WriteLine($"  TID: {tag.TID}");
            Console.WriteLine($"  Detailed Type: {tag.Type}");
            Console.WriteLine();
          }
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"Tag type detection failed: {ex.Message}");
        }
        Console.WriteLine("Press any key to stop for starting the continuous inventory scan and press any key to stop.");
        Console.ReadKey();
        // Start continuous inventory scanning in the background
        Console.WriteLine("Starting continuous inventory scan...");
        reader.StartInventory();
        Console.ReadKey();
        Console.WriteLine("Continuous inventory scan stopped");
        
        // Stop the continuous scanning
        reader.StopInventory();
        Console.WriteLine("Continuous inventory stopped");
      }
      catch (MetratecReaderException ex)
      {
        Console.WriteLine($"Error during operation: {ex.Message}");
        Console.WriteLine("\nPossible causes:");
        Console.WriteLine("- No NFC tags in range (bring tag closer - NFC has very short range)");
        Console.WriteLine("- RF interference in 13.56 MHz band");
        Console.WriteLine("- Tag orientation issues (try different angles)");
        Console.WriteLine("- Unsupported tag type");
        Console.WriteLine("- Reader antenna problems");
      }
      finally
      {
        // Always disconnect to free resources and close communication interface
        if (reader.Connected)
        {
          reader.Disconnect();
          Console.WriteLine("Connection closed");
        }
      }
    }

    /// <summary>
    /// Demonstrates advanced NFC operations including Mifare Classic authentication and data access.
    /// Shows how to read and write data blocks with proper authentication using keys and comprehensive error handling.
    /// </summary>
    public static void ReadWriteMifareData()
    {
      // Create the reader instance using serial communication
      // Note: Update "/dev/ttyUSB0" to match your actual device path (Linux/Mac) or "COM#" for Windows
      String port = "/dev/ttyACM0";
      DeskID_NFC reader = new DeskID_NFC(port);
      
      // Subscribe to reader connection status changes
      reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} Reader status changed to {e.Message} ({e.Status})");
      
      // Establish connection to the reader with 2-second timeout
      try
      {
        Console.WriteLine($"Connecting to DeskID_NFC on port {port} for Mifare operations...");
        reader.Connect(2000);
        Console.WriteLine("Connection established!");
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
        Console.WriteLine("\nTroubleshooting:");
        Console.WriteLine("- Check USB cable connection");
        Console.WriteLine("- Verify device path (currently set to /dev/ttyACM0)");
        Console.WriteLine("- On Windows, use COM port (e.g., COM3)");
        Console.WriteLine("- Ensure DeskID_NFC driver is installed");
        Console.WriteLine("- Check device permissions (Linux/Mac)");
        return;
      }
      
      try
      {
        // Set the reader to ISO14A mode for Mifare compatibility
        // ISO14A supports Mifare Classic, Mifare Plus, and other 14443-A tags
        reader.SetMode(NfcReaderMode.ISO14A);
        Console.WriteLine("Reader configured for ISO14A (Mifare) mode");
        
        // Wait for a Mifare Classic tag to be detected
        Console.WriteLine("Please place a Mifare Classic tag on the NFC reader...");
        Console.WriteLine("Looking for Mifare Classic (MFC) tags specifically...");
        
        ISO14ATag? tag = null;
        int attempts = 0;
        const int maxAttempts = 30;
        
        while (tag == null && attempts < maxAttempts)
        {
          try
          {
            // Use DetectTagTypes() for detailed tag type information
            List<HfTag> tags = reader.DetectTagTypes();
            foreach (HfTag item in tags)
            {
              Console.WriteLine($"Tag detected: {item.TID} (Type: {item.Type})");
              // Look specifically for Mifare Classic (MFC) tags
              if (item.Type.Contains("MFC"))
              {
                tag = (ISO14ATag)item;
                Console.WriteLine("Mifare Classic tag found!");
                break;
              }
              else
              {
                Console.WriteLine($"Found {item.Type} tag, but need Mifare Classic for this example");
              }
            }
            
            if (tag == null)
            {
              attempts++;
              if (attempts % 5 == 0)
              {
                Console.WriteLine($"No Mifare Classic tags found after {attempts} attempts...");
                Console.WriteLine("Make sure you have a Mifare Classic tag");
                Console.WriteLine("Mifare Ultralight and other types won't work for this example");
              }
              System.Threading.Thread.Sleep(1000);
            }
          }
          catch (MetratecReaderException ex)
          {
            Console.WriteLine($"Tag detection error: {ex.Message}");
            attempts++;
            System.Threading.Thread.Sleep(1000);
          }
        }
        
        if (tag == null)
        {
          Console.WriteLine("No Mifare Classic tag found after 30 seconds.");
          Console.WriteLine("Please ensure you have a Mifare Classic tag and try again.");
          return;
        }

        Console.WriteLine($"\n=== Working with Mifare Classic tag: {tag.TID} ===");

        // Perform Mifare Classic operations with proper error handling
        try
        {
          // Select the specific tag for subsequent operations
          Console.WriteLine("Selecting tag for operations...");
          reader.SelectTag(tag.TID);
          Console.WriteLine("Tag selected successfully");
          
          // Block 5 is typically in sector 1, which uses the default key on new cards
          int blockToAccess = 5;
          string defaultKey = "FFFFFFFFFFFF"; // Default factory key (all 0xFF)
          
          Console.WriteLine($"Authenticating block {blockToAccess} with default key...");
          // Authenticate block 5 using the default factory key (all 0xFF)
          // Note: Production tags should use custom keys for security
          reader.AuthenticateMifareClassicBlock(blockToAccess, defaultKey, KeyType.A);
          Console.WriteLine("Authentication successful!");
          
          // Read data from block 5 (16 bytes for Mifare Classic)
          Console.WriteLine($"Reading data from block {blockToAccess}...");
          string data = reader.ReadBlock(blockToAccess);
          Console.WriteLine($"Current data in block {blockToAccess}: {data}");
          
          // Write new data to block 5 (32 hex characters = 16 bytes)
          // Format: each byte as 2 hex digits (01020304... = bytes 0x01, 0x02, 0x03, 0x04...)
          string dataToWrite = "01020304050607080910111213141516";
          Console.WriteLine($"Writing new data '{dataToWrite}' to block {blockToAccess}...");
          reader.WriteBlock(blockToAccess, dataToWrite);
          Console.WriteLine("Write operation completed!");
          
          // Verify the write operation by reading the data back
          Console.WriteLine($"Verifying written data...");
          string newData = reader.ReadBlock(blockToAccess);
          Console.WriteLine($"Verification read from block {blockToAccess}: {newData}");
          
          if (newData?.ToUpper() == dataToWrite.ToUpper())
          {
            Console.WriteLine(" Data verification successful!");
          }
          else
          {
            Console.WriteLine(" Data mismatch - write may have failed");
          }

          // Demonstrate reading multiple blocks (if authentication allows)
          Console.WriteLine($"Reading multiple blocks in the same sector...");
          for (int block = 4; block <= 6; block++) // Blocks 4, 5, 6 are in sector 1
          {
            try
            {
              string blockData = reader.ReadBlock(block);
              Console.WriteLine($"  Block {block}: {blockData}");
            }
            catch (TransponderException ex)
            {
              Console.WriteLine($"  Block {block}: Error - {ex.Message}");
            }
          }
        }
        catch (TransponderException e)
        {
          Console.WriteLine($"Transponder Error: {e.Message}");
          Console.WriteLine(" Possible causes:");
          Console.WriteLine(" - Wrong authentication key (try different key)");
          Console.WriteLine(" - Block is read-only or protected");
          Console.WriteLine(" - Tag removed during operation");
          Console.WriteLine(" - Authentication timeout");
          Console.WriteLine(" - Sector access bits prevent operation");
        }
        catch (MetratecReaderException ex)
        {
          Console.WriteLine($"Reader Error: {ex.Message}");
          Console.WriteLine(" Possible causes:");
          Console.WriteLine(" - Communication timeout");
          Console.WriteLine(" - Unsupported operation for this tag type");
          Console.WriteLine(" - Hardware communication failure");
        }
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"General reader error: {e.Message}");
      }
      finally
      {
        // Always disconnect in finally block to ensure cleanup even if exceptions occur
        if (reader.Connected)
        {
          reader.Disconnect();
          Console.WriteLine("Connection closed");
        }
      }
    }
  }
}