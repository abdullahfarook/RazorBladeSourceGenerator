# RazorBlade Incremental Source Generator Example

This is a minimal example demonstrating how to use **RazorBlade** with an **incremental source generator** in .NET.

## What This Does

- Uses an incremental source generator to find classes marked with `[GenerateCode]`
- Uses RazorBlade templates (`.cshtml`) to generate C# code at build time
- Generates helper methods (`ToString()` and `PrintProperties()`) for marked classes

## Project Structure

```
RazorBladeGenerator/
├── Generator.cs              # Incremental source generator
├── Attributes.cs             # GenerateCode attribute
├── Templates/
│   └── ClassTemplate.cshtml # RazorBlade template
└── RazorBladeGenerator.csproj

SampleApp/
├── Program.cs                # Sample usage
└── SampleApp.csproj
```

## How It Works

1. **Incremental Generator** (`Generator.cs`):
   - Scans for classes with `[GenerateCode]` attribute
   - Extracts class name, namespace, and properties
   - Uses RazorBlade template to generate code

2. **RazorBlade Template** (`ClassTemplate.cshtml`):
   - Uses Razor syntax to generate C# code
   - Compiles at build time (no runtime dependencies)
   - Generates helper methods

3. **Sample App**:
   - Marks `Person` class with `[GenerateCode]`
   - Uses generated methods at runtime

## Building

```bash
dotnet build
```

## Running

```bash
cd SampleApp
dotnet run
```

## Key Features

- ✅ **Build-time compilation** - Templates compile to C# classes during build
- ✅ **No runtime dependencies** - RazorBlade compiles templates at build time
- ✅ **Incremental** - Only regenerates when source changes
- ✅ **Type-safe** - Compile-time validation
- ✅ **Razor syntax** - Familiar templating syntax

## Generated Code Example

For a class like:
```csharp
[GenerateCode]
public class Person
{
    public string FirstName { get; set; }
    public int Age { get; set; }
}
```

The generator creates:
```csharp
public partial class Person
{
    public string ToString() { ... }
    public void PrintProperties() { ... }
}
```

## Notes

- RazorBlade templates are compiled at build time (not runtime)
- Generated files appear in `obj/Debug/net8.0/generated/` folder
- The generator only processes classes with `[GenerateCode]` attribute

