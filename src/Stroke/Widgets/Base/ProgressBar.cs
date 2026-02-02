using Stroke.Layout;
using Stroke.Layout.Containers;

namespace Stroke.Widgets.Base;

/// <summary>
/// Widget for displaying a progress bar with a percentage label overlay.
/// </summary>
/// <remarks>
/// <para>
/// The progress bar is composed of a <see cref="FloatContainer"/> with two layers:
/// a <see cref="Label"/> displaying the percentage text, and a <see cref="VSplit"/>
/// with two dynamically weighted windows representing the used and remaining portions.
/// The label floats on top so its text is visible through the progress bar colors.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>ProgressBar</c> class from <c>widgets/base.py</c>.
/// </para>
/// <para>
/// Thread-safe: The <see cref="Percentage"/> property is protected by a <see cref="Lock"/>
/// to ensure atomic reads and writes of both the percentage value and the label text.
/// </para>
/// </remarks>
public class ProgressBar : IMagicContainer
{
    private readonly Lock _lock = new();
    private int _percentage;

    /// <summary>
    /// Gets the label that displays the percentage text.
    /// </summary>
    public Label Label { get; }

    /// <summary>
    /// Gets the underlying <see cref="FloatContainer"/> that composes the progress bar layout.
    /// </summary>
    public FloatContainer Container { get; }

    /// <summary>
    /// Gets or sets the current progress percentage (0–100).
    /// </summary>
    /// <remarks>
    /// Setting this property atomically updates both the internal percentage value
    /// and the <see cref="Label"/> text under a lock.
    /// </remarks>
    public int Percentage
    {
        get
        {
            using (_lock.EnterScope())
                return _percentage;
        }
        set
        {
            using (_lock.EnterScope())
            {
                _percentage = value;
                Label.Text = $"{value}%";
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressBar"/> class with a default
    /// percentage of 60.
    /// </summary>
    public ProgressBar()
    {
        _percentage = 60;

        Label = new Label("60%");

        // We first draw the label, then the actual progress bar. Right
        // now, this is the only way to have the colors of the progress
        // bar appear on top of the label. The problem is that our label
        // can't be part of any Window below.
        var vsplit = new VSplit(
            new IContainer[]
            {
                new Window(
                    style: "class:progress-bar.used",
                    widthGetter: () =>
                    {
                        int p;
                        using (_lock.EnterScope())
                            p = _percentage;
                        return new Dimension(weight: p);
                    }),
                new Window(
                    style: "class:progress-bar",
                    widthGetter: () =>
                    {
                        int p;
                        using (_lock.EnterScope())
                            p = _percentage;
                        return new Dimension(weight: 100 - p);
                    }),
            });

        Container = new FloatContainer(
            content: new AnyContainer(new Window(height: Dimension.Exact(1))),
            floats: new[]
            {
                new Float(
                    content: new AnyContainer(Label),
                    top: 0,
                    bottom: 0),
                new Float(
                    content: new AnyContainer(vsplit),
                    left: 0,
                    top: 0,
                    right: 0,
                    bottom: 0),
            });
    }

    /// <inheritdoc/>
    public IContainer PtContainer() => Container;
}
