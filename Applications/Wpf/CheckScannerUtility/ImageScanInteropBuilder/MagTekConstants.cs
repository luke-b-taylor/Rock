using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageScanInteropBuilder
{
    public static class MagTekConstants
    {
        #region Win32 constants declaration
        public const UInt32 GENERIC_READ = 0x80000000;
        public const UInt32 GENERIC_WRITE = 0x40000000;
        public const short INVALID_HANDLE_VALUE = -1;
        public const uint CREATE_NEW = 3;
        public const uint CREATE_ALWAYS = 2;
        public const uint OPEN_EXISTING = 3;
        public const uint OPEN_ALWAYS = 4;
        public const uint FILE_SHARE_READ = 0x1;
        public const uint FILE_SHARE_WRITE = 0x2;
        public const uint FILE_SHARE_DELETE = 0x4;
        public const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        public const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;
        public const int SW_HIDE = 0;
        public const int SW_NORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_RESTORE = 9;
        #endregion
    }
}
