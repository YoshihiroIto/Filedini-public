#if TARGET_WINDOWS

using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Filedini.ServiceImplements;

public static partial class LockingProcessesHelper
{
    // ReSharper disable InconsistentNaming
    private const int CCH_RM_MAX_APP_NAME = 255;
    private const int CCH_RM_MAX_SVC_NAME = 63;
    private const int CCH_RM_SESSION_KEY = 32;
    private const int ERROR_SUCCESS = 0;
    private const int ERROR_MORE_DATA = 234;
    private const int STATUS_INFO_LENGTH_MISMATCH = unchecked((int)0xC0000004);
    private const int SYSTEM_EXTENDED_HANDLE_INFORMATION = 64;
    private const int OBJECT_NAME_INFORMATION = 1;
    private const int RESOURCES_PER_BATCH = 256;
    private const int STACKALLOC_PROCESS_INFO_BYTES = 16 * 1024;
    private const uint DUPLICATE_SAME_ACCESS = 0x2;
    private const uint PROCESS_DUP_HANDLE = 0x0040;
    private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
    private const uint FILE_TYPE_DISK = 0x0001;
    private const uint SE_PRIVILEGE_ENABLED = 0x00000002;

    [StructLayout(LayoutKind.Sequential)]
    private struct RM_UNIQUE_PROCESS
    {
        public int dwProcessId;
        public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private unsafe struct RM_PROCESS_INFO
    {
        public RM_UNIQUE_PROCESS Process;
        public fixed char strAppName[CCH_RM_MAX_APP_NAME + 1];
        public fixed char strServiceShortName[CCH_RM_MAX_SVC_NAME + 1];
        public int ApplicationType;
        public int AppStatus;
        public int TSSessionId;
        public int bRestartable;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX
    {
        public IntPtr Object;
        public IntPtr UniqueProcessId;
        public IntPtr HandleValue;
        public uint GrantedAccess;
        public ushort CreatorBackTraceIndex;
        public ushort ObjectTypeIndex;
        public uint HandleAttributes;
        public uint Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LUID
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LUID_AND_ATTRIBUTES
    {
        public LUID Luid;
        public uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TOKEN_PRIVILEGES
    {
        public uint PrivilegeCount;
        public LUID_AND_ATTRIBUTES Privileges;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct UNICODE_STRING
    {
        public ushort Length;
        public ushort MaximumLength;
        public IntPtr Buffer;
    }
    // ReSharper restore InconsistentNaming

    [LibraryImport("rstrtmgr.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static unsafe partial int RmStartSession(out uint pSessionHandle, int dwSessionFlags, char* strSessionKey);

    [LibraryImport("rstrtmgr.dll")]
    // ReSharper disable once UnusedMethodReturnValue.Local
    private static partial int RmEndSession(uint pSessionHandle);

    [LibraryImport("rstrtmgr.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int RmRegisterResources(
        uint pSessionHandle,
        uint nFiles,
        string[] rgsFilenames,
        uint nApplications,
        IntPtr rgApplications,
        uint nServices,
        IntPtr rgsServiceNames);

    [LibraryImport("rstrtmgr.dll")]
    private static partial int RmGetList(
        uint dwSessionHandle,
        out uint pnProcInfoNeeded,
        ref uint pnProcInfo,
        IntPtr rgAffectedApps,
        out uint lpdwRebootReasons);

    [LibraryImport("ntdll.dll")]
    private static partial int NtQuerySystemInformation(
        int systemInformationClass,
        IntPtr systemInformation,
        int systemInformationLength,
        out int returnLength);

    [LibraryImport("ntdll.dll")]
    private static partial int NtQueryObject(
        IntPtr objectHandle,
        int objectInformationClass,
        IntPtr objectInformation,
        int objectInformationLength,
        out int returnLength);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DuplicateHandle(
        IntPtr hSourceProcessHandle,
        IntPtr hSourceHandle,
        IntPtr hTargetProcessHandle,
        out IntPtr lpTargetHandle,
        uint dwDesiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
        uint dwOptions);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr OpenProcess(
        uint processAccess,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
        int processId);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    // ReSharper disable once UnusedMethodReturnValue.Local
    private static partial bool CloseHandle(IntPtr hObject);

    [LibraryImport("kernel32.dll")]
    private static partial IntPtr GetCurrentProcess();

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial uint GetFileType(IntPtr hFile);

    [LibraryImport("kernel32.dll", EntryPoint = "QueryDosDeviceW", SetLastError = true,
        StringMarshalling = StringMarshalling.Utf16)]
    private static unsafe partial uint QueryDosDevice(
        string? lpDeviceName,
        char* lpTargetPath,
        int ucchMax);

    [LibraryImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool OpenProcessToken(
        IntPtr processHandle,
        uint desiredAccess,
        out IntPtr tokenHandle);

    [LibraryImport("advapi32.dll", EntryPoint = "LookupPrivilegeValueW", SetLastError = true,
        StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool LookupPrivilegeValue(
        string? lpSystemName,
        string lpName,
        out LUID lpLuid);

    [LibraryImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    // ReSharper disable once UnusedMethodReturnValue.Local
    private static partial bool AdjustTokenPrivileges(
        IntPtr tokenHandle,
        [MarshalAs(UnmanagedType.Bool)] bool disableAllPrivileges,
        ref TOKEN_PRIVILEGES newState,
        uint bufferLength,
        IntPtr previousState,
        IntPtr returnLength);

    public static List<Process> GetLockingProcesses(string filePath)
        => GetLockingProcesses([filePath]);

    public static List<Process> GetLockingProcesses(IEnumerable<string> itemPaths)
    {
        var processes = new Dictionary<int, Process>();
        var normalizedTargets = NormalizeTargetPaths(itemPaths);
        if (normalizedTargets.Count is 0)
            return [];

        EnableDebugPrivilege();
        CollectProcessesFromRestartManager(normalizedTargets, processes);
        CollectProcessesFromHandles(normalizedTargets, processes);
        CollectProcessesFromModules(normalizedTargets, processes);

        return processes.Values.OrderBy(x => x.ProcessName).ThenBy(x => x.Id).ToList();
    }

    private static void CollectProcessesFromRestartManager(
        IReadOnlyCollection<string> normalizedTargets,
        IDictionary<int, Process> processes)
    {
        foreach (var batch in normalizedTargets.Chunk(RESOURCES_PER_BATCH))
            CollectProcessesFromRestartManagerBatch(batch, processes);
    }

    [SkipLocalsInit]
    private static void CollectProcessesFromRestartManagerBatch(
        string[] resources,
        IDictionary<int, Process> processes)
    {
        Span<char> key = stackalloc char[CCH_RM_SESSION_KEY + 1];
        uint handle;
        int res;
        unsafe
        {
            fixed (char* keyPtr = key)
            {
                res = RmStartSession(out handle, 0, keyPtr);
            }
        }

        if (res != ERROR_SUCCESS)
            return;

        try
        {
            res = RmRegisterResources(handle, (uint)resources.Length, resources, 0, IntPtr.Zero, 0, IntPtr.Zero);
            if (res != ERROR_SUCCESS)
                return;

            uint needed;
            uint count = 0;
            res = RmGetList(handle, out needed, ref count, IntPtr.Zero, out _);

            while (res == ERROR_MORE_DATA && needed > 0)
                res = ReadProcessesFromRestartManager(handle, needed, processes, out needed);
        }
        finally
        {
            RmEndSession(handle);
        }
    }

    private static unsafe int ReadProcessesFromRestartManager(
        uint handle,
        uint needed,
        IDictionary<int, Process> processes,
        out uint nextNeeded)
    {
        var infoCount = checked((int)needed);
        if ((long)infoCount * sizeof(RM_PROCESS_INFO) <= STACKALLOC_PROCESS_INFO_BYTES)
            return ReadProcessesFromRestartManagerStackalloc(handle, infoCount, processes, out nextNeeded);

        return ReadProcessesFromRestartManagerArrayPool(handle, infoCount, processes, out nextNeeded);
    }

    [SkipLocalsInit]
    private static unsafe int ReadProcessesFromRestartManagerStackalloc(
        uint handle,
        int infoCount,
        IDictionary<int, Process> processes,
        out uint nextNeeded)
    {
        Span<RM_PROCESS_INFO> infos = stackalloc RM_PROCESS_INFO[infoCount];
        return ReadProcessesFromRestartManagerCore(handle, infos, processes, out nextNeeded);
    }

    private static int ReadProcessesFromRestartManagerArrayPool(
        uint handle,
        int infoCount,
        IDictionary<int, Process> processes,
        out uint nextNeeded)
    {
        var rentedInfos = ArrayPool<RM_PROCESS_INFO>.Shared.Rent(infoCount);

        try
        {
            return ReadProcessesFromRestartManagerCore(handle, rentedInfos.AsSpan(0, infoCount), processes, out nextNeeded);
        }
        finally
        {
            ArrayPool<RM_PROCESS_INFO>.Shared.Return(rentedInfos);
        }
    }

    private static unsafe int ReadProcessesFromRestartManagerCore(
        uint handle,
        Span<RM_PROCESS_INFO> infos,
        IDictionary<int, Process> processes,
        out uint nextNeeded)
    {
        var count = checked((uint)infos.Length);

        fixed (RM_PROCESS_INFO* infosPtr = infos)
        {
            var res = RmGetList(handle, out nextNeeded, ref count, (IntPtr)infosPtr, out _);
            if (res != ERROR_SUCCESS)
                return res;

            for (var i = 0; i < count; i++)
                TryAddProcessById(infos[i].Process.dwProcessId, processes);

            return res;
        }
    }

    private static unsafe void CollectProcessesFromHandles(
        IReadOnlyCollection<string> normalizedTargets,
        IDictionary<int, Process> processes)
    {
        var currentProcess = GetCurrentProcess();
        var devicePathMap = BuildDevicePathMap();
        var processHandles = new Dictionary<int, IntPtr>();
        IntPtr buffer = IntPtr.Zero;

        try
        {
            var bufferLength = 64 * 1024;

            while (true)
            {
                buffer = Marshal.AllocHGlobal(bufferLength);
                var status = NtQuerySystemInformation(
                    SYSTEM_EXTENDED_HANDLE_INFORMATION,
                    buffer,
                    bufferLength,
                    out var returnLength);

                if (status == STATUS_INFO_LENGTH_MISMATCH)
                {
                    Marshal.FreeHGlobal(buffer);
                    buffer = IntPtr.Zero;
                    bufferLength = Math.Max(bufferLength * 2, returnLength);
                    continue;
                }

                if (status != 0)
                    return;

                var handleCount = Marshal.ReadIntPtr(buffer).ToInt64();
                var entrySize = sizeof(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX);
                var entriesPtr = IntPtr.Add(buffer, IntPtr.Size * 2);

                for (long i = 0; i < handleCount; i++)
                {
                    var entryPtr = IntPtr.Add(entriesPtr, checked((int)(i * entrySize)));
                    var entry = (*(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX*)entryPtr);
                    var processId = unchecked((int)entry.UniqueProcessId.ToInt64());
                    if (processId <= 0)
                        continue;

                    var sourceProcess = GetOrOpenProcessHandle(processHandles, processId);
                    if (sourceProcess == IntPtr.Zero)
                        continue;

                    if (DuplicateHandle(
                            sourceProcess,
                            entry.HandleValue,
                            currentProcess,
                            out var duplicatedHandle,
                            0,
                            false,
                            DUPLICATE_SAME_ACCESS) is false)
                        continue;

                    try
                    {
                        if (GetFileType(duplicatedHandle) != FILE_TYPE_DISK)
                            continue;

                        var handlePath = TryGetHandlePath(duplicatedHandle, devicePathMap);
                        if (string.IsNullOrEmpty(handlePath))
                            continue;

                        if (MatchesAnyTarget(handlePath, normalizedTargets) is false)
                            continue;

                        TryAddProcessById(processId, processes);
                    }
                    finally
                    {
                        CloseHandle(duplicatedHandle);
                    }
                }

                return;
            }
        }
        finally
        {
            foreach (var handle in processHandles.Values)
            {
                if (handle != IntPtr.Zero)
                    CloseHandle(handle);
            }

            if (buffer != IntPtr.Zero)
                Marshal.FreeHGlobal(buffer);
        }
    }

    private static void CollectProcessesFromModules(
        IReadOnlyCollection<string> normalizedTargets,
        IDictionary<int, Process> processes)
    {
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                foreach (ProcessModule module in process.Modules)
                {
                    var modulePath = NormalizePath(module.FileName);
                    if (string.IsNullOrEmpty(modulePath))
                        continue;

                    if (MatchesAnyTarget(modulePath, normalizedTargets))
                    {
                        processes.TryAdd(process.Id, process);
                        break;
                    }
                }
            }
            catch (InvalidOperationException)
            {
            }
            catch (Win32Exception)
            {
            }
        }
    }

    private static HashSet<string> NormalizeTargetPaths(IEnumerable<string> itemPaths)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var itemPath in itemPaths)
        {
            var normalizedPath = NormalizePath(itemPath);
            if (string.IsNullOrEmpty(normalizedPath))
                continue;

            result.Add(normalizedPath);

            if (Directory.Exists(normalizedPath) is false)
                continue;

            try
            {
                foreach (var childPath in Directory.EnumerateFileSystemEntries(
                             normalizedPath,
                             "*",
                             new EnumerationOptions
                             {
                                 RecurseSubdirectories = true,
                                 IgnoreInaccessible = true,
                                 ReturnSpecialDirectories = false
                             }))
                {
                    var normalizedChildPath = NormalizePath(childPath);
                    if (string.IsNullOrEmpty(normalizedChildPath) is false)
                        result.Add(normalizedChildPath);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        return result;
    }

    private static bool MatchesAnyTarget(string candidatePath, IReadOnlyCollection<string> normalizedTargets)
    {
        candidatePath = NormalizePath(candidatePath);

        foreach (var targetPath in normalizedTargets)
        {
            if (string.Equals(candidatePath, targetPath, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string NormalizePath(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return "";

            if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
                path = @"\\" + path[8..];
            else if (path.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
                path = path[4..];

            return Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return path;
        }
    }

    private static IntPtr GetOrOpenProcessHandle(IDictionary<int, IntPtr> processHandles, int processId)
    {
        if (processHandles.TryGetValue(processId, out var processHandle))
            return processHandle;

        processHandle = OpenProcess(PROCESS_DUP_HANDLE | PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
        processHandles[processId] = processHandle;
        return processHandle;
    }

    private static unsafe string? TryGetHandlePath(IntPtr handle, IReadOnlyDictionary<string, string> devicePathMap)
    {
        var bufferLength = 1024;
        IntPtr buffer = IntPtr.Zero;

        try
        {
            while (true)
            {
                buffer = Marshal.AllocHGlobal(bufferLength);
                var status = NtQueryObject(handle, OBJECT_NAME_INFORMATION, buffer, bufferLength, out var returnLength);

                if (status == STATUS_INFO_LENGTH_MISMATCH)
                {
                    Marshal.FreeHGlobal(buffer);
                    buffer = IntPtr.Zero;
                    bufferLength = Math.Max(bufferLength * 2, returnLength);
                    continue;
                }

                if (status != 0)
                    return null;

                var unicodeString = (*(UNICODE_STRING*)buffer);
                if (unicodeString.Buffer == IntPtr.Zero || unicodeString.Length == 0)
                    return null;

                var kernelPath = Marshal.PtrToStringUni(unicodeString.Buffer, unicodeString.Length / 2);
                if (string.IsNullOrWhiteSpace(kernelPath))
                    return null;

                return ConvertKernelPathToDosPath(kernelPath, devicePathMap);
            }
        }
        finally
        {
            if (buffer != IntPtr.Zero)
                Marshal.FreeHGlobal(buffer);
        }
    }

    private static void TryAddProcessById(int processId, IDictionary<int, Process> processes)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            processes.TryAdd(process.Id, process);
        }
        catch (ArgumentException)
        {
        }
    }

    private static void EnableDebugPrivilege()
    {
        const uint tokenAdjustPrivileges = 0x0020;

        try
        {
            if (OpenProcessToken(GetCurrentProcess(), tokenAdjustPrivileges, out var tokenHandle) is false)
                return;

            try
            {
                if (LookupPrivilegeValue(null, "SeDebugPrivilege", out var luid) is false)
                    return;

                var tokenPrivileges = new TOKEN_PRIVILEGES
                {
                    PrivilegeCount = 1,
                    Privileges = new LUID_AND_ATTRIBUTES
                    {
                        Luid = luid,
                        Attributes = SE_PRIVILEGE_ENABLED
                    }
                };

                AdjustTokenPrivileges(tokenHandle, false, ref tokenPrivileges, 0, IntPtr.Zero, IntPtr.Zero);
            }
            finally
            {
                CloseHandle(tokenHandle);
            }
        }
        catch
        {
            // Best-effort only. Continue scanning even when privilege elevation is unavailable.
        }
    }

    [SkipLocalsInit]
    private static Dictionary<string, string> BuildDevicePathMap()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Span<char> buffer = stackalloc char[1024];

        foreach (var drive in DriveInfo.GetDrives())
        {
            if (string.IsNullOrWhiteSpace(drive.Name) || drive.Name.Length < 2)
                continue;

            var driveName = drive.Name[..2];
            buffer.Clear();
            uint length;
            unsafe
            {
                fixed (char* bufferPtr = buffer)
                {
                    length = QueryDosDevice(driveName, bufferPtr, buffer.Length);
                }
            }

            if (length is 0)
                continue;

            var bufferLength = checked((int)length);
            var terminatorIndex = buffer[..bufferLength].IndexOf('\0');
            var devicePath = new string(buffer[..(terminatorIndex >= 0 ? terminatorIndex : bufferLength)]);
            if (string.IsNullOrWhiteSpace(devicePath))
                continue;

            result.TryAdd(devicePath, driveName);
        }

        return result;
    }

    private static string ConvertKernelPathToDosPath(string kernelPath,
        IReadOnlyDictionary<string, string> devicePathMap)
    {
        foreach (var pair in devicePathMap.OrderByDescending(x => x.Key.Length))
        {
            if (kernelPath.StartsWith(pair.Key, StringComparison.OrdinalIgnoreCase))
                return pair.Value + kernelPath[pair.Key.Length..];
        }

        return kernelPath;
    }
}

#endif
