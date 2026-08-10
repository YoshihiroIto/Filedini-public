#if TARGET_WINDOWS

using System.Runtime.InteropServices;
using Filedini.DomainModel.FileSystem;

// ReSharper disable once CheckNamespace
namespace Filedini.ServiceImplements.Windows;

using static WindowsNativeMethods;
using static ComHelper;

internal static class ShellItemIdListResolver
{
    public static bool TryResolve(ShellItemReference item, out IntPtr absoluteIdList)
    {
        if (SHParseDisplayName(
                item.RootParsingPath,
                IntPtr.Zero,
                out var currentAbsoluteIdList,
                0,
                out _) is not S_OK)
        {
            absoluteIdList = IntPtr.Zero;
            return false;
        }

        if (item.RelativeDisplaySegments.Count is 0)
        {
            absoluteIdList = currentAbsoluteIdList;
            return true;
        }

        if (SHGetDesktopFolder(out var rawDesktopFolder) is not S_OK)
        {
            Marshal.FreeCoTaskMem(currentAbsoluteIdList);
            absoluteIdList = IntPtr.Zero;
            return false;
        }

        var desktopFolder = GetOrCreateObjectForComInstance<IShellFolder>(rawDesktopFolder);
        if (desktopFolder.BindToObject(
                currentAbsoluteIdList,
                IntPtr.Zero,
                in IID_IShellFolder,
                out var rawCurrentFolder) is not S_OK)
        {
            Marshal.FreeCoTaskMem(currentAbsoluteIdList);
            absoluteIdList = IntPtr.Zero;
            return false;
        }

        var currentFolder = GetOrCreateObjectForComInstance<IShellFolder>(rawCurrentFolder);
        for (var index = 0; index < item.RelativeDisplaySegments.Count; ++index)
        {
            var eaten = 0U;
            var attributes = default(SFGAO);
            if (currentFolder.ParseDisplayName(
                    IntPtr.Zero,
                    IntPtr.Zero,
                    item.RelativeDisplaySegments[index],
                    ref eaten,
                    out var childRelativeIdList,
                    ref attributes) is not S_OK)
            {
                Marshal.FreeCoTaskMem(currentAbsoluteIdList);
                absoluteIdList = IntPtr.Zero;
                return false;
            }

            var childAbsoluteIdList = ILCombine(currentAbsoluteIdList, childRelativeIdList);
            if (childAbsoluteIdList == IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(childRelativeIdList);
                Marshal.FreeCoTaskMem(currentAbsoluteIdList);
                absoluteIdList = IntPtr.Zero;
                return false;
            }

            if (index + 1 < item.RelativeDisplaySegments.Count)
            {
                if (currentFolder.BindToObject(
                        childRelativeIdList,
                        IntPtr.Zero,
                        in IID_IShellFolder,
                        out var rawChildFolder) is not S_OK)
                {
                    Marshal.FreeCoTaskMem(childRelativeIdList);
                    Marshal.FreeCoTaskMem(childAbsoluteIdList);
                    Marshal.FreeCoTaskMem(currentAbsoluteIdList);
                    absoluteIdList = IntPtr.Zero;
                    return false;
                }

                currentFolder = GetOrCreateObjectForComInstance<IShellFolder>(rawChildFolder);
            }

            Marshal.FreeCoTaskMem(childRelativeIdList);
            Marshal.FreeCoTaskMem(currentAbsoluteIdList);
            currentAbsoluteIdList = childAbsoluteIdList;
        }

        absoluteIdList = currentAbsoluteIdList;
        return true;
    }
}

#endif
