using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;

namespace Filedini.ServiceImplements;

internal static class WindowsNativeExtensions
{
    extension(FILETIME time)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime ToDateTime()
        {
            var fileTime = ((long)time.dwHighDateTime << 32) | (uint)time.dwLowDateTime;
            return DateTime.FromFileTime(fileTime);
        }
    }
}