namespace Stroke.Input.Windows;

/// <summary>
/// Standard console handle identifiers.
/// </summary>
/// <remarks>
/// <para>
/// Pass these values to <see cref="ConsoleApi.GetStdHandle"/> to retrieve
/// the corresponding console handle.
/// </para>
/// </remarks>
public static class StdHandles
{
    /// <summary>
    /// Standard input handle identifier.
    /// </summary>
    public const int STD_INPUT_HANDLE = -10;

    /// <summary>
    /// Standard output handle identifier.
    /// </summary>
    public const int STD_OUTPUT_HANDLE = -11;

    /// <summary>
    /// Standard error handle identifier.
    /// </summary>
    public const int STD_ERROR_HANDLE = -12;
}
