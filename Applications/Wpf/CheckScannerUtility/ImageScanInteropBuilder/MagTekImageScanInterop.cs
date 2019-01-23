using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        #region Private Members
        private int m_nTotalDevice;
        private DocType m_nDocType;
        private string g_strAppPath = null;
        private int g_hLogFile;
        private string g_strLogFileName = null;

        private bool m_bDeviceSelected;
        private bool m_bRNDIS;
        private int m_nCurrentDevice;
        private bool m_bDeviceOpened;
        private string m_strCurrentDeviceName;
        private string m_strQueryItem;

        private string[] m_arrQueryOptions = { "DeviceCapabilities", "DeviceStatus", "DeviceUsage" };
        private string[] m_arrFeederOptions = { "CHECK", "MSR" };

        private string m_strOptions;
        private string m_strDocInfo;
        private List<string> _deviceListNames;

        #endregion

        #region Public Members
        public string AppPath
        {
            get { return g_strAppPath; }
            set { g_strAppPath = value; }
        }
        public string CurrentDeviceName
        {
            get
            {
                return m_strCurrentDeviceName;
            }
            set
            {
                m_strCurrentDeviceName = value;
            }
        }
        public DocType DocType
        {
            get { return m_nDocType; }
        }
        public string DocInfo
        {
            get
            {
                return m_strDocInfo;
            }
            set
            {
                m_strDocInfo = value;
            }
        }
        public string MicrData { get; private set; }
        public string Routing { get; private set; }
        public string AcctNumber { get; private set; }
        public string CheckNumber { get; private set; }
        public string Options
        {
            get { return m_strOptions; }
            set { m_strOptions = value; }
        }
        public bool OpenDeviceEnabled { get; private set; }
        public bool QueryDeviceEnabled { get; private set; }
        public bool ProcessDocumentEnabled { get; private set; }
        public StringBuilder sbLogText { get; private set; }
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
        #endregion
        #region Public Methods
        public bool OpenDevice( string strDeviceName )
        {
            m_bDeviceOpened = ( MTMICROpenDevice( strDeviceName ) == MagTekConstants.MICR_ST_OK );
            return m_bDeviceOpened;
        }

        public bool CloseDevice( string strDevicename )
        {
            return ( MTMICRCloseDevice( strDevicename ) == MagTekConstants.MICR_ST_OK );
        }


        public bool DeviceOpened
        {
            get { return m_bDeviceOpened; }
        }

        public int MICRSetValue( StringBuilder strOptions, string strSection, string strKey, string strValue, ref int nActualLength )
        {   
            return MTMICRSetValue( strOptions, strSection, strKey, strValue, ref nActualLength );
        }

        public int MICRSetIndexValue( StringBuilder strOptions, string strSection, string strKey, int nIndex, string strValue, ref int nActualLength )
        {
            return MTMICRSetIndexValue( strOptions, strSection, strKey, nIndex, strValue, ref nActualLength );
        }

        public int MICRGetIndexValue( string strDocInfo, string strSection, string strKey, int nIndex, StringBuilder strResponse, ref int nResponseLength )
        {
            return MTMICRGetIndexValue( strDocInfo, strSection, strKey, nIndex, strResponse, ref nResponseLength );
        }

        public int MICRProcessCheck( string strDeviceName, string strOptions, StringBuilder strResponse, ref int nResponseLength )
        {
            return MTMICRProcessCheck( strDeviceName, strOptions, strResponse, ref nResponseLength );
        }

        public int MICRGetValue( string strDocInfo, string strSection, string strKey, StringBuilder strResponse, ref int nResponseLength )
        {
            return MTMICRGetValue(strDocInfo,strSection, strKey, strResponse, ref nResponseLength );
        }

        public int MICRGetImage( string strDeviceName, string strImageID, byte[] imageBuf, ref int nBufLength )
        {
            return MTMICRGetImage(strDeviceName,strImageID,imageBuf, ref nBufLength );
        }

        public int MICRCreateFile( string lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode, UInt32 lpSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, int hTemplateFile ) {
            return CreateFile(lpFileName,dwDesiredAccess, dwShareMode, lpSecurityAttributes,dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile );
        }

        public bool MICRWriteFile( int hFile, byte[] pBuf, int nBytesToWrite, ref int nBytesWritten, IntPtr pOverlapped )
        {return WriteFile(hFile,pBuf, nBytesToWrite, ref nBytesWritten, pOverlapped );
        }
        #endregion        
        #region DLL Import

        #region Win32 DLL
        [DllImport( "kernel32.dll", SetLastError = true )]
        public static extern unsafe int CreateFile( string lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode,UInt32 lpSecurityAttributes, UInt32 dwCreationDisposition,UInt32 dwFlagsAndAttributes, int hTemplateFile );


        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern unsafe bool WriteFile( int hFile, byte[] pBuf, int nBytesToWrite, ref int nBytesWritten, IntPtr pOverlapped );
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

        public void Init( string executablePath )
        {
            m_nDocType = DocType.CHECK;
            m_bRNDIS = false;
            m_bDeviceSelected = false;
            m_bDeviceOpened = false;
            m_nTotalDevice = 0;

            g_strAppPath = Path.GetDirectoryName( executablePath );
            PrintStatus( g_strAppPath );
            PrintStatus( g_strLogFileName );

            OpenDeviceEnabled = false;
            QueryDeviceEnabled = false;
            ProcessDocumentEnabled = false;

            m_strCurrentDeviceName = "";
            m_strQueryItem = "DeviceCapabilities";
            m_strOptions = new string( '\0', 4096 );
            Setup();

        }

        public void ProcessInit()
        {
            this.Options = "";
            this.DocInfo = "";
            this.MicrData = "";
            this.Routing = "";
            this.AcctNumber = "";
            this.CheckNumber = "";
        }

        private void Setup()
        {
            GetDeviceList();
            SetupLogging();


        }
        private void GetDeviceList()
        {
            byte nTotalDev = 1;

            try
            {
                string strLogTxt = "Enumerating Configured Devices...";
                PrintStatus( strLogTxt );
                string strDeviceName;
                StringBuilder str1 = new StringBuilder();
                str1.Capacity = 256;
                int nRetCode = MagTekConstants.MICR_ST_DEVICE_NOT_FOUND;
                nRetCode = MTMICRGetDevice( nTotalDev++, str1 );
                if ( nRetCode != MagTekConstants.MICR_ST_DEVICE_NOT_FOUND )
                    PrintStatus( "Found Device(s) :" );
                while ( nRetCode != MagTekConstants.MICR_ST_DEVICE_NOT_FOUND )
                {
                    strDeviceName = str1.ToString();
                    if ( strDeviceName.Length > 1 )
                    {
                        PrintStatus( strDeviceName );
                        this.DeviceListNames.Add( strDeviceName );
                        //comboDeviceName.Items.Add( strDeviceName );
                        m_nTotalDevice++;
                    }
                    nRetCode = MagTekConstants.MICR_ST_DEVICE_NOT_FOUND;
                    nRetCode = MTMICRGetDevice( nTotalDev++, str1 );
                }
            }
            catch ( NullReferenceException e )
            {
                Trace.WriteLine( "Caught error: {0}.", e.ToString() );
            }
        }

        private void SetupLogging()
        {

            g_strLogFileName = g_strAppPath + "\\ExcellaLog.txt";

            Trace.WriteLine( g_strLogFileName );

            g_hLogFile = -1;

            g_hLogFile = CreateFile( g_strLogFileName, GENERIC_READ | GENERIC_WRITE,
                FILE_SHARE_READ | FILE_SHARE_WRITE, 0, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, 0 );

            if ( g_hLogFile > 0 )
            {
                ///        MTMICRSetLogEnable(1);
                MTMICRSetLogFileHandle( g_hLogFile );
            }
        }

        public void PrintStatus( string strText )
        {
            if ( strText != null )
            {
                if ( sbLogText == null )
                {
                    sbLogText = new StringBuilder();
                }
                sbLogText.Append( strText );
                sbLogText.Append( "\r\n" );
            }
        }
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

        public List<String> DeviceListNames
        {
            get
            {
                if ( _deviceListNames == null )
                {
                    _deviceListNames = new List<string>();
                }
                return _deviceListNames;
            }
        }


    }
}
