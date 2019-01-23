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
        #region MICR constants

        public const short MICR_ST_OK = 0;
        public const short MICR_ST_UNKNOWN_ERROR = 1;
        public const short MICR_ST_BAD_PARAMETER = 2;
        public const short MICR_ST_PENDING = 3;
        public const short MICR_ST_PROCESS_CHECK_FAILED = 4;
        public const short MICR_ST_OVERFLOW = 5;
        public const short MICR_ST_DEVICE_NOT_FOUND = 6;
        public const short MICR_ST_ACCESS_DENIED = 7;
        public const short MICR_ST_DEVICE_NOT_RESPONDING = 8;
        public const short MICR_ST_DEVICE_NOT_READY = 9;
        public const short MICR_ST_HOPPER_EMPTY = 10;
        public const short MICR_ST_FILE_COPY_ERROR = 11;
        public const short MICR_ST_DEVICE_PROCESS_ERROR = 12;
        public const short MICR_ST_IMAGE_NOT_SPECIFIED = 13;
        public const short MICR_ST_IMAGE_NOT_FOUND = 14;
        public const short MICR_ST_NO_CHECKS_PRESENT = 15;
        public const short MICR_ST_NOT_ENOUGH_MEMORY = 16;
        public const short MICR_ST_KEY_NOT_FOUND = 17;
        public const short MICR_ST_SECTION_NOT_FOUND = 18;
        public const short MICR_ST_INVALID_SECTION = 19;
        public const short MICR_ST_INVALID_DATA = 20;
        public const short MICR_ST_FUNCTION_NOT_SUPPORTED = 21;
        public const short MICR_ST_MEMORY_ALLOCATION_PROBLEM = 22;
        public const short MICR_ST_REQUEST_TIMEDOUT = 23;
        public const short MICR_ST_QUERY_DATA_LENGTH_ERROR = 24;
        public const short MICR_ST_DEVICE_CONNECTION_ERROR = 25;
        public const short MICR_ST_DEVICE_NOT_OPEN = 26;
        public const short MICR_ST_ERR_GET_DOM_POINTER = 27;
        public const short MICR_ST_ERR_LOAD_XML = 28;
        public const short MICR_ST_KEY_NUMBER_NOT_FOUND = 29;
        public const short MICR_ST_ERR_INTERNET_CONNECT = 30;
        public const short MICR_ST_ERR_HTTP_OPEN_REQUEST = 31;
        public const short MICR_ST_ERR_HTTP_SEND_REQUEST = 32;
        public const short MICR_ST_ERR_CREATE_EVENT = 33;
        public const short MICR_ST_ERR_DOM_CREATE_NODE = 34;
        public const short MICR_ST_ERR_DOM_QUERY_INTERFACE = 35;
        public const short MICR_ST_ERR_DOM_ADD_KEY = 36;
        public const short MICR_ST_ERR_DOM_APPEND_CHILD = 37;
        public const short MICR_ST_ERR_DOM_GET_DOCUMENT_ELEMENT = 38;
        public const short MICR_ST_ERR_DOM_GET_XML = 39;
        public const short MICR_ST_ERR_DOM_GET_ITEM = 40;
        public const short MICR_ST_ERR_DOM_GET_CHILD_NODES = 41;
        public const short MICR_ST_ERR_DOM_GET_BASE_NAME = 42;
        public const short MICR_ST_ERR_DOM_GET_LENGTH = 43;
        public const short MICR_ST_ERR_DOM_GET_ELEMENT_BY_TAG_NAME = 44;
        public const short MICR_ST_ERR_DOM_GET_TEXT = 45;
        public const short MICR_ST_ERR_DOM_PUT_TEXT = 46;
        public const short MICR_ST_ERR_HTTP_QUERY_INFO = 47;
        public const short MICR_ST_INSUFFICIENT_DATA = 48;
        public const short MICR_ST_BAD_HTTP_CONNECTION = 49;
        public const short MICR_ST_CONTENT_ZERO_LENGTH = 50;
        public const short MICR_ST_BAD_DEVICE_NAME = 51;
        public const short MICR_ST_BAD_DATA = 52;
        public const short MICR_ST_BAD_SECTION_NAME = 53;
        public const short MICR_ST_BAD_KEY_NAME = 54;
        public const short MICR_ST_BAD_VALUE_BUFFER = 55;
        public const short MICR_ST_BAD_BUFFER_LENGTH = 56;
        public const short MICR_ST_BAD_QUERY_PARM = 57;
        public const short MICR_ST_BAD_IMAGE_NAME = 58;
        public const short MICR_ST_BAD_BUFFER = 59;
        public const short MICR_ST_BAD_BUFFER_SIZE = 60;
        public const short MICR_ST_CONNECT_REQUEST_TIMEDOUT = 61;
        public const short MICR_ST_INSUFFICIENT_DISKSPACE = 62;
        public const short MICR_ST_MSXML_FAILED = 63;
        public const short MICR_ST_QUERY_CONTENT_ERROR = 64;
        public const short MICR_ST_ERR_INTERNET_CONNECTION = 65;
        public const short MICR_ST_BAD_DEVICE_IP_OR_DOMAIN_NAME = 66;
        public const short MICR_ST_USB_GET_DATA_FAILED = 67;
        public const short MICR_ST_INET_GET_DATA_FAILED = 68;
        public const short MICR_ST_HTTP_HEADER_NOT_FOUND = 69;
        #endregion
    }
}
