using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;

namespace Filedini.ServiceImplements;

internal static class WindowsNativeHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ToDateTime(this FILETIME time)
    {
        var fileTime = ((long)time.dwHighDateTime << 32) | (uint)time.dwLowDateTime;
        return DateTime.FromFileTime(fileTime);
    }
}