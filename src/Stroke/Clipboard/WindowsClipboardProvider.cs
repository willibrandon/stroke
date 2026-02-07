using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Stroke.Clipboard;

/// <summary>
/// Clipboard provider for Windows using Win32 API (P/Invoke).
/// </summary>
/// <remarks>
/// <para>
/// Uses <c>OpenClipboard</c>, <c>GetClipboardData</c>, <c>SetClipboardData</c>,
/// and related Win32 functions with <c>CF_UNICODETEXT</c> format.
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is stateless and inherently thread-safe.
/// The Win32 clipboard API serializes access via <c>OpenClipboard</c>/<c>CloseClipboard</c>.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
internal sealed partial class WindowsClipboardProvider : IClipboardProvider
{
    private const uint CfUnicodeText = 13;
    private const uint GmemMoveable = 0x0002;

    /// <inheritdoc/>
    public void SetText(string text)
    {
        try
        {
            if (!ClipboardApi.OpenClipboard(nint.Zero))
            {
                return;
            }

            try
            {
                ClipboardApi.EmptyClipboard();

                var bytes = (text.Length + 1) * 2; // UTF-16 + null terminator
                var hGlobal = ClipboardApi.GlobalAlloc(GmemMoveable, (nuint)bytes);
                if (hGlobal == nint.Zero)
                {
                    return;
                }

                var locked = ClipboardApi.GlobalLock(hGlobal);
                if (locked == nint.Zero)
                {
                    ClipboardApi.GlobalFree(hGlobal);
                    return;
                }

                try
                {
                    Marshal.Copy(text.ToCharArray(), 0, locked, text.Length);
                    // Write null terminator
                    Marshal.WriteInt16(locked, text.Length * 2, 0);
                }
                finally
                {
                    ClipboardApi.GlobalUnlock(hGlobal);
                }

                if (ClipboardApi.SetClipboardData(CfUnicodeText, hGlobal) == nint.Zero)
                {
                    // SetClipboardData failed — we still own the memory
                    ClipboardApi.GlobalFree(hGlobal);
                }
                // On success, the system owns hGlobal — do NOT free it
            }
            finally
            {
                ClipboardApi.CloseClipboard();
            }
        }
        catch
        {
            // Silently swallow all write failures (FR-008)
        }
    }

    /// <inheritdoc/>
    public string GetText()
    {
        try
        {
            if (!ClipboardApi.OpenClipboard(nint.Zero))
            {
                return "";
            }

            try
            {
                var hData = ClipboardApi.GetClipboardData(CfUnicodeText);
                if (hData == nint.Zero)
                {
                    return "";
                }

                var locked = ClipboardApi.GlobalLock(hData);
                if (locked == nint.Zero)
                {
                    return "";
                }

                try
                {
                    return Marshal.PtrToStringUni(locked) ?? "";
                }
                finally
                {
                    ClipboardApi.GlobalUnlock(hData);
                }
            }
            finally
            {
                ClipboardApi.CloseClipboard();
            }
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// P/Invoke declarations for Win32 clipboard operations.
    /// </summary>
    [SupportedOSPlatform("windows")]
    private static partial class ClipboardApi
    {
        private const string User32 = "user32.dll";
        private const string Kernel32 = "kernel32.dll";

        [LibraryImport(User32, EntryPoint = "OpenClipboard", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool OpenClipboard(nint hWndNewOwner);

        [LibraryImport(User32, EntryPoint = "CloseClipboard", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool CloseClipboard();

        [LibraryImport(User32, EntryPoint = "EmptyClipboard", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool EmptyClipboard();

        [LibraryImport(User32, EntryPoint = "GetClipboardData", SetLastError = true)]
        public static partial nint GetClipboardData(uint uFormat);

        [LibraryImport(User32, EntryPoint = "SetClipboardData", SetLastError = true)]
        public static partial nint SetClipboardData(uint uFormat, nint hMem);

        [LibraryImport(Kernel32, EntryPoint = "GlobalAlloc", SetLastError = true)]
        public static partial nint GlobalAlloc(uint uFlags, nuint dwBytes);

        [LibraryImport(Kernel32, EntryPoint = "GlobalLock", SetLastError = true)]
        public static partial nint GlobalLock(nint hMem);

        [LibraryImport(Kernel32, EntryPoint = "GlobalUnlock", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GlobalUnlock(nint hMem);

        [LibraryImport(Kernel32, EntryPoint = "GlobalFree", SetLastError = true)]
        public static partial nint GlobalFree(nint hMem);
    }
}
