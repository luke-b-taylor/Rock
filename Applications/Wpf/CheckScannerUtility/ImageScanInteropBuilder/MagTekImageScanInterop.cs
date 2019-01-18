using System;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Used to interact with MagTek Image Scanner USB
/// DllImport is used for consuming a UnManged Code 
/// </summary>
namespace ImageScanInteropBuilder
{
   public class MagTekImageScanInterop
    {
        #region DLL Import

        #region Win32 DLL
        [DllImport( "kernel32.dll", SetLastError = true )]
        public static extern unsafe int CreateFile( string lpFileName,
                 UInt32 dwDesiredAccess, UInt32 dwShareMode,
                 UInt32 lpSecurityAttributes, UInt32 dwCreationDisposition,
                 UInt32 dwFlagsAndAttributes, int hTemplateFile );


        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern unsafe bool WriteFile( int hFile,
              byte[] pBuf, int nBytesToWrite, ref int nBytesWritten, IntPtr pOverlapped );
        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern unsafe bool CloseHandle( int hHandle );
        [DllImport( "shell32.dll", CharSet = CharSet.Ansi )]
        static extern bool ShellExecuteEx( ref SHELLEXECUTEINFO lpExecInfo );


        #endregion
        #region MTXMLMCR.DLL
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        static extern unsafe int MTMICRGetImage( string strDeviceName, string strImageID, byte[] imageBuf, ref int nBufLength );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        static extern unsafe int MTMICRGetDevice( int dwDeviceContext, StringBuilder strDeviceName );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        static extern unsafe int MTMICRQueryInfo( string strDeviceName, string strQueryParm, StringBuilder strResponse, ref int nResponseLength );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        static extern unsafe int MTMICRSendCommand( string strDeviceName, string strSendParm, StringBuilder strResponse, ref int nResponseLength );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        static extern unsafe int MTMICRSetValue( StringBuilder strOptions, string strSection, string strKey, string strValue, ref int nActualLength );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        static extern unsafe int MTMICRGetValue( string strDocInfo, string strSection, string strKey, StringBuilder strResponse, ref int nResponseLength );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        static extern unsafe int MTMICRSetIndexValue( StringBuilder strOptions, string strSection, string strKey, int nIndex, string strValue, ref int nActualLength );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        static extern unsafe int MTMICRGetIndexValue( string strDocInfo, string strSection, string strKey, int nIndex, StringBuilder strResponse, ref int nResponseLength );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        static extern unsafe int MTMICRProcessCheck( string strDeviceName, string strOptions, StringBuilder strResponse, ref int nResponseLength );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        static extern unsafe int MTMICRSetLogFileHandle( Int32 hLogHandle );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        static extern unsafe int MTMICRSetLogEnable( int nEnable );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        static extern unsafe int MTMICROpenDevice( string strDeviceName );
        [DllImport( "mtxmlmcr.dll", SetLastError = true )]
        static extern unsafe int MTMICRCloseDevice( string strDeviceName );
        #endregion
        #endregion

        #region Win32 constants declaration

        const UInt32 GENERIC_READ = 0x80000000;
        const UInt32 GENERIC_WRITE = 0x40000000;
        const short INVALID_HANDLE_VALUE = -1;
        const uint CREATE_NEW = 3;
        const uint CREATE_ALWAYS = 2;
        const uint OPEN_EXISTING = 3;
        const uint OPEN_ALWAYS = 4;
        const uint FILE_SHARE_READ = 0x1;
        const uint FILE_SHARE_WRITE = 0x2;
        const uint FILE_SHARE_DELETE = 0x4;
        const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;


        const int SW_HIDE = 0;
        const int SW_NORMAL = 1;
        const int SW_SHOWMINIMIZED = 2;
        const int SW_MAXIMIZE = 3;
        const int SW_SHOWNOACTIVATE = 4;
        const int SW_SHOW = 5;
        const int SW_MINIMIZE = 6;
        const int SW_SHOWMINNOACTIVE = 7;
        const int SW_SHOWNA = 8;
        const int SW_RESTORE = 9;


        #endregion

        #region MICR constants




        const short MICR_ST_OK = 0;
        const short MICR_ST_UNKNOWN_ERROR = 1;
        const short MICR_ST_BAD_PARAMETER = 2;
        const short MICR_ST_PENDING = 3;
        const short MICR_ST_PROCESS_CHECK_FAILED = 4;
        const short MICR_ST_OVERFLOW = 5;
        const short MICR_ST_DEVICE_NOT_FOUND = 6;
        const short MICR_ST_ACCESS_DENIED = 7;
        const short MICR_ST_DEVICE_NOT_RESPONDING = 8;
        const short MICR_ST_DEVICE_NOT_READY = 9;
        const short MICR_ST_HOPPER_EMPTY = 10;
        const short MICR_ST_FILE_COPY_ERROR = 11;
        const short MICR_ST_DEVICE_PROCESS_ERROR = 12;
        const short MICR_ST_IMAGE_NOT_SPECIFIED = 13;
        const short MICR_ST_IMAGE_NOT_FOUND = 14;
        const short MICR_ST_NO_CHECKS_PRESENT = 15;
        const short MICR_ST_NOT_ENOUGH_MEMORY = 16;
        const short MICR_ST_KEY_NOT_FOUND = 17;
        const short MICR_ST_SECTION_NOT_FOUND = 18;
        const short MICR_ST_INVALID_SECTION = 19;
        const short MICR_ST_INVALID_DATA = 20;
        const short MICR_ST_FUNCTION_NOT_SUPPORTED = 21;
        const short MICR_ST_MEMORY_ALLOCATION_PROBLEM = 22;
        const short MICR_ST_REQUEST_TIMEDOUT = 23;
        const short MICR_ST_QUERY_DATA_LENGTH_ERROR = 24;
        const short MICR_ST_DEVICE_CONNECTION_ERROR = 25;
        const short MICR_ST_DEVICE_NOT_OPEN = 26;
        const short MICR_ST_ERR_GET_DOM_POINTER = 27;
        const short MICR_ST_ERR_LOAD_XML = 28;
        const short MICR_ST_KEY_NUMBER_NOT_FOUND = 29;
        const short MICR_ST_ERR_INTERNET_CONNECT = 30;
        const short MICR_ST_ERR_HTTP_OPEN_REQUEST = 31;
        const short MICR_ST_ERR_HTTP_SEND_REQUEST = 32;
        const short MICR_ST_ERR_CREATE_EVENT = 33;
        const short MICR_ST_ERR_DOM_CREATE_NODE = 34;
        const short MICR_ST_ERR_DOM_QUERY_INTERFACE = 35;
        const short MICR_ST_ERR_DOM_ADD_KEY = 36;
        const short MICR_ST_ERR_DOM_APPEND_CHILD = 37;
        const short MICR_ST_ERR_DOM_GET_DOCUMENT_ELEMENT = 38;
        const short MICR_ST_ERR_DOM_GET_XML = 39;
        const short MICR_ST_ERR_DOM_GET_ITEM = 40;
        const short MICR_ST_ERR_DOM_GET_CHILD_NODES = 41;
        const short MICR_ST_ERR_DOM_GET_BASE_NAME = 42;
        const short MICR_ST_ERR_DOM_GET_LENGTH = 43;
        const short MICR_ST_ERR_DOM_GET_ELEMENT_BY_TAG_NAME = 44;
        const short MICR_ST_ERR_DOM_GET_TEXT = 45;
        const short MICR_ST_ERR_DOM_PUT_TEXT = 46;
        const short MICR_ST_ERR_HTTP_QUERY_INFO = 47;
        const short MICR_ST_INSUFFICIENT_DATA = 48;
        const short MICR_ST_BAD_HTTP_CONNECTION = 49;
        const short MICR_ST_CONTENT_ZERO_LENGTH = 50;
        const short MICR_ST_BAD_DEVICE_NAME = 51;
        const short MICR_ST_BAD_DATA = 52;
        const short MICR_ST_BAD_SECTION_NAME = 53;
        const short MICR_ST_BAD_KEY_NAME = 54;
        const short MICR_ST_BAD_VALUE_BUFFER = 55;
        const short MICR_ST_BAD_BUFFER_LENGTH = 56;
        const short MICR_ST_BAD_QUERY_PARM = 57;
        const short MICR_ST_BAD_IMAGE_NAME = 58;
        const short MICR_ST_BAD_BUFFER = 59;
        const short MICR_ST_BAD_BUFFER_SIZE = 60;
        const short MICR_ST_CONNECT_REQUEST_TIMEDOUT = 61;
        const short MICR_ST_INSUFFICIENT_DISKSPACE = 62;
        const short MICR_ST_MSXML_FAILED = 63;
        const short MICR_ST_QUERY_CONTENT_ERROR = 64;
        const short MICR_ST_ERR_INTERNET_CONNECTION = 65;
        const short MICR_ST_BAD_DEVICE_IP_OR_DOMAIN_NAME = 66;

        const short MICR_ST_USB_GET_DATA_FAILED = 67;
        const short MICR_ST_INET_GET_DATA_FAILED = 68;
        const short MICR_ST_HTTP_HEADER_NOT_FOUND = 69;
        #endregion

        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            public String lpVerb;
            public String lpFile;
            public String lpParameters;
            public String lpDirectory;
            public int nShow;
            public int hInstApp;
            public int lpIDList;
            public String lpClass;
            public int hkeyClass;
            public uint dwHotKey;
            public int hIcon;
            public int hProcess;
        }
    }
}
