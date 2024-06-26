﻿using System;
using System.Collections.Generic;
using MetraTecDevices;

namespace ReaderExamples
{
  internal class DeskidIsoExamples
  {
    public static void InventoryExample()
    {
      // Create the reader instance
      DeskID_ISO reader = new DeskID_ISO("COM7");

      // add a reader status listener
      reader.StatusChanged += (s, e) => Console.WriteLine($"Reader status changed to {e.Message} ({e.Status})");
      // add an inventory listener
      reader.NewInventory += (s, e) =>
      {
        Console.WriteLine($"New inventory event! {e.Tags.Count} Tag(s) found");
        foreach (HfTag tag in e.Tags)
        {
          Console.WriteLine($" {tag.TID}");
        }
      };
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
      // fetches the current inventory - if an inventory listener exists, this method also triggers the listener
      List<HfTag> tags = reader.GetInventory();
      Console.WriteLine($"Current inventory: {tags.Count} Tag(s) found");
      foreach (HfTag tag in tags)
      {
        Console.WriteLine($" {tag.TID}");
      }

      reader.StartInventory();
      Console.WriteLine("Continuous inventory scan started - Press any key to stop");
      Console.ReadKey();
      reader.StopInventory();
      // Disconnect reader
      reader.Disconnect();
    }

    public static void ReadWriteExample()
    {
      // Create the reader instance
      DeskID_ISO reader = new DeskID_ISO("COM7");
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
      // fetches the current inventory - if an inventory listener exists, this method also triggers the listener
      List<HfTag> tags = reader.GetInventory();
      while (tags.Count == 0)
      {
        Console.WriteLine("Please put a tag on the reader and press enter...");
        Console.ReadLine();
        tags = reader.GetInventory();
      }
      HfTag tag = tags[0];
      Console.WriteLine("Try to read tag bock 0...");
      try
      {
        string resp = reader.ReadBlock(0, tag.TID);
        Console.WriteLine($"Transponder Block 0: {resp}");
      }
      catch (TransponderException e)
      {
        Console.WriteLine($"Can not read the transponder block 0 {e.Message}");
      }

      Console.WriteLine("Try to write tag bock 0...");
      try
      {
        reader.WriteBlock(0, "01020304", tag.TID);
        Console.WriteLine("Transponder Block 0 written");
      }
      catch (TransponderException e)
      {
        Console.WriteLine($"Can not write the transponder block 0 {e.Message}");
      }
      reader.Disconnect();
    }

  }
}
