using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Filedini.ServiceImplements.Windows;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static partial class WindowsNativeMethods
{
    public const int DBT_DEVICEARRIVAL = 0x8000;
    public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

    public const int MAX_PATH = 260;
    public const uint CMD_FIRST = 1;
    public const uint CMD_LAST = 30000;

    public const int S_OK = 0;
    
    internal const uint GENERIC_READ = 0x80000000;
    internal const uint FILE_SHARE_READ = 0x00000001;
    internal const uint OPEN_EXISTING = 3;
    internal const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

    internal const uint ERROR_NO_MORE_FILES = 18;
    internal const nint INVALID_HANDLE_VALUE = -1;
    
    internal const uint STATUS_SUCCESS = 0x00000000;
    internal const uint STATUS_NO_MORE_FILES = 0x80000006;
    
    internal const uint FileIdBothDirectoryInfo = 37;

    public static readonly Guid IID_IImageList = new("46EB5926-582E-4017-9FDF-E8998DAA0950");
    public static readonly Guid IID_IShellFolder = new("{000214E6-0000-0000-C000-000000000046}");
    public static readonly Guid IID_IContextMenu = new("{000214e4-0000-0000-c000-000000000046}");
    public static readonly Guid IID_IContextMenu2 = new("{000214f4-0000-0000-c000-000000000046}");
    public static readonly Guid IID_IContextMenu3 = new("{bcfce0a0-ec17-11d0-8d10-00a0c90f2719}");

    public const int VK_SHIFT = 0x10;
    public const int VK_CONTROL = 0x11;

    public enum FINDEX_INFO_LEVELS
    {
        FindExInfoStandard = 0,
        FindExInfoBasic = 1
    }

    public enum FINDEX_SEARCH_OPS
    {
        FindExSearchNameMatch = 0,
        FindExSearchLimitToDirectories = 1,
        FindExSearchLimitToDevices = 2
    }

    public const int FIND_FIRST_EX_LARGE_FETCH = 2;

    public const uint SHGFI_ICON = 0x100;
    public const uint SHGFI_SMALLICON = 0x01;
    public const uint SHGFI_LARGEICON = 0x00;
    public const uint SHIL_JUMBO = 0x4;
    public const uint SHGFI_SYSICONINDEX = 0x000004000;

    public const uint SHERB_NOCONFIRMATION = 0x00000001;
    public const uint SHERB_NOPROGRESSUI = 0x00000002;
    public const uint SHERB_NOSOUND = 0x00000004;

    [Flags]
    public enum SHCNF
    {
        SHCNF_IDLIST = 0x0000,
        SHCNF_PATHA = 0x0001,
        SHCNF_PRINTERA = 0x0002,
        SHCNF_DWORD = 0x0003,
        SHCNF_PATHW = 0x0005,
        SHCNF_PRINTERW = 0x0006,
        SHCNF_TYPE = 0x00FF,
        SHCNF_FLUSH = 0x1000,
        SHCNF_FLUSHNOWAIT = 0x2000,
    }

    [Flags]
    public enum SHCNE : uint
    {
        SHCNE_MEDIAINSERTED = 0x00000020,
        SHCNE_MEDIAREMOVED = 0x00000040,
    }

    [Flags]
    public enum FileOperationFlags : ushort
    {
        FOF_SILENT = 0x0004,
        FOF_NOCONFIRMATION = 0x0010,
        FOF_ALLOWUNDO = 0x0040,
        FOF_SIMPLEPROGRESS = 0x0100,
        FOF_NOERRORUI = 0x0400,
        FOF_WANTNUKEWARNING = 0x4000,
    }

    public enum FileOperationType : uint
    {
        FO_MOVE = 0x0001,
        FO_COPY = 0x0002,
        FO_DELETE = 0x0003,
        FO_RENAME = 0x0004,
    }

    [Flags]
    public enum SHGNO
    {
        NORMAL = 0x0000,
        INFOLDER = 0x0001,
        FOREDITING = 0x1000,
        FORADDRESSBAR = 0x4000,
        FORPARSING = 0x8000
    }

    [Flags]
    public enum SFGAO : uint
    {
        BROWSABLE = 0x8000000,
        CANCOPY = 1,
        CANDELETE = 0x20,
        CANLINK = 4,
        CANMONIKER = 0x400000,
        CANMOVE = 2,
        CANRENAME = 0x10,
        CAPABILITYMASK = 0x177,
        COMPRESSED = 0x4000000,
        CONTENTSMASK = 0x80000000,
        DISPLAYATTRMASK = 0xfc000,
        DROPTARGET = 0x100,
        ENCRYPTED = 0x2000,
        FILESYSANCESTOR = 0x10000000,
        FILESYSTEM = 0x40000000,
        FOLDER = 0x20000000,
        GHOSTED = 0x8000,
        HASPROPSHEET = 0x40,
        HASSTORAGE = 0x400000,
        HASSUBFOLDER = 0x80000000,
        HIDDEN = 0x80000,
        ISSLOW = 0x4000,
        LINK = 0x10000,
        NEWCONTENT = 0x200000,
        NONENUMERATED = 0x100000,
        READONLY = 0x40000,
        REMOVABLE = 0x2000000,
        SHARE = 0x20000,
        STORAGE = 8,
        STORAGEANCESTOR = 0x800000,
        STORAGECAPMASK = 0x70c50008,
        STREAM = 0x400000,
        VALIDATE = 0x1000000
    }

    [Flags]
    public enum SHCONTF
    {
        FOLDERS = 0x0020,
        NONFOLDERS = 0x0040,
        INCLUDEHIDDEN = 0x0080,
        INIT_ON_FIRST_NEXT = 0x0100,
        NETPRINTERSRCH = 0x0200,
        SHAREABLE = 0x0400,
        STORAGE = 0x0800,
    }

    [Flags]
    public enum CMF : uint
    {
        NORMAL = 0x00000000,
        DEFAULTONLY = 0x00000001,
        VERBSONLY = 0x00000002,
        EXPLORE = 0x00000004,
        NOVERBS = 0x00000008,
        CANRENAME = 0x00000010,
        NODEFAULT = 0x00000020,
        INCLUDESTATIC = 0x00000040,
        EXTENDEDVERBS = 0x00000100,
        RESERVED = 0xffff0000
    }

    [Flags]
    public enum GCS : uint
    {
        VERBA = 0,
        HELPTEXTA = 1,
        VALIDATEA = 2,
        VERBW = 4,
        HELPTEXTW = 5,
        VALIDATEW = 6
    }

    [Flags]
    public enum TPM : uint
    {
        LEFTBUTTON = 0x0000,
        RIGHTBUTTON = 0x0002,
        LEFTALIGN = 0x0000,
        CENTERALIGN = 0x0004,
        RIGHTALIGN = 0x0008,
        TOPALIGN = 0x0000,
        VCENTERALIGN = 0x0010,
        BOTTOMALIGN = 0x0020,
        HORIZONTAL = 0x0000,
        VERTICAL = 0x0040,
        NONOTIFY = 0x0080,
        RETURNCMD = 0x0100,
        RECURSE = 0x0001,
        HORPOSANIMATION = 0x0400,
        HORNEGANIMATION = 0x0800,
        VERPOSANIMATION = 0x1000,
        VERNEGANIMATION = 0x2000,
        NOANIMATION = 0x4000,
        LAYOUTRTL = 0x8000
    }

    [Flags]
    public enum CMIC : uint
    {
        HOTKEY = 0x00000020,
        ICON = 0x00000010,
        FLAG_NO_UI = 0x00000400,
        UNICODE = 0x00004000,
        NO_CONSOLE = 0x00008000,
        ASYNCOK = 0x00100000,
        NOZONECHECKS = 0x00800000,
        SHIFT_DOWN = 0x10000000,
        CONTROL_DOWN = 0x40000000,
        FLAG_LOG_USAGE = 0x04000000,
        PTINVOKE = 0x20000000
    }

    [Flags]
    public enum SW
    {
        HIDE = 0,
        SHOWNORMAL = 1,
        NORMAL = 1,
        SHOWMINIMIZED = 2,
        SHOWMAXIMIZED = 3,
        MAXIMIZE = 3,
        SHOWNOACTIVATE = 4,
        SHOW = 5,
        MINIMIZE = 6,
        SHOWMINNOACTIVE = 7,
        SHOWNA = 8,
        RESTORE = 9,
        SHOWDEFAULT = 10,
    }

    [Flags]
    public enum WM : uint
    {
        ACTIVATE = 0x6,
        ACTIVATEAPP = 0x1C,
        AFXFIRST = 0x360,
        AFXLAST = 0x37F,
        APP = 0x8000,
        ASKCBFORMATNAME = 0x30C,
        CANCELJOURNAL = 0x4B,
        CANCELMODE = 0x1F,
        CAPTURECHANGED = 0x215,
        CHANGECBCHAIN = 0x30D,
        CHAR = 0x102,
        CHARTOITEM = 0x2F,
        CHILDACTIVATE = 0x22,
        CLEAR = 0x303,
        CLOSE = 0x10,
        COMMAND = 0x111,
        COMPACTING = 0x41,
        COMPAREITEM = 0x39,
        CONTEXTMENU = 0x7B,
        COPY = 0x301,
        COPYDATA = 0x4A,
        CREATE = 0x1,
        CTLCOLORBTN = 0x135,
        CTLCOLORDLG = 0x136,
        CTLCOLOREDIT = 0x133,
        CTLCOLORLISTBOX = 0x134,
        CTLCOLORMSGBOX = 0x132,
        CTLCOLORSCROLLBAR = 0x137,
        CTLCOLORSTATIC = 0x138,
        CUT = 0x300,
        DEADCHAR = 0x103,
        DELETEITEM = 0x2D,
        DESTROY = 0x2,
        DESTROYCLIPBOARD = 0x307,
        DEVICECHANGE = 0x219,
        DEVMODECHANGE = 0x1B,
        DISPLAYCHANGE = 0x7E,
        DRAWCLIPBOARD = 0x308,
        DRAWITEM = 0x2B,
        DROPFILES = 0x233,
        ENABLE = 0xA,
        ENDSESSION = 0x16,
        ENTERIDLE = 0x121,
        ENTERMENULOOP = 0x211,
        ENTERSIZEMOVE = 0x231,
        ERASEBKGND = 0x14,
        EXITMENULOOP = 0x212,
        EXITSIZEMOVE = 0x232,
        FONTCHANGE = 0x1D,
        GETDLGCODE = 0x87,
        GETFONT = 0x31,
        GETHOTKEY = 0x33,
        GETICON = 0x7F,
        GETMINMAXINFO = 0x24,
        GETOBJECT = 0x3D,
        GETSYSMENU = 0x313,
        GETTEXT = 0xD,
        GETTEXTLENGTH = 0xE,
        HANDHELDFIRST = 0x358,
        HANDHELDLAST = 0x35F,
        HELP = 0x53,
        HOTKEY = 0x312,
        HSCROLL = 0x114,
        HSCROLLCLIPBOARD = 0x30E,
        ICONERASEBKGND = 0x27,
        IME_CHAR = 0x286,
        IME_COMPOSITION = 0x10F,
        IME_COMPOSITIONFULL = 0x284,
        IME_CONTROL = 0x283,
        IME_ENDCOMPOSITION = 0x10E,
        IME_KEYDOWN = 0x290,
        IME_KEYLAST = 0x10F,
        IME_KEYUP = 0x291,
        IME_NOTIFY = 0x282,
        IME_REQUEST = 0x288,
        IME_SELECT = 0x285,
        IME_SETCONTEXT = 0x281,
        IME_STARTCOMPOSITION = 0x10D,
        INITDIALOG = 0x110,
        INITMENU = 0x116,
        INITMENUPOPUP = 0x117,
        INPUTLANGCHANGE = 0x51,
        INPUTLANGCHANGEREQUEST = 0x50,
        KEYDOWN = 0x100,
        KEYFIRST = 0x100,
        KEYLAST = 0x108,
        KEYUP = 0x101,
        KILLFOCUS = 0x8,
        LBUTTONDBLCLK = 0x203,
        LBUTTONDOWN = 0x201,
        LBUTTONUP = 0x202,
        LVM_GETEDITCONTROL = 0x1018,
        LVM_SETIMAGELIST = 0x1003,
        MBUTTONDBLCLK = 0x209,
        MBUTTONDOWN = 0x207,
        MBUTTONUP = 0x208,
        MDIACTIVATE = 0x222,
        MDICASCADE = 0x227,
        MDICREATE = 0x220,
        MDIDESTROY = 0x221,
        MDIGETACTIVE = 0x229,
        MDIICONARRANGE = 0x228,
        MDIMAXIMIZE = 0x225,
        MDINEXT = 0x224,
        MDIREFRESHMENU = 0x234,
        MDIRESTORE = 0x223,
        MDISETMENU = 0x230,
        MDITILE = 0x226,
        MEASUREITEM = 0x2C,
        MENUCHAR = 0x120,
        MENUCOMMAND = 0x126,
        MENUDRAG = 0x123,
        MENUGETOBJECT = 0x124,
        MENURBUTTONUP = 0x122,
        MENUSELECT = 0x11F,
        MOUSEACTIVATE = 0x21,
        MOUSEFIRST = 0x200,
        MOUSEHOVER = 0x2A1,
        MOUSELAST = 0x20A,
        MOUSELEAVE = 0x2A3,
        MOUSEMOVE = 0x200,
        MOUSEWHEEL = 0x20A,
        MOVE = 0x3,
        MOVING = 0x216,
        NCACTIVATE = 0x86,
        NCCALCSIZE = 0x83,
        NCCREATE = 0x81,
        NCDESTROY = 0x82,
        NCHITTEST = 0x84,
        NCLBUTTONDBLCLK = 0xA3,
        NCLBUTTONDOWN = 0xA1,
        NCLBUTTONUP = 0xA2,
        NCMBUTTONDBLCLK = 0xA9,
        NCMBUTTONDOWN = 0xA7,
        NCMBUTTONUP = 0xA8,
        NCMOUSEHOVER = 0x2A0,
        NCMOUSELEAVE = 0x2A2,
        NCMOUSEMOVE = 0xA0,
        NCPAINT = 0x85,
        NCRBUTTONDBLCLK = 0xA6,
        NCRBUTTONDOWN = 0xA4,
        NCRBUTTONUP = 0xA5,
        NEXTDLGCTL = 0x28,
        NEXTMENU = 0x213,
        NOTIFY = 0x4E,
        NOTIFYFORMAT = 0x55,
        NULL = 0x0,
        PAINT = 0xF,
        PAINTCLIPBOARD = 0x309,
        PAINTICON = 0x26,
        PALETTECHANGED = 0x311,
        PALETTEISCHANGING = 0x310,
        PARENTNOTIFY = 0x210,
        PASTE = 0x302,
        PENWINFIRST = 0x380,
        PENWINLAST = 0x38F,
        POWER = 0x48,
        PRINT = 0x317,
        PRINTCLIENT = 0x318,
        QUERYDRAGICON = 0x37,
        QUERYENDSESSION = 0x11,
        QUERYNEWPALETTE = 0x30F,
        QUERYOPEN = 0x13,
        QUEUESYNC = 0x23,
        QUIT = 0x12,
        RBUTTONDBLCLK = 0x206,
        RBUTTONDOWN = 0x204,
        RBUTTONUP = 0x205,
        RENDERALLFORMATS = 0x306,
        RENDERFORMAT = 0x305,
        SETCURSOR = 0x20,
        SETFOCUS = 0x7,
        SETFONT = 0x30,
        SETHOTKEY = 0x32,
        SETICON = 0x80,
        SETMARGINS = 0xD3,
        SETREDRAW = 0xB,
        SETTEXT = 0xC,
        SETTINGCHANGE = 0x1A,
        SHOWWINDOW = 0x18,
        SIZE = 0x5,
        SIZECLIPBOARD = 0x30B,
        SIZING = 0x214,
        SPOOLERSTATUS = 0x2A,
        STYLECHANGED = 0x7D,
        STYLECHANGING = 0x7C,
        SYNCPAINT = 0x88,
        SYSCHAR = 0x106,
        SYSCOLORCHANGE = 0x15,
        SYSCOMMAND = 0x112,
        SYSDEADCHAR = 0x107,
        SYSKEYDOWN = 0x104,
        SYSKEYUP = 0x105,
        TCARD = 0x52,
        TIMECHANGE = 0x1E,
        TIMER = 0x113,
        TVM_GETEDITCONTROL = 0x110F,
        TVM_SETIMAGELIST = 0x1109,
        UNDO = 0x304,
        UNINITMENUPOPUP = 0x125,
        USER = 0x400,
        USERCHANGED = 0x54,
        VKEYTOITEM = 0x2E,
        VSCROLL = 0x115,
        VSCROLLCLIPBOARD = 0x30A,
        WINDOWPOSCHANGED = 0x47,
        WINDOWPOSCHANGING = 0x46,
        WININICHANGE = 0x1A,
        SH_NOTIFY = 0x0401,
    }
}