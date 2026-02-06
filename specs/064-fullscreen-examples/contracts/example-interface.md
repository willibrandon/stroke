# Contract: Example Interface

**Feature**: 064-fullscreen-examples
**Date**: 2026-02-05

## Overview

All full-screen examples follow a consistent interface pattern for discoverability and maintainability.

## Example Class Contract

Each example is implemented as a static class with a `Run()` method:

```csharp
namespace Stroke.Examples.FullScreenExamples;

/// <summary>
/// [Brief description of what the example demonstrates]
/// Port of Python Prompt Toolkit's [filename].py example.
/// </summary>
internal static class ExampleName
{
    /// <summary>
    /// Runs the example application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// [Key interaction instructions]
    /// </para>
    /// <para>
    /// Press Ctrl+C to exit.
    /// </para>
    /// </remarks>
    public static void Run()
    {
        try
        {
            // Create layout, key bindings, and application
            // Run the application
        }
        catch (KeyboardInterrupt)
        {
            // Ctrl+C pressed - exit gracefully without message
        }
        catch (EOFException)
        {
            // Ctrl+D pressed - exit gracefully without message
        }
    }
}
```

## Subdirectory Examples

Examples in subdirectories use nested namespaces:

```csharp
namespace Stroke.Examples.FullScreenExamples.SimpleDemos;

internal static class HorizontalSplit
{
    public static void Run() { /* ... */ }
}
```

```csharp
namespace Stroke.Examples.FullScreenExamples.ScrollablePanes;

internal static class SimpleExample
{
    public static void Run() { /* ... */ }
}
```

## Program.cs Router Contract

The entry point routes to examples by name:

```csharp
namespace Stroke.Examples.FullScreenExamples;

internal static class Program
{
    /// <summary>
    /// Dictionary mapping example names to their run functions.
    /// Names are case-insensitive.
    /// </summary>
    private static readonly Dictionary<string, Action> Examples =
        new(StringComparer.OrdinalIgnoreCase)
    {
        // Main examples
        ["HelloWorld"] = HelloWorld.Run,
        ["DummyApp"] = DummyApp.Run,
        ["NoLayout"] = NoLayout.Run,
        ["Buttons"] = Buttons.Run,
        ["Calculator"] = Calculator.Run,
        ["SplitScreen"] = SplitScreen.Run,
        ["Pager"] = Pager.Run,
        ["FullScreenDemo"] = FullScreenDemo.Run,
        ["TextEditor"] = TextEditor.Run,
        ["AnsiArtAndTextArea"] = AnsiArtAndTextArea.Run,

        // ScrollablePanes
        ["SimpleExample"] = ScrollablePanes.SimpleExample.Run,
        ["WithCompletionMenu"] = ScrollablePanes.WithCompletionMenu.Run,

        // SimpleDemos
        ["HorizontalSplit"] = SimpleDemos.HorizontalSplit.Run,
        ["VerticalSplit"] = SimpleDemos.VerticalSplit.Run,
        ["Alignment"] = SimpleDemos.Alignment.Run,
        ["HorizontalAlign"] = SimpleDemos.HorizontalAlign.Run,
        ["VerticalAlign"] = SimpleDemos.VerticalAlign.Run,
        ["Floats"] = SimpleDemos.Floats.Run,
        ["FloatTransparency"] = SimpleDemos.FloatTransparency.Run,
        ["Focus"] = SimpleDemos.Focus.Run,
        ["Margins"] = SimpleDemos.Margins.Run,
        ["LinePrefixes"] = SimpleDemos.LinePrefixes.Run,
        ["ColorColumn"] = SimpleDemos.ColorColumn.Run,
        ["CursorHighlight"] = SimpleDemos.CursorHighlight.Run,
        ["AutoCompletion"] = SimpleDemos.AutoCompletion.Run,
    };

    public static void Main(string[] args)
    {
        var exampleName = args.Length > 0 ? args[0] : null;

        if (exampleName == "--help" || exampleName == "-h")
        {
            ShowUsage();
            return;
        }

        if (exampleName is null)
        {
            Console.Error.WriteLine("Error: No example name provided.");
            Console.Error.WriteLine();
            ShowUsage();
            Environment.Exit(1);
            return;
        }

        if (Examples.TryGetValue(exampleName, out var runAction))
        {
            runAction();
        }
        else
        {
            Console.Error.WriteLine($"Unknown example: '{exampleName}'");
            Console.Error.WriteLine();
            ShowUsage();
            Environment.Exit(1);
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine("Stroke Full-Screen Examples");
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet run -- <example-name>");
        Console.WriteLine();
        Console.WriteLine("Available examples:");

        Console.WriteLine();
        Console.WriteLine("  Main:");
        foreach (var name in new[] { "HelloWorld", "DummyApp", "NoLayout", "Buttons",
            "Calculator", "SplitScreen", "Pager", "FullScreenDemo", "TextEditor",
            "AnsiArtAndTextArea" })
        {
            Console.WriteLine($"    {name}");
        }

        Console.WriteLine();
        Console.WriteLine("  ScrollablePanes:");
        foreach (var name in new[] { "SimpleExample", "WithCompletionMenu" })
        {
            Console.WriteLine($"    {name}");
        }

        Console.WriteLine();
        Console.WriteLine("  SimpleDemos:");
        foreach (var name in new[] { "HorizontalSplit", "VerticalSplit", "Alignment",
            "HorizontalAlign", "VerticalAlign", "Floats", "FloatTransparency", "Focus",
            "Margins", "LinePrefixes", "ColorColumn", "CursorHighlight", "AutoCompletion" })
        {
            Console.WriteLine($"    {name}");
        }
    }
}
```

## Project File Contract

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <LangVersion>13</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>Stroke.Examples.FullScreenExamples</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Stroke\Stroke.csproj" />
  </ItemGroup>

</Project>
```

## Common Using Directives

Most examples will use these namespaces:

```csharp
using Stroke.Application;
using Stroke.Core;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Styles;
using Stroke.Widgets.Base;
```

Additional namespaces as needed:

```csharp
// For list widgets
using Stroke.Widgets.Lists;

// For dialogs
using Stroke.Widgets.Dialogs;

// For toolbars
using Stroke.Widgets.Toolbars;

// For menus
using Stroke.Layout.Menus;

// For completion
using Stroke.Completion;

// For lexers
using Stroke.Lexers;

// For shortcut dialogs
using Stroke.Shortcuts;
```

## Error Handling Contract

All examples must handle interrupts gracefully:

```csharp
public static void Run()
{
    try
    {
        // ... application code ...
    }
    catch (KeyboardInterrupt)
    {
        // Ctrl+C - silent exit
    }
    catch (EOFException)
    {
        // Ctrl+D - silent exit
    }
}
```

No stack traces should be displayed on normal exit paths.
