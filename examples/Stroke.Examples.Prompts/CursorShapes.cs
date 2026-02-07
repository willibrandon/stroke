using Stroke.CursorShapes;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates different cursor shape configurations.
/// Port of Python Prompt Toolkit's cursor-shapes.py example.
/// </summary>
public static class CursorShapes
{
    public static void Run()
    {
        try
        {
            Prompt.RunPrompt("(block): ", cursor: new SimpleCursorShapeConfig(CursorShape.Block), enableSuspend: true);
            Prompt.RunPrompt("(underline): ", cursor: new SimpleCursorShapeConfig(CursorShape.Underline), enableSuspend: true);
            Prompt.RunPrompt("(beam): ", cursor: new SimpleCursorShapeConfig(CursorShape.Beam), enableSuspend: true);
            Prompt.RunPrompt(
                "(modal - according to vi input mode): ",
                cursor: new ModalCursorShapeConfig(
                    () => ModalCursorShapeConfig.EditingMode.ViNavigation),
                viMode: true,
                enableSuspend: true);
        }
        catch (KeyboardInterruptException)
        {
            // Ctrl+C pressed - exit gracefully
        }
        catch (EOFException)
        {
            // Ctrl+D pressed - exit gracefully
        }
    }
}
