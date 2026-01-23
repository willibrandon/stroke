# Stroke Documentation Plan

## Overview

This document defines the documentation strategy for Stroke - a complete .NET 10 port of Python Prompt Toolkit. The approach follows .NET idioms using **DocFX + GitHub Pages** - no domain required.

## Hosting

| Aspect | Value |
|--------|-------|
| **Platform** | GitHub Pages |
| **URL** | `https://<username>.github.io/stroke` |
| **Cost** | Free |
| **Automation** | GitHub Actions |
| **Generator** | DocFX |

## Documentation Architecture

### 1. API Reference (Auto-Generated)

DocFX extracts API documentation from:
- Compiled assemblies (`Stroke.dll`)
- XML documentation files (from `///` comments)
- Links to Microsoft .NET docs via xrefmap

**Output:** Full namespace/class/method documentation with:
- Type signatures
- Parameter descriptions
- Return values
- Exceptions
- Code examples (from `<example>` blocks)

### 2. Conceptual Documentation (Human-Written)

Markdown articles mirroring Python Prompt Toolkit's documentation structure:

```
docfx/
├── docs/
│   ├── index.md                    # Documentation home
│   ├── toc.yml                     # Table of contents
│   │
│   ├── getting-started/
│   │   ├── index.md                # Overview & installation
│   │   ├── first-prompt.md         # Hello world example
│   │   └── learning-path.md        # Recommended progression
│   │
│   ├── tutorials/
│   │   ├── printing-text.md        # Formatted text & colors
│   │   ├── asking-for-input.md     # Prompts with features
│   │   ├── dialogs.md              # Dialog boxes
│   │   ├── progress-bars.md        # Progress indicators
│   │   ├── full-screen-apps.md     # Full-screen applications
│   │   └── building-a-repl.md      # Complete REPL tutorial
│   │
│   ├── advanced/
│   │   ├── key-bindings.md         # Key binding system
│   │   ├── styling.md              # Colors & themes
│   │   ├── filters.md              # Composable conditions
│   │   ├── layout-system.md        # Container hierarchy
│   │   ├── rendering-pipeline.md   # How rendering works
│   │   ├── architecture.md         # System architecture
│   │   └── unit-testing.md         # Testing Stroke apps
│   │
│   ├── editing-modes/
│   │   ├── emacs.md                # Emacs mode
│   │   └── vi.md                   # Vi mode
│   │
│   ├── migration/
│   │   └── from-ptk.md             # Python PTK → Stroke
│   │
│   └── reference/
│       └── api-comparison.md       # PTK ↔ Stroke API mapping
│
├── apispec/                        # API doc overrides
│   ├── namespace-core.md           # Stroke.Core namespace summary
│   ├── namespace-application.md    # Stroke.Application summary
│   ├── namespace-layout.md         # Stroke.Layout summary
│   └── [per-namespace overrides]
│
├── images/
│   ├── logo.png                    # Stroke logo
│   ├── favicon.png                 # Site favicon
│   └── screenshots/                # Feature screenshots
│       ├── completion.png
│       ├── dialogs/
│       ├── progress-bars/
│       └── [per-feature images]
│
└── docfx.json                      # DocFX configuration
```

## Content Mapping: Python PTK → Stroke

The documentation structure mirrors Python Prompt Toolkit exactly:

| Python PTK Doc | Stroke Equivalent |
|----------------|-------------------|
| `getting_started.rst` | `docs/getting-started/index.md` |
| `printing_text.rst` | `docs/tutorials/printing-text.md` |
| `asking_for_input.rst` | `docs/tutorials/asking-for-input.md` |
| `asking_for_a_choice.rst` | `docs/tutorials/dialogs.md` |
| `dialogs.rst` | `docs/tutorials/dialogs.md` |
| `progress_bars.rst` | `docs/tutorials/progress-bars.md` |
| `full_screen_apps.rst` | `docs/tutorials/full-screen-apps.md` |
| `tutorials/repl.rst` | `docs/tutorials/building-a-repl.md` |
| `advanced_topics/key_bindings.rst` | `docs/advanced/key-bindings.md` |
| `advanced_topics/styling.rst` | `docs/advanced/styling.md` |
| `advanced_topics/filters.rst` | `docs/advanced/filters.md` |
| `advanced_topics/rendering_flow.rst` | `docs/advanced/rendering-pipeline.md` |
| `advanced_topics/rendering_pipeline.rst` | `docs/advanced/rendering-pipeline.md` |
| `advanced_topics/asyncio.rst` | `docs/advanced/architecture.md` |
| `advanced_topics/unit_testing.rst` | `docs/advanced/unit-testing.md` |
| `advanced_topics/architecture.rst` | `docs/advanced/architecture.md` |
| `reference.rst` (autodoc) | Auto-generated `/api/` |
| `gallery.rst` | `docs/index.md` (showcase section) |

## DocFX Configuration

```json
{
  "metadata": [
    {
      "src": [
        {
          "src": "../src/Stroke",
          "files": ["bin/Release/net10.0/Stroke.dll"]
        }
      ],
      "dest": "api",
      "memberLayout": "separatePages"
    }
  ],
  "build": {
    "xref": [
      "https://learn.microsoft.com/en-us/dotnet/.xrefmap.json"
    ],
    "template": ["default", "_exported_templates/modern"],
    "output": "_site",
    "content": [
      {
        "files": ["**/*.{md,yml}"],
        "exclude": ["_site/**", "includes/**", "apispec/**"]
      }
    ],
    "resource": [
      {
        "files": ["images/**"]
      }
    ],
    "overwrite": "apispec/*.md",
    "globalMetadata": {
      "_appName": "Stroke",
      "_appTitle": "Stroke - Terminal UI for .NET",
      "pdf": false,
      "_appFaviconPath": "images/favicon.png",
      "_appLogoPath": "images/logo.png",
      "_appFooter": "Stroke - A .NET port of Python Prompt Toolkit",
      "_enableSearch": true,
      "_disableContribution": false,
      "_gitContribute": {
        "repo": "https://github.com/<username>/stroke",
        "branch": "main",
        "apiSpecFolder": "docfx/apispec"
      },
      "_gitUrlPattern": "github"
    },
    "markdownEngineName": "markdig",
    "postProcessors": ["ExtractSearchIndex"]
  }
}
```

## GitHub Actions Workflow

```yaml
name: Build and Publish Documentation

on:
  push:
    branches: [main]
  workflow_dispatch:

permissions:
  id-token: write
  pages: write

jobs:
  build-docs:
    name: Build and Deploy Documentation
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.x

      - name: Restore dependencies
        run: dotnet restore

      - name: Build Stroke (Release)
        run: dotnet build src/Stroke/Stroke.csproj --configuration Release --no-restore

      - name: Install DocFX
        run: dotnet tool install -g docfx

      - name: Build Documentation
        working-directory: docfx
        run: |
          docfx metadata
          docfx build

      - name: Setup Pages
        uses: actions/configure-pages@v5

      - name: Upload artifact
        uses: actions/upload-pages-artifact@v3
        with:
          path: docfx/_site

      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v4
        with:
          token: ${{ secrets.GITHUB_TOKEN }}
```

## XML Documentation Standards

All public types and members MUST have XML documentation:

```csharp
/// <summary>
/// Represents an immutable text document with lazy-computed properties.
/// </summary>
/// <remarks>
/// <para>
/// The Document class is the core data structure for text editing.
/// It provides efficient character-based indexing and line-based operations.
/// </para>
/// <para>
/// Documents are immutable - all modification operations return new instances.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a document from text
/// var doc = new Document("Hello\nWorld");
///
/// // Access properties
/// Console.WriteLine(doc.LineCount);        // 2
/// Console.WriteLine(doc.CursorPositionRow); // 0
///
/// // Create modified copy
/// var newDoc = doc.InsertAfter(" there");
/// </code>
/// </example>
public sealed class Document
{
    /// <summary>
    /// Gets the full text content of the document.
    /// </summary>
    /// <value>The complete text as a string.</value>
    public string Text { get; }

    /// <summary>
    /// Inserts text after the current cursor position.
    /// </summary>
    /// <param name="text">The text to insert.</param>
    /// <returns>A new <see cref="Document"/> with the text inserted.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="text"/> is <c>null</c>.
    /// </exception>
    public Document InsertAfter(string text);
}
```

## Documentation Content Requirements

### Getting Started

1. **Installation**
   - NuGet package installation
   - .NET CLI: `dotnet add package Stroke`
   - Package Manager: `Install-Package Stroke`

2. **First Prompt**
   ```csharp
   using Stroke.Shortcuts;

   var result = await Prompt.PromptAsync("Enter your name: ");
   Console.WriteLine($"Hello, {result}!");
   ```

3. **Learning Path**
   - Printing formatted text
   - Simple prompts
   - Prompts with completion
   - Dialogs
   - Full-screen applications

### Tutorials

Each tutorial includes:
- Conceptual explanation
- Code examples (complete, runnable)
- Screenshots of expected output
- Cross-references to related topics
- API links to relevant classes

### Advanced Topics

Each advanced topic includes:
- Architecture diagrams (ASCII or images)
- Deep-dive explanations
- Real-world patterns
- Performance considerations
- Platform-specific notes

### API Reference

Auto-generated with manual overrides for:
- Namespace summaries (conceptual overview)
- Key class examples (inline code)
- Cross-references to tutorials

## Screenshot Standards

- **Format:** PNG (24-bit, no transparency for terminal screenshots)
- **Resolution:** 800px width maximum
- **Font:** Monospace terminal font
- **Theme:** Dark terminal background (consistent across all screenshots)
- **Naming:** `feature-description.png` (lowercase, hyphenated)

## Build Requirements

```xml
<!-- In Stroke.csproj -->
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);CS1591</NoWarn> <!-- Optional: suppress missing XML warnings -->
</PropertyGroup>
```

## Documentation Validation

### Pre-commit Checks

1. **XML Documentation Coverage**
   - All public types documented
   - All public members documented
   - `<summary>` required for all
   - `<param>` required for all parameters
   - `<returns>` required for non-void methods

2. **Link Validation**
   - All internal links resolve
   - All API cross-references valid
   - All image paths exist

3. **Code Example Validation**
   - All code examples compile
   - All examples use current API

### CI Checks

```yaml
- name: Validate XML Documentation
  run: |
    dotnet build src/Stroke/Stroke.csproj \
      -p:TreatWarningsAsErrors=true \
      -p:WarningsAsErrors=CS1591
```

## Timeline

| Phase | Content | Dependencies |
|-------|---------|--------------|
| **Phase 1** | DocFX setup, index page, getting started | Project structure |
| **Phase 2** | Tutorial pages (mirroring PTK) | Core features implemented |
| **Phase 3** | Advanced topics | All features implemented |
| **Phase 4** | API overrides (namespace summaries) | Stable API |
| **Phase 5** | Screenshots and visual polish | All examples working |

## File Inventory

### Required Files

| File | Purpose |
|------|---------|
| `docfx/docfx.json` | DocFX configuration |
| `docfx/docs/index.md` | Documentation home |
| `docfx/docs/toc.yml` | Table of contents |
| `docfx/images/logo.png` | Site logo |
| `docfx/images/favicon.png` | Browser favicon |
| `.github/workflows/docs.yml` | GitHub Actions workflow |

### Documentation Pages (18 minimum)

| # | File | PTK Equivalent |
|---|------|----------------|
| 1 | `docs/index.md` | Landing page |
| 2 | `docs/getting-started/index.md` | `getting_started.rst` |
| 3 | `docs/getting-started/first-prompt.md` | Hello world |
| 4 | `docs/getting-started/learning-path.md` | Recommended path |
| 5 | `docs/tutorials/printing-text.md` | `printing_text.rst` |
| 6 | `docs/tutorials/asking-for-input.md` | `asking_for_input.rst` |
| 7 | `docs/tutorials/dialogs.md` | `dialogs.rst`, `asking_for_a_choice.rst` |
| 8 | `docs/tutorials/progress-bars.md` | `progress_bars.rst` |
| 9 | `docs/tutorials/full-screen-apps.md` | `full_screen_apps.rst` |
| 10 | `docs/tutorials/building-a-repl.md` | `tutorials/repl.rst` |
| 11 | `docs/advanced/key-bindings.md` | `advanced_topics/key_bindings.rst` |
| 12 | `docs/advanced/styling.md` | `advanced_topics/styling.rst` |
| 13 | `docs/advanced/filters.md` | `advanced_topics/filters.rst` |
| 14 | `docs/advanced/layout-system.md` | Layout deep-dive |
| 15 | `docs/advanced/rendering-pipeline.md` | `advanced_topics/rendering_*.rst` |
| 16 | `docs/advanced/architecture.md` | `advanced_topics/architecture.rst` |
| 17 | `docs/advanced/unit-testing.md` | `advanced_topics/unit_testing.rst` |
| 18 | `docs/editing-modes/emacs.md` | Emacs mode reference |
| 19 | `docs/editing-modes/vi.md` | Vi mode reference |
| 20 | `docs/migration/from-ptk.md` | Python → C# migration |

### Namespace Override Pages (10)

| # | File | Namespace |
|---|------|-----------|
| 1 | `apispec/namespace-core.md` | `Stroke.Core` |
| 2 | `apispec/namespace-application.md` | `Stroke.Application` |
| 3 | `apispec/namespace-layout.md` | `Stroke.Layout` |
| 4 | `apispec/namespace-rendering.md` | `Stroke.Rendering` |
| 5 | `apispec/namespace-input.md` | `Stroke.Input` |
| 6 | `apispec/namespace-keybinding.md` | `Stroke.KeyBinding` |
| 7 | `apispec/namespace-completion.md` | `Stroke.Completion` |
| 8 | `apispec/namespace-styles.md` | `Stroke.Styles` |
| 9 | `apispec/namespace-shortcuts.md` | `Stroke.Shortcuts` |
| 10 | `apispec/namespace-widgets.md` | `Stroke.Widgets` |

## Summary

- **30 documentation pages** (18 conceptual + 10 API overrides + 2 TOC)
- **Auto-generated API reference** from XML comments
- **Free hosting** on GitHub Pages
- **Automated builds** via GitHub Actions
- **Full-text search** enabled
- **Cross-linked** with Microsoft .NET documentation
- **1:1 mapping** to Python Prompt Toolkit documentation structure
