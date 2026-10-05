#if TARGET_WINDOWS

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

// ReSharper disable once CheckNamespace
namespace Filedini.ServiceImplements.Windows;

internal sealed class ComObjectScope : IDisposable
{
    private static readonly StrategyBasedComWrappers ComWrappers = new();
    private readonly List<ComObject> _objects = [];
    private bool _disposed;

    // Consumes one caller-owned native reference, including when wrapping fails.
    public T GetObjectForOwnedReference<T>(nint externalComObject) where T : class
    {
        ComObject? wrapper = null;
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            // FinalRelease is only effective for unique instances. Never release
            // a cached wrapper that another menu or COM consumer may still use.
            wrapper = (ComObject)ComWrappers.GetOrCreateObjectForComInstance(
                externalComObject, CreateObjectFlags.UniqueInstance);
            var result = (T)(object)wrapper;
            _objects.Add(wrapper);
            wrapper = null;
            return result;
        }
        finally
        {
            try
            {
                wrapper?.FinalRelease();
            }
            finally
            {
                // The wrapper owns its own AddRef; it does not adopt the input.
                if (externalComObject != 0)
                    Marshal.Release(externalComObject);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        for (var i = _objects.Count - 1; i >= 0; i--)
            _objects[i].FinalRelease();
        _objects.Clear();
    }
}

#endif
