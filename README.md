# Metratec Device SDK

Metratec Devices SDK for .net 6.0 (core)

## Install the library

This library also requires for the serial connection the System.IO.Ports package.

To add the required libraries, you can do this via the Visual Studio UI: Right-click on `Dependencies->Add Project Reference->Browse` and select the external `MetratecDevices.dll`.
For the serial connection add the required package `System.IO.Ports` via the Visual Studio UI. Right-click on `Dependencies->Manage NuGet Packages` and locate and install the `System.IO.Ports` package.

This library uses the Microsoft Logging System, for which at least the `Microsoft.Extensions.Logging.Abstraction` package is necessary. Add this package as well.

Or you can alternatively edit your `.csproj` file:

```xml
<ItemGroup>
  <Reference Include="MetratecDevices, Version=3.3.0.0">
    <HintPath>path\to\MetratecDevices.dll</HintPath>
  </Reference>
  <!-- For serial connection -->
  <PackageReference Include="System.IO.Ports" Version="7.0.0" />
  <!-- For logging -->
  <PackageReference Include="Microsoft.Extensions.Logging.Abstraction" Version="7.0.0" />
</ItemGroup>
```

## Usage

```cs
using MetraTecDevices;

namespace Tests
{
  class Program
  {
    static void Main(string[] args)
    {
      try
      {
        // Create a DeskID uhf device object
        DeskID_UHF_v2 deskid = new DeskID_UHF_v2("COM6");
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
      catch (MetratecReaderException e)
      {
        Console.WriteLine(e.ToString());
      }
    }
  }
}
```

## License

MIT License
