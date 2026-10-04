#if TARGET_WINDOWS

using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Filedini.ServiceImplements.Windows;

using static WindowsNativeMethods;

internal sealed class ShellContextMenu
{
    public static void Show(TopLevel topLevel, FileInfo[] files, int x, int y)
    {
        Guard.IsNotEmpty(files);

        var leadFile = files[0];
        files = leadFile.DirectoryName is null
            ? files.Where(f => string.Equals(f.FullName, leadFile.FullName, StringComparison.OrdinalIgnoreCase)).ToArray()
            : files.Where(f => string.Equals(f.DirectoryName, leadFile.DirectoryName, StringComparison.OrdinalIgnoreCase)).ToArray();

        var handleOwner = topLevel.TryGetPlatformHandle()?.Handle;
        if (handleOwner is null)
            return;

        IntPtr[] idls = [];
        var hook = default(Hook);
        var menu = IntPtr.Zero;
        var comObjects = new ComObjectScope();

        try
        {
            if (!TryCreateAbsoluteIdls(files, out idls))
                return;

            if (!TryGetContextMenuInterfaces(idls, comObjects, out var contextMenu, out var contextMenu2,
                    out var contextMenu3))
                return;

            ApplyTheme(topLevel);

            menu = CreatePopupMenu();

            contextMenu.QueryContextMenu(menu, 0, CMD_FIRST, CMD_LAST,
                CMF.EXPLORE | CMF.NORMAL | (IsKeyPressShiftKey ? CMF.EXTENDEDVERBS : 0));

            hook = new Hook(topLevel, contextMenu2, contextMenu3);

            var selectedIndex = TrackPopupMenuEx(
                menu, TPM.RETURNCMD, x, y, (IntPtr)handleOwner, IntPtr.Zero);

            if (selectedIndex >= CMD_FIRST)
                InvokeCommand(contextMenu, handleOwner.Value, selectedIndex, GetInvokeDirectory(files[0]), x, y);
        }
        finally
        {
            try
            {
                hook?.Dispose();
            }
            finally
            {
                try
                {
                    if (menu != IntPtr.Zero)
                        DestroyMenu(menu);
                }
                finally
                {
                    try
                    {
                        // The window hook and native menu must stop using the
                        // interfaces before their owning wrappers are released.
                        comObjects.Dispose();
                    }
                    finally
                    {
                        FreeIdls(idls);
                    }
                }
            }
        }
    }

    private static bool TryGetContextMenuInterfaces(
        IntPtr[] idls,
        ComObjectScope comObjects,
        out IContextMenu contextMenu,
        out IContextMenu2? contextMenu2,
        out IContextMenu3? contextMenu3)
    {
        contextMenu = null!;
        contextMenu2 = null;
        contextMenu3 = null;

        var result = SHCreateShellItemArrayFromIDLists((uint)idls.Length, idls, out var unknownShellItemArray);
        if (result is not S_OK)
        {
            if (unknownShellItemArray != IntPtr.Zero)
                Marshal.Release(unknownShellItemArray);
            return false;
        }

        var shellItemArray = comObjects.GetObjectForOwnedReference<IShellItemArray>(unknownShellItemArray);
        result = shellItemArray.BindToHandler(IntPtr.Zero, in BHID_SFUIObject, in IID_IContextMenu, out var unknownContextMenu);

        if (result is not S_OK)
        {
            if (unknownContextMenu != IntPtr.Zero)
                Marshal.Release(unknownContextMenu);
            return false;
        }

        contextMenu = comObjects.GetObjectForOwnedReference<IContextMenu>(unknownContextMenu);
        // Generated COM interface casts query and cache optional interfaces in
        // the same uniquely owned wrapper, which releases all of them together.
        contextMenu2 = contextMenu as IContextMenu2;
        contextMenu3 = contextMenu as IContextMenu3;

        return true;
    }

    private static bool TryCreateAbsoluteIdls(FileInfo[] files, out IntPtr[] idls)
    {
        idls = new IntPtr[files.Length];

        for (var i = 0; i != files.Length; ++i)
        {
            var result = SHParseDisplayName(files[i].FullName, IntPtr.Zero, out var absoluteIdl, 0, out _);
            if (result is S_OK)
            {
                idls[i] = absoluteIdl;
                continue;
            }

            FreeIdls(idls);
            idls = [];
            return false;
        }

        return true;
    }

    private static string GetInvokeDirectory(FileInfo file)
    {
        return file.DirectoryName ?? file.FullName;
    }

    internal static string GetChildParseDisplayName(FileInfo file)
    {
        return string.IsNullOrEmpty(file.Name) ? file.FullName : file.Name;
    }

    private static void FreeIdls(IntPtr[] idls)
    {
        for (var i = 0; i != idls.Length; i++)
        {
            if (idls[i] == IntPtr.Zero)
                continue;

            Marshal.FreeCoTaskMem(idls[i]);
            idls[i] = IntPtr.Zero;
        }
    }

    private static bool? _isSetPreferredAppModeAvailable;

    private static void ApplyTheme(TopLevel topLevel)
    {
        if (_isSetPreferredAppModeAvailable is false)
            return;

        try
        {
            var mode = topLevel.ActualThemeVariant == ThemeVariant.Dark ? 2 : 3;
            SetPreferredAppMode(mode);
            FlushMenuThemes();
            _isSetPreferredAppModeAvailable = true;
        }
        catch (EntryPointNotFoundException)
        {
            _isSetPreferredAppModeAvailable = false;
        }
        catch (DllNotFoundException)
        {
            _isSetPreferredAppModeAvailable = false;
        }
    }

    private static unsafe void InvokeCommand(
        IContextMenu contextMenu,
        IntPtr hwndOwner,
        uint cmd,
        string folderName,
        int x,
        int y)
    {
        fixed (char* p = folderName)
        {
            var command = new CMINVOKECOMMANDINFOEX
            {
                cbSize = sizeof(CMINVOKECOMMANDINFOEX),
                hwnd = hwndOwner,
                lpVerb = (IntPtr)(cmd - CMD_FIRST),
                lpDirectory = p,
                lpVerbW = (IntPtr)(cmd - CMD_FIRST),
                lpDirectoryW = p,
                fMask = CMIC.UNICODE | CMIC.PTINVOKE |
                        (IsKeyPressControlKey ? CMIC.CONTROL_DOWN : 0) |
                        (IsKeyPressShiftKey ? CMIC.SHIFT_DOWN : 0),
                ptInvoke = new POINT(x, y),
                nShow = SW.SHOWNORMAL
            };

            contextMenu.InvokeCommand(ref command);
        }
    }
}

file class Hook : IDisposable
{
    private readonly TopLevel _topLevel;
    private readonly IContextMenu2? _contextMenu2;
    private readonly IContextMenu3? _contextMenu3;

    public Hook(TopLevel topLevel, IContextMenu2? contextMenu2, IContextMenu3? contextMenu3)
    {
        _topLevel = topLevel;
        _contextMenu2 = contextMenu2;
        _contextMenu3 = contextMenu3;

        Win32Properties.AddWndProcHookCallback(_topLevel, WndProc);
    }

    public void Dispose()
    {
        Win32Properties.RemoveWndProcHookCallback(_topLevel, WndProc);
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (_contextMenu2 is { })
        {
            if (msg is (uint)WM.INITMENUPOPUP or (uint)WM.MEASUREITEM or (uint)WM.DRAWITEM)
            {
                if (_contextMenu2.HandleMenuMsg(msg, wParam, lParam) is S_OK)
                {
                    handled = true;
                    return IntPtr.Zero;
                }
            }
        }

        if (_contextMenu3 is { })
        {
            if (msg is (uint)WM.MENUCHAR)
            {
                if (_contextMenu3.HandleMenuMsg2(msg, wParam, lParam, IntPtr.Zero) is S_OK)
                {
                    handled = true;
                    // ReSharper disable once DuplicatedStatements
                    return IntPtr.Zero;
                }
            }
        }

        return IntPtr.Zero;
    }
}

#endif
