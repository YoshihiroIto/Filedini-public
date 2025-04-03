using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using CommunityToolkit.Diagnostics;

namespace Filedini.ServiceImplements.Windows;

using static WindowsNativeMethods;
using static ComHelper;

internal sealed class ShellContextMenu
{
    public static void Show(TopLevel topLevel, FileInfo[] files, int x, int y)
    {
        Guard.IsNotEmpty(files);

        var handleOwner = topLevel.TryGetPlatformHandle()?.Handle;
        if (handleOwner is null)
            return;

        IntPtr[] idls = [];
        var hook = default(Hook);
        var menu = IntPtr.Zero;

        try
        {
            var desktopFolder = CreateDesktopFolder();
            var (parentFolder, parentFolderName) = CreateParentFolder(files[0].DirectoryName ?? "", desktopFolder);

            idls = CreateIdls(files, parentFolder);

            var (contextMenu, contextMenu2, contextMenu3) = GetContextMenuInterfaces(idls, parentFolder);
            menu = CreatePopupMenu();

            contextMenu.QueryContextMenu(menu, 0, CMD_FIRST, CMD_LAST,
                CMF.EXPLORE | CMF.NORMAL | (IsKeyPressShiftKey ? CMF.EXTENDEDVERBS : 0));

            hook = new Hook(topLevel, contextMenu2, contextMenu3);

            var selectedIndex = TrackPopupMenuEx(
                menu, TPM.RETURNCMD, x, y, (IntPtr)handleOwner, IntPtr.Zero);

            if (selectedIndex >= CMD_FIRST)
                InvokeCommand(contextMenu, selectedIndex, parentFolderName, x, y);
        }
        finally
        {
            hook?.Dispose();

            if (menu != IntPtr.Zero)
                DestroyMenu(menu);

            FreeIdls(idls);
        }
    }

    private static (IContextMenu, IContextMenu2?, IContextMenu3?)
        GetContextMenuInterfaces(IntPtr[] idls, IShellFolder parentFolder)
    {
        var result = parentFolder.GetUIObjectOf(
            IntPtr.Zero, (uint)idls.Length, idls, in IID_IContextMenu, IntPtr.Zero, out var unknownContextMenu);

        if (result is not S_OK)
            throw new InvalidOperationException();

        var contextMenu = GetOrCreateObjectForComInstance<IContextMenu>(unknownContextMenu);

        IContextMenu2? contextMenu2 = null;
        IContextMenu3? contextMenu3 = null;

        if (Marshal.QueryInterface(unknownContextMenu, in IID_IContextMenu2, out var unknownContextMenu2) is S_OK)
            contextMenu2 = GetOrCreateObjectForComInstance<IContextMenu2>(unknownContextMenu2);

        if (Marshal.QueryInterface(unknownContextMenu, in IID_IContextMenu3, out var unknownContextMenu3) is S_OK)
            contextMenu3 = GetOrCreateObjectForComInstance<IContextMenu3>(unknownContextMenu3);

        return (contextMenu, contextMenu2, contextMenu3);
    }

    private static IShellFolder CreateDesktopFolder()
    {
        var result = SHGetDesktopFolder(out var unknownDesktopFolder);
        if (result is not S_OK)
            throw new InvalidOperationException();

        return GetOrCreateObjectForComInstance<IShellFolder>(unknownDesktopFolder);
    }

    private static (IShellFolder, string) CreateParentFolder(string folderName, IShellFolder desktopFolder)
    {
        var pchEaten = 0u;
        var pdwAttributes = default(SFGAO);

        var result = desktopFolder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, folderName, ref pchEaten,
            out var idl, ref pdwAttributes);

        if (result is not S_OK)
            throw new InvalidOperationException();

        var folder = Marshal.AllocCoTaskMem(MAX_PATH * 2 + 4);
        Marshal.WriteInt32(folder, 0, 0);

        _ = desktopFolder.GetDisplayNameOf(idl, SHGNO.FORPARSING, folder);

        var name = StrRetToBufWrap(folder, idl);
        result = desktopFolder.BindToObject(idl, IntPtr.Zero, in IID_IShellFolder, out var unknownParentFolder);

        Marshal.FreeCoTaskMem(folder);
        Marshal.FreeCoTaskMem(idl);

        if (result is not S_OK)
            throw new InvalidOperationException();

        return (GetOrCreateObjectForComInstance<IShellFolder>(unknownParentFolder), name);
    }

    private static IntPtr[] CreateIdls(FileInfo[] files, IShellFolder parentFolder)
    {
        var idls = new IntPtr[files.Length];

        for (var i = 0; i != files.Length; ++i)
        {
            var pchEaten = 0u;
            var pdwAttributes = default(SFGAO);

            var result = parentFolder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, files[i].Name, ref pchEaten,
                out var idl, ref pdwAttributes);

            if (result is not S_OK)
                throw new InvalidOperationException();

            idls[i] = idl;
        }

        return idls;
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

    private static unsafe void InvokeCommand(IContextMenu contextMenu, uint cmd, string folderName, int x, int y)
    {
        fixed (char* p = folderName)
        {
            var command = new CMINVOKECOMMANDINFOEX
            {
                cbSize = sizeof(CMINVOKECOMMANDINFOEX),
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

    [SkipLocalsInit]
    private static unsafe string StrRetToBufWrap(IntPtr target, IntPtr idl)
    {
        Span<char> buffer = stackalloc char[MAX_PATH + 16];

        fixed (char* p = buffer)
        {
            _ = StrRetToBuf(target, idl, (IntPtr)p, MAX_PATH);

            var len = buffer.IndexOf('\0');
            return buffer[..len].ToString();
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
                    return IntPtr.Zero;
                }
            }
        }

        return IntPtr.Zero;
    }
}