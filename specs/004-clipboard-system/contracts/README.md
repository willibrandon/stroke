# Contracts: Clipboard System

This feature does not expose external APIs (REST, GraphQL, etc.).

The clipboard system is an internal library component with a C# interface (`IClipboard`) and implementations. See `data-model.md` for the detailed type contracts.

## Internal Contracts

| Type | File | Description |
|------|------|-------------|
| `IClipboard` | `IClipboard.cs` | Interface defining clipboard operations |
| `ClipboardData` | `ClipboardData.cs` | Immutable data container |
| `InMemoryClipboard` | `InMemoryClipboard.cs` | Kill ring implementation |
| `DummyClipboard` | `DummyClipboard.cs` | No-op implementation |
| `DynamicClipboard` | `DynamicClipboard.cs` | Dynamic wrapper |
