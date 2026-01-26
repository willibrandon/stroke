using System.Text;

namespace Stroke.FormattedText;

/// <summary>
/// Internal formatter for safe ANSI string interpolation.
/// </summary>
/// <remarks>
/// Equivalent to Python Prompt Toolkit's <c>ANSIFormatter</c> class.
/// </remarks>
internal static class AnsiFormatter
{
    /// <summary>
    /// Neutralizes ANSI escape sequences in a string.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>The text with escape characters (\x1b) and backspaces (\b) replaced with '?'.</returns>
    public static string Escape(object? text)
    {
        var str = text?.ToString() ?? string.Empty;
        return str
            .Replace("\x1b", "?")
            .Replace("\b", "?");
    }

    /// <summary>
    /// Formats a string with positional arguments, escaping ANSI sequences in the arguments.
    /// </summary>
    /// <param name="format">The format string with {0}, {1}, etc. placeholders.</param>
    /// <param name="args">The arguments to substitute.</param>
    /// <returns>The formatted string with escaped arguments.</returns>
    public static string Format(string format, params object[] args)
    {
        // Escape all arguments
        var escapedArgs = new object[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            escapedArgs[i] = Escape(args[i]);
        }

        return string.Format(format, escapedArgs);
    }

    /// <summary>
    /// Formats a string with named arguments, escaping ANSI sequences in the arguments.
    /// </summary>
    /// <param name="format">The format string with {name} placeholders.</param>
    /// <param name="args">The named arguments to substitute.</param>
    /// <returns>The formatted string with escaped arguments.</returns>
    public static string Format(string format, IDictionary<string, object> args)
    {
        var result = format;
        foreach (var (key, value) in args)
        {
            result = result.Replace($"{{{key}}}", Escape(value));
        }
        return result;
    }

    /// <summary>
    /// Formats a string using %-style formatting with ANSI escaping.
    /// </summary>
    /// <param name="format">The format string with %s placeholders.</param>
    /// <param name="args">The arguments to substitute.</param>
    /// <returns>The formatted string with escaped arguments.</returns>
    public static string FormatPercent(string format, params object[] args)
    {
        var escapedArgs = new object[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            escapedArgs[i] = Escape(args[i]);
        }

        // Replace %s placeholders one at a time
        var result = new StringBuilder(format);
        int argIndex = 0;

        // Find and replace each %s
        int pos = 0;
        while ((pos = result.ToString().IndexOf("%s", pos, StringComparison.Ordinal)) >= 0 && argIndex < escapedArgs.Length)
        {
            var replacement = escapedArgs[argIndex].ToString() ?? string.Empty;
            result.Remove(pos, 2);
            result.Insert(pos, replacement);
            pos += replacement.Length;
            argIndex++;
        }

        return result.ToString();
    }
}
