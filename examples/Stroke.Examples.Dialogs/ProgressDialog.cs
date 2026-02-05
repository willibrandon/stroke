using Stroke.Shortcuts;

using static Stroke.Shortcuts.Dialogs;

namespace Stroke.Examples.DialogExamples;

/// <summary>
/// Example of a progress dialog with background task.
/// Port of Python Prompt Toolkit's progress_dialog.py example.
/// </summary>
internal static class ProgressDialogExample
{
    public static void Run()
    {
        try
        {
            ProgressDialog(
                title: "Progress dialog example",
                text: "As an examples, we walk through the filesystem and print all directories",
                runCallback: Worker
            ).Run();
        }
        catch (KeyboardInterrupt)
        {
            // Ctrl+C pressed - exit gracefully
        }
        catch (EOFException)
        {
            // Ctrl+D pressed - exit gracefully
        }
    }

    /// <summary>
    /// Worker function called by the progress dialog. Runs in a background thread.
    /// The setPercentage function updates the progress bar, while logText logs
    /// messages to the logging window.
    /// </summary>
    private static void Worker(Action<int> setPercentage, Action<string> logText)
    {
        var percentage = 0;

        try
        {
            // Walk through the repository root, matching Python example behavior.
            // Python runs from examples/dialogs/ and uses os.walk("../..") to reach repo root.
            // .NET runs from bin/Debug/net10.0/, so we go up 5 levels to reach repo root.
            // Output format matches Python: "../../relative/path / filename"
            const string relativePath = "../..";
            var startPath = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

            foreach (var (dirPath, fileName) in WalkFiles(startPath, relativePath))
            {
                logText($"{dirPath} / {fileName}\n");
                setPercentage(percentage + 1);
                percentage += 2;
                Thread.Sleep(100);

                if (percentage >= 100)
                    break;
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Handle permission errors gracefully (edge case)
        }

        // Show 100% for a second before quitting
        setPercentage(100);
        Thread.Sleep(1000);
    }

    /// <summary>
    /// Walk through a directory tree yielding (relativeDirPath, fileName) tuples,
    /// matching Python's os.walk() behavior with relative path output.
    /// </summary>
    private static IEnumerable<(string DirPath, string FileName)> WalkFiles(string rootPath, string relativePrefix)
    {
        var rootFullPath = Path.GetFullPath(rootPath);
        Queue<string> directories = new();
        directories.Enqueue(rootFullPath);

        while (directories.Count > 0)
        {
            var currentDir = directories.Dequeue();
            string[] files;
            string[] subdirs;

            try
            {
                files = Directory.GetFiles(currentDir);
                subdirs = Directory.GetDirectories(currentDir);
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }

            // Compute relative path from root for display (matching Python's output)
            var relativeDir = currentDir == rootFullPath
                ? relativePrefix
                : Path.Combine(relativePrefix, Path.GetRelativePath(rootFullPath, currentDir));

            // Yield files in current directory (matching Python's inner loop)
            foreach (var file in files)
            {
                yield return (relativeDir, Path.GetFileName(file));
            }

            // Queue subdirectories for later processing
            foreach (var subdir in subdirs)
            {
                directories.Enqueue(subdir);
            }
        }
    }
}
