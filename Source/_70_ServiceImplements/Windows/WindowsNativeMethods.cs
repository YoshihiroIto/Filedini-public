using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: DisableRuntimeMarshalling]

// ReSharper disable once CheckNamespace
namespace Filedini.ServiceImplements.Windows;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static partial class WindowsNativeMethods
{
    public static bool IsKeyPressShiftKey => (GetKeyState(VK_SHIFT) & 0x8000) is not 0;
    public static bool IsKeyPressControlKey => (GetKeyState(VK_CONTROL) & 0x8000) is not 0;

    [LibraryImport("kernel32.dll", EntryPoint = "FindFirstFileExW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial IntPtr FindFirstFileEx(string lpFileName, FINDEX_INFO_LEVELS fInfoLevelId,
        out WIN32_FIND_DATA lpFindFileData, FINDEX_SEARCH_OPS fSearchOp, IntPtr lpSearchFilter, int dwAdditionalFlags);

    [LibraryImport("kernel32.dll", EntryPoint = "FindFirstFileExW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial IntPtr FindFirstFileEx(ReadOnlySpan<char> lpFileName, FINDEX_INFO_LEVELS fInfoLevelId,
        out WIN32_FIND_DATA lpFindFileData, FINDEX_SEARCH_OPS fSearchOp, IntPtr lpSearchFilter, int dwAdditionalFlags);

    [LibraryImport("kernel32.dll", EntryPoint = "FindNextFileW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool FindClose(IntPtr hFindFile);

    [LibraryImport("kernel32.dll", EntryPoint = "GetFileAttributesW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial uint GetFileAttributes(string fileName);

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetFileAttributesExW(string lpFileName, GET_FILEEX_INFO_LEVELS fInfoLevelId,
        out WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

    [LibraryImport("Shell32.dll", EntryPoint = "ShellExecuteExW", StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

    [LibraryImport("Shell32.dll", EntryPoint = "SHGetFileInfoW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO shfileinfo,
        int cbFileInfo, uint uFlags);

    [LibraryImport("Shell32.dll", EntryPoint = "SHGetImageList", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int SHGetImageList(uint iImageList, in Guid riid, out IntPtr ppv);

    [LibraryImport("Shell32.dll", EntryPoint = "SHEmptyRecycleBinW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial uint SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

    [LibraryImport("shell32.dll", EntryPoint = "SHFileOperationW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

    [LibraryImport("shell32.dll", EntryPoint = "SHQueryRecycleBinW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int SHQueryRecycleBin(string pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);

    [LibraryImport("shell32.dll")]
    public static partial int SHGetDesktopFolder(out IntPtr ppshf);

    [LibraryImport("shell32.dll")]
    public static partial uint SHChangeNotifyRegister(IntPtr hWnd, SHCNF fSources, SHCNE fEvents, uint wMsg,
        int cEntries, ref SHChangeNotifyEntry pFsne);

    [LibraryImport("shell32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SHChangeNotifyDeregister(uint hNotify);

    [LibraryImport("user32.dll", EntryPoint = "DestroyIcon")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyIcon(IntPtr hIcon);

    [LibraryImport("user32.dll")]
    public static partial uint TrackPopupMenuEx(IntPtr hmenu, TPM flags, int x, int y, IntPtr hwnd, IntPtr lptpm);

    [LibraryImport("user32")]
    public static partial IntPtr CreatePopupMenu();

    [LibraryImport("user32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyMenu(IntPtr hMenu);

    [LibraryImport("user32.dll")]
    public static partial short GetKeyState(int key);

    [LibraryImport("comctl32.dll", SetLastError = true)]
    public static partial IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);

    [LibraryImport("shlwapi.dll", EntryPoint = "StrRetToBufW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int StrRetToBuf(IntPtr pstr, IntPtr pidl, IntPtr pszBuf, int cchBuf);


    [LibraryImport("kernel32.dll", EntryPoint = "CreateFileW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode,
        IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetFileInformationByHandleEx(IntPtr hFile, uint FileInformationClass,
        IntPtr lpFileInformation, uint dwBufferSize);

    [LibraryImport("kernel32.dll")]
    internal static partial uint GetLastError();
}