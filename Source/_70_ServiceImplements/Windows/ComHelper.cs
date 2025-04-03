using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Filedini.ServiceImplements.Windows;

internal static class ComHelper
{
    private static readonly StrategyBasedComWrappers ComWrappers = new ();

    public static T GetOrCreateObjectForComInstance<T>(nint externalComObject) where T : class
    {
        return (T)ComWrappers.GetOrCreateObjectForComInstance(externalComObject, CreateObjectFlags.None);
    }
}