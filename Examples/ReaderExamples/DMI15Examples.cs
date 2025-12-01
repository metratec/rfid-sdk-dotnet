using System;
using System.Collections.Generic;
using CommunicationInterfaces;
using MetraTecDevices;

namespace ReaderExamples
{
    /// <summary>
    /// Examples demonstrating DMI15 reader operations for High Frequency (HF) ISO15693 RFID tags.
    /// The DMI15 is an IoT-focused HF reader with integrated antenna and Power over Ethernet (PoE) capability.
    /// Uses ASCII command protocol over Ethernet communication.
    /// </summary>
    internal class DMI15Examples
    {
        /// <summary>
        /// Demonstrates basic HF inventory operations using the DMI15 reader.
        /// Shows network connectivity, tag detection, and continuous scanning for ISO15693 tags.
        /// </summary>
        public static void InventoryExample()
        {
            // Create the reader instance using Ethernet communication
            // Note: Update IP address and port to match your DMI15 network configuration
            // DMI15 typically uses PoE and standard port 10001
            DMI15 reader = new DMI15("192.168.2.239", 10001);

            // Subscribe to reader connection status changes (Connected/Disconnected)
            reader.StatusChanged += (s, e) => Console.WriteLine($"{e.Timestamp} Reader status changed to {e.Message} ({e.Status})");

            // Subscribe to inventory events - triggered when HF tags are detected during continuous scanning
            reader.NewInventory += (s, e) =>
            {
                Console.WriteLine($"{e.Timestamp} New inventory event! {e.Tags.Count} HF Tag(s) found");
                foreach (HfTag tag in e.Tags)
                {
                    Console.WriteLine($"  TID: {tag.TID}");
                }
            };

            // Subscribe to GPIO input changes - useful for trigger inputs or sensor monitoring
            reader.InputChanged += (s, e) => Console.WriteLine($"Input Changed: {e.Pin} {e.IsHigh}");

            // Establish network connection to the reader with 5-second timeout
            try
            {
                Console.WriteLine("Connecting to DMI15...");
                reader.Connect(5000);
                Console.WriteLine("Connection established!");
            }
            catch (MetratecReaderException e)
            {
                Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
                Console.WriteLine("\nTroubleshooting:");
                Console.WriteLine("- Check network cable connection");
                Console.WriteLine("- Verify DMI15 IP address configuration");
                Console.WriteLine("- Ensure PoE power supply is connected");
                Console.WriteLine("- Check firewall settings on port 10001");
                return;
            }

            try
            {
                // Set reader transmission power (100 or 200 mW)
                // DMI15 typically operates at optimal power for integrated antenna
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

                // Enable RF interface with optimal settings for ISO15693
                reader.EnableRfInterface(SubCarrier.SINGLE, ModulationDepth.Depth100);
                Console.WriteLine("RF interface enabled for ISO15693 operation");

                // Perform a single inventory scan to detect currently present HF tags
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
                Console.WriteLine("- No HF/ISO15693 tags in range");
                Console.WriteLine("- RF interference in 13.56 MHz band");
                Console.WriteLine("- Tag orientation or distance issues");
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
        /// Demonstrates reading and writing data to HF ISO15693 tag memory using the DMI15 reader.
        /// Shows how to access different memory blocks of ISO15693 tags.
        /// </summary>
        public static void ReadWriteExample()
        {
            // Create the reader instance using Ethernet communication
            DMI15 reader = new DMI15("192.168.1.100", 10001);

            // Subscribe to reader connection status changes
            reader.StatusChanged += (s, e) => Console.WriteLine($"Reader status changed to {e.Message} ({e.Status})");

            // Establish network connection with 5-second timeout
            try
            {
                Console.WriteLine("Connecting to DMI15 for read/write operations...");
                reader.Connect(5000);
                Console.WriteLine("Connection established!");
            }
            catch (MetratecReaderException e)
            {
                Console.WriteLine($"Cannot connect to reader ({e.Message}). Program exits");
                return;
            }

            try
            {
                // Configure reader for optimal HF operation
                reader.EnableRfInterface(SubCarrier.SINGLE, ModulationDepth.Depth100);

                // Wait for an HF tag to be placed within reader range
                Console.WriteLine("Please place an ISO15693 tag near the DMI15 reader...");
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
                            Console.WriteLine("Make sure you have an ISO15693 compatible tag");
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

                // Read data from tag memory block 0
                // ISO15693 tags typically have multiple 4-byte blocks
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