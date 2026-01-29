namespace Stroke.Tests.Layout;

/// <summary>
/// Test implementation of <see cref="Stroke.Layout.IWindow"/> for unit testing.
/// </summary>
/// <remarks>
/// Provides name-based equality semantics for dictionary key usage in tests.
/// </remarks>
internal sealed class TestWindow : Stroke.Layout.IWindow, IEquatable<TestWindow>
{
    /// <summary>
    /// Gets the name identifying this window.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestWindow"/> class.
    /// </summary>
    /// <param name="name">The name identifying this window.</param>
    public TestWindow(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));

    /// <inheritdoc/>
    public bool Equals(TestWindow? other) => other is not null && Name == other.Name;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is TestWindow w && Equals(w);

    /// <inheritdoc/>
    public override int GetHashCode() => Name.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => $"TestWindow({Name})";
}
