using System;
using System.Collections.Generic;
using System.Threading;
using MetraTecDevices;
using Microsoft.Extensions.Logging;

namespace ReaderExamples
{
  internal class DeskidNFCExamples
  {
    public static void InventoryExample()
    {
      // Create the reader instance
      DeskID_NFC reader = new DeskID_NFC("/dev/ttyACM0");

      // add a reader status listener
      reader.StatusChanged += (s, e) => Console.WriteLine($"Reader status changed to {e.Message} ({e.Status})");
      // add an inventory listener
      reader.NewInventory += (s, e) =>
      {
        Console.WriteLine($"New inventory event! {e.Tags.Count} Tag(s) found");
        foreach (HfTag tag in e.Tags)
        {
          Console.WriteLine($" {tag.TID} {tag.Type}");
        }
      };
      // connect the reader with timeout
      try
      {
        reader.Connect(5000);
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Can not connect to reader ({e.Message}). Program exits");
        return;
      }
      // fetches the current inventory - if an inventory listener exists, this method also triggers the listener
      List<HfTag> tags = reader.GetInventory();
      Console.WriteLine($"Current inventory: {tags.Count} Tag(s) found");
      foreach (HfTag tag in tags)
      {
        Console.WriteLine($" {tag.TID} {tag.Type}");
      }

      reader.StartInventory();
      Console.WriteLine("Continuous inventory scan started - Press any key to stop");
      Console.ReadKey();
      // Thread.Sleep(5000);
      reader.StopInventory();
      // Disconnect reader
      reader.Disconnect();
    }

    public static void ReadWriteMifareData()
    {
      // Create the reader instance
      DeskID_NFC reader = new DeskID_NFC("/dev/ttyACM0");
      // add a reader status listener
      reader.StatusChanged += (s, e) => Console.WriteLine($"Reader status changed to {e.Message} ({e.Status})");
      // connect the reader with timeout
      try
      {
        reader.Connect(2000);
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Can not connect to reader ({e.Message}). Program exits");
        return;
      }
      try
      {


        reader.SetMode(NfcReaderMode.ISO14A);
        // fetches the current inventory - if an inventory listener exists, this method also triggers the listener
        ISO14ATag? tag = null;
        while (true)
        {
          List<HfTag> tags = reader.DetectTagTypes();
          foreach (HfTag item in tags)
          {
            Console.WriteLine($" Tag found: {item.TID} ({item.Type})");
            if (item.Type.Contains("MFC"))
            {
              tag = (ISO14ATag)item;
              break;
            }
          }
          if (null == tag)
          {
            Console.WriteLine("Please put a mifare tag on the reader and press enter...");
            Console.ReadLine();
          }
          else
          {
            break;
          }
        }

        // Tag operations
        try
        {
          // Select tag
          reader.SelectTag(tag.TID);
          // Authenticate Block 5
          reader.AuthenticateMifareClassicBlock(5, "FFFFFFFFFFFF", KeyType.A);
          // Try to read tag bock 5
          string data = reader.ReadBlock(5);
          Console.WriteLine($"Block 5 data: {data}");
          // Try to write tag bock 5 with 16 Byte - 32 Hex;
          reader.WriteBlock(5, "01020304050607080910111213141516");
          // Reread Data:
          Console.WriteLine($"Block 5 data new: {reader.ReadBlock(5)}");
        }
        catch (TransponderException e)
        {
          Console.WriteLine($"Transponder Error: {e.Message}");
        }
      }
      catch (MetratecReaderException e)
      {
        Console.WriteLine($"Reader Error: {e.Message}");
      }
      finally
      {
        reader.Disconnect();
      }
    }

  }
}
