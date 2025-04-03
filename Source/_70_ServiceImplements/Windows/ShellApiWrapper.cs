using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Filedini.ServiceImplements.Windows;

using static WindowsNativeMethods;

internal static class ShellApiWrapper
{
    public static bool EmptyTrashCan()
    {
        var result = SHEmptyRecycleBin(IntPtr.Zero,
            null,
            SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND);

        return result is 0;
    }

    public static unsafe bool SendToTrashCan(IEnumerable<string> targetFilePaths)
    {
        var pFrom = MakePathsString(targetFilePaths);

        fixed (char* p = pFrom)
        {
            var fileOp = new SHFILEOPSTRUCT
            {
                wFunc = FileOperationType.FO_DELETE,
                pFrom = p,
                fFlags = FileOperationFlags.FOF_ALLOWUNDO |
                         FileOperationFlags.FOF_NOCONFIRMATION |
                         FileOperationFlags.FOF_NOERRORUI |
                         FileOperationFlags.FOF_SILENT
            };

            var result = SHFileOperation(ref fileOp);

            return result is 0;
        }
    }

    public static (long ItemAllSize, long ItemCount) GetTrashCanInfo()
    {
        var info = new SHQUERYRBINFO();
        info.cbSize = Unsafe.SizeOf<SHQUERYRBINFO>();

        _ = SHQueryRecycleBin("", ref info);

        return (info.i64Size, info.i64NumItems);
    }

    private static string MakePathsString(IEnumerable<string> paths)
    {
        var sh = new DefaultInterpolatedStringHandler();

        foreach (var path in paths)
        {
            sh.AppendLiteral(path);
            sh.AppendLiteral("\0");
        }

        sh.AppendLiteral("\0");

        return sh.ToStringAndClear();
    }
}