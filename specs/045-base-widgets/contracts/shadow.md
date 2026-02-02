# Contract: Shadow

**Namespace**: `Stroke.Widgets.Base`
**Python Source**: `prompt_toolkit/widgets/base.py` lines 574-607

## API

```csharp
/// <summary>
/// Draw a shadow underneath/behind a container.
/// </summary>
public class Shadow : IMagicContainer
{
    public Shadow(AnyContainer body);

    public FloatContainer Container { get; }

    // IMagicContainer
    public IContainer PtContainer();  // returns Container
}
```

## Layout Structure

```
FloatContainer(
  content: body,
  floats: [
    Float(bottom=-1, height=1, left=1, right=-1, transparent=true,
          content=Window(style="class:shadow")),     // bottom shadow strip
    Float(bottom=-1, top=1, width=1, right=-1, transparent=true,
          content=Window(style="class:shadow"))      // right shadow strip
  ]
)
```

## Notes

- Two transparent Float windows create the shadow effect
- Bottom float: 1 row high, offset 1 right and 1 below
- Right float: 1 col wide, offset 1 below body top and 1 right of body right
- The `class:shadow` style should define dark/dim colors for the shadow effect
