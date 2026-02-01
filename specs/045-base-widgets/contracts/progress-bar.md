# Contract: ProgressBar

**Namespace**: `Stroke.Widgets.Base`
**Python Source**: `prompt_toolkit/widgets/base.py` lines 1036-1081

## API

```csharp
/// <summary>
/// Progress bar widget showing percentage completion.
/// </summary>
/// <remarks>
/// Thread-safe. Percentage protected by Lock.
/// </remarks>
public class ProgressBar : IMagicContainer
{
    public ProgressBar();

    /// <summary>
    /// Gets or sets the percentage (0-100). Default: 60.
    /// </summary>
    /// <remarks>
    /// Not clamped to 0-100 to match Python behavior.
    /// Setting updates the label text.
    /// </remarks>
    public int Percentage { get; set; }

    /// <summary>
    /// Gets the label widget displaying the percentage text.
    /// </summary>
    public Label Label { get; }

    /// <summary>
    /// Gets the underlying container.
    /// </summary>
    public FloatContainer Container { get; }

    // IMagicContainer
    public IContainer PtContainer();  // returns Container
}
```

## Layout Structure

```
FloatContainer(
  content: Window(height=1),
  floats: [
    Float(content=Label("60%"), top=0, bottom=0),
    Float(left=0, top=0, right=0, bottom=0,
      content: VSplit [
        Window(style="class:progress-bar.used", width=D(weight=percentage)),
        Window(style="class:progress-bar", width=D(weight=100-percentage))
      ]
    )
  ]
)
```

## Thread Safety

- `Lock _lock` protects `_percentage` reads and writes
- Setting `Percentage` MUST update both `_percentage` and `Label.Text` within the same lock scope: `using (_lock.EnterScope()) { _percentage = value; Label.Text = $"{value}%"; }`. This ensures a reader never sees a percentage value inconsistent with the displayed label text.
- Reading `Percentage` also acquires the lock for a consistent read.

## Notes

- Default percentage is 60 (matching Python)
- No clamping — matches Python behavior
- Uses weighted dimensions for proportional bar rendering
