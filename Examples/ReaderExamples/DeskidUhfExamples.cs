using MetraTecDevices;

namespace ReaderExamples
{
  internal class DeskidUhfExamples
  {
    public static void InventoryExample()
    {
      // Create the reader instance
      DeskID_UHF reader = new DeskID_UHF("COM8");

      // add a reader status listener
      reader.StatusChanged += (s, e) => Console.WriteLine($"Reader status changed to {e.Message} ({e.Status})");
      // add an inventory listener
      reader.NewInventory += (s, e) =>
      {
        Console.WriteLine($"New inventory event! {e.Tags.Count} Tag(s) found");
        foreach (UhfTag tag in e.Tags)
        {
          Console.WriteLine($" {tag.EPC}");
        }
      };
      // connect the reader with timeout
      try
      {
        reader.Connect(2000);
      }
      catch (TimeoutException)
      {
        Console.WriteLine($"Can not connect to reader. Program exits");
        return;
      }
      // set reader power
      reader.SetPower(5);
      // fetches the current inventory - if an inventory listener exists, this method also triggers the listener
      List<UhfTag> tags = reader.GetInventory();
      Console.WriteLine($"Current inventory: {tags.Count} Tag(s) found");
      foreach (UhfTag tag in tags)
      {
        Console.WriteLine($" {tag.EPC}");
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
      DeskID_UHF reader = new DeskID_UHF("COM8");
      // add a reader status listener
      reader.StatusChanged += (s, e) => Console.WriteLine($"Reader status changed to {e.Message} ({e.Status})");
      // connect the reader with timeout
      try
      {
        reader.Connect(2000);
      }
      catch (TimeoutException)
      {
        Console.WriteLine($"Can not connect to reader. Program exits");
        return;
      }
      // set reader power
      reader.SetPower(5);
      // fetches the current inventory - if an inventory listener exists, this method also triggers the listener
      List<UhfTag> tags = reader.GetInventory();
      while (tags.Count == 0)
      {
        Console.WriteLine("Please put a tag on the reader and press enter...");
        Console.ReadLine();
        tags = reader.GetInventory();
      }
      Console.WriteLine("Try to read address 0...");
      List<UhfTag> resp = reader.ReadTagUsrData(0, 2);
      if (resp.Count == 0)
      {
        Console.WriteLine("No tag found during read tag user data");
      }
      else if (resp[0].HasError)
      {
        Console.WriteLine($"Can not read the transponder block 0 {resp[0].Message}");
      }
      else
      {
        Console.WriteLine($"Transponder Block 0: {resp[0].Data}");
      }
      Console.WriteLine("Try to write tag address 0...");
      resp = reader.WriteTagUsrData(0, "01020304");
      if (resp.Count == 0)
      {
        Console.WriteLine("No tag found during read tag user data");
      }
      else if (resp[0].HasError)
      {
        Console.WriteLine($"Can not write the transponder block 0 {resp[0].Message}");
      }
      else
      {
        Console.WriteLine($"Transponder Block 0 written: {resp[0].Data}");
      }
      reader.Disconnect();
    }

  }
}
