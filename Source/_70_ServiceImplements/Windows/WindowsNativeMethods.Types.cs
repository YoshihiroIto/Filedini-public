using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.Marshalling;

// ReSharper disable once CheckNamespace
namespace Filedini.ServiceImplements.Windows;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static partial class WindowsNativeMethods
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public unsafe struct WIN32_FIND_DATA
    {
        public uint dwFileAttributes;
        public FILETIME ftCreationTime;
        public FILETIME ftLastAccessTime;
        public FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;

        public fixed char cFileName[260];
        public fixed char cAlternateFileName[14];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string MakeFileNameString()
        {
            fixed (char* p = cFileName)
                return new string(p);
        }

        public bool IsFileNameDotOrDoubleDot
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if false
                if (cFileName[0] is not '.')
                    return false;

                if (cFileName[1] is (char)0)
                    return true;

                return cFileName[1] is '.' &&
                       cFileName[2] is (char)0;
#else
                if (cFileName[0] != '.')
                    return false;

                var nextTwoChars = Unsafe.ReadUnaligned<uint>(ref Unsafe.As<char, byte>(ref cFileName[1]));
                return (nextTwoChars & 0xFFFF) == 0 || nextTwoChars == '.';
#endif
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct WIN32_FILE_ATTRIBUTE_DATA
    {
        public FileAttributes dwFileAttributes;
        public FILETIME ftCreationTime;
        public FILETIME ftLastAccessTime;
        public FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
    }

    public enum GET_FILEEX_INFO_LEVELS
    {
        GetFileExInfoStandard = 0
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public unsafe struct SHELLEXECUTEINFO
    {
        public int cbSize;
        public uint fMask;
        public IntPtr hwnd;
        public char* lpVerb;
        public char* lpFile;
        public char* lpParameters;
        public char* lpDirectory;
        public int nShow;
        public IntPtr hInstApp;
        public IntPtr lpIDList;

        public char* lpClass;

        public IntPtr hkeyClass;
        public uint dwHotKey;
        public IntPtr hIconOrMonitor;
        public IntPtr hProcess;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public unsafe struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;

        public fixed ushort szDisplayName[MAX_PATH];
        public fixed ushort szTypeName[80];
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public unsafe struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;

        [MarshalAs(UnmanagedType.U4)]
        public FileOperationType wFunc;

        public char* pFrom;
        public IntPtr pTo;
        public FileOperationFlags fFlags;

        [MarshalAs(UnmanagedType.Bool)]
        public bool fAnyOperationsAborted;

        public IntPtr hNameMappings;
        public IntPtr lpszProgressTitle;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct SHQUERYRBINFO
    {
        public int cbSize;
        public long i64Size;
        public long i64NumItems;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CWPSTRUCT
    {
        public IntPtr lparam;
        public IntPtr wparam;
        public int message;
        public IntPtr hwnd;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public unsafe struct CMINVOKECOMMANDINFOEX
    {
        public int cbSize;
        public CMIC fMask;
        public IntPtr hwnd;
        public IntPtr lpVerb;
        public char* lpParameters;
        public char* lpDirectory;
        public SW nShow;
        public int dwHotKey;
        public IntPtr hIcon;
        public char* lpTitle;
        public IntPtr lpVerbW;
        public char* lpParametersW;
        public char* lpDirectoryW;
        public char* lpTitleW;
        public POINT ptInvoke;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct POINT(int x, int y)
    {
        public int x = x;
        public int y = y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SHChangeNotifyEntry
    {
        public IntPtr pIdl;
        public bool Recursively;
    }

    [GeneratedComInterface]
    [Guid("000214E6-0000-0000-C000-000000000046")]
    public partial interface IShellFolder
    {
        [PreserveSig]
        int ParseDisplayName(IntPtr hwnd, IntPtr pbc, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName,
            ref uint pchEaten, out IntPtr ppidl, ref SFGAO pdwAttributes);

        [PreserveSig]
        int EnumObjects(IntPtr hwnd, SHCONTF grfFlags, out IntPtr enumIDList);

        [PreserveSig]
        int BindToObject(IntPtr pidl, IntPtr pbc, in Guid riid, out IntPtr ppv);

        [PreserveSig]
        int BindToStorage(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);

        [PreserveSig]
        int CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);

        [PreserveSig]
        int CreateViewObject(IntPtr hwndOwner, Guid riid, out IntPtr ppv);

        [PreserveSig]
        int GetAttributesOf(uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, ref SFGAO rgfInOut);

        [PreserveSig]
        int GetUIObjectOf(IntPtr hwndOwner, uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl,
            in Guid riid,
            IntPtr rgfReserved, out IntPtr ppv);

        [PreserveSig]
        int GetDisplayNameOf(IntPtr pidl, SHGNO uFlags, IntPtr lpName);

        [PreserveSig]
        int SetNameOf(IntPtr hwnd, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, SHGNO uFlags,
            out IntPtr ppidlOut);
    }

    [GeneratedComInterface]
    [Guid("000214e4-0000-0000-c000-000000000046")]
    public partial interface IContextMenu
    {
        [PreserveSig]
        int QueryContextMenu(IntPtr hmenu, uint iMenu, uint idCmdFirst, uint idCmdLast, CMF uFlags);

        [PreserveSig]
        int InvokeCommand(ref CMINVOKECOMMANDINFOEX info);

        [PreserveSig]
        int GetCommandString(uint idcmd, GCS uflags, uint reserved,
            [MarshalAs(UnmanagedType.LPArray)] byte[] commandstring, int cch);
    }

    [GeneratedComInterface]
    [Guid("000214f4-0000-0000-c000-000000000046")]
    public partial interface IContextMenu2
    {
        [PreserveSig]
        int QueryContextMenu(IntPtr hmenu, uint iMenu, uint idCmdFirst, uint idCmdLast, CMF uFlags);

        [PreserveSig]
        int InvokeCommand(ref CMINVOKECOMMANDINFOEX info);

        [PreserveSig]
        int GetCommandString(uint idcmd, GCS uflags, uint reserved,
            [MarshalAs(UnmanagedType.LPArray)] byte[] commandstring, int cch);

        [PreserveSig]
        int HandleMenuMsg(uint uMsg, IntPtr wParam, IntPtr lParam);
    }

    [GeneratedComInterface]
    [Guid("bcfce0a0-ec17-11d0-8d10-00a0c90f2719")]
    public partial interface IContextMenu3
    {
        [PreserveSig]
        int QueryContextMenu(IntPtr hmenu, uint iMenu, uint idCmdFirst, uint idCmdLast, CMF uFlags);

        [PreserveSig]
        int InvokeCommand(ref CMINVOKECOMMANDINFOEX info);

        [PreserveSig]
        int GetCommandString(uint idcmd, GCS uflags, uint reserved,
            [MarshalAs(UnmanagedType.LPArray)] byte[] commandstring, int cch);

        [PreserveSig]
        int HandleMenuMsg(uint uMsg, IntPtr wParam, IntPtr lParam);

        [PreserveSig]
        int HandleMenuMsg2(uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr plResult);
    }
}