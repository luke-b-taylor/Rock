using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
namespace ImageScanInteropBuilder
{
    /// <summary>
    /// Used to interact with MagTek Image Scanner USB
    /// DllImport is used for consuming a UnManged Code
    /// Example provided by https://www.magtek.com/support/excella
    /// Windows API programming manual for Excella and Excella STX.
    /// </summary>
    public class MagTekImageScanInterop
    {
        #region Private Members
        private int m_nTotalDevice;
        private int g_hLogFile;
        private string g_strLogFileName = null;

        private string[] m_arrQueryOptions = { "DeviceCapabilities", "DeviceStatus", "DeviceUsage" };
        private string[] m_arrFeederOptions = { "CHECK", "MSR" };
        private List<string> _deviceListNames;

        #endregion

        #region Public Members
        public StringBuilder Errors = new StringBuilder();
        public string AppPath { get; set; } = null;
        public string CurrentDeviceName { get; set; }
        public DocType DocType { get; private set; }
        public string DocInfo { get; set; }
        public string MicrData { get; private set; }
        public string Routing { get; private set; }
        public string AcctNumber { get; private set; }
        public string CheckNumber { get; private set; }
        public string Options { get; set; }
        public bool OpenDeviceEnabled { get; private set; }
        public bool QueryDeviceEnabled { get; private set; }
        public bool ProcessDocumentEnabled { get; private set; }
        public StringBuilder sbLogText { get; private set; }
        #endregion
        #region Public Methods
        public bool OpenDevice( string strDeviceName )
        {  
           var status = MTMICROpenDevice( strDeviceName );
            this.SetErrorOnStatus( status );
            return this.DeviceOpened;

        }

        public bool CloseDevice( string strDevicename )
        {
             var status = MTMICRCloseDevice( strDevicename );
            return DeviceClosed;
        }

        private void SetErrorOnStatus( int status )
        {
            if ( status == ( int ) ImageScanStatus.MICR_ST_OK )
            {
                this.BDeviceOpened = true;
            }
            else
            {
                Errors.AppendLine( string.Format( "Device Return Status of {0}", ( ImageScanStatus ) status ).ToString() );
                this.BDeviceOpened = false;
            }
        }
        public bool DeviceOpened
        {
            get { return BDeviceOpened; }
        }
        public bool DeviceClosed
        {
            get { return !BDeviceOpened; }
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

        public int MICRQueryInfo( string strDeviceName, string strQueryParm, StringBuilder strResponse, ref int nResponseLength )
        {
            return MTMICRQueryInfo(strDeviceName, strQueryParm, strResponse, ref nResponseLength );
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
            DocType = DocType.CHECK;
            BDeviceOpened = false;
            m_nTotalDevice = 0;

            AppPath = Path.GetDirectoryName( executablePath );
            PrintStatus( AppPath );
            PrintStatus( g_strLogFileName );

            OpenDeviceEnabled = false;
            QueryDeviceEnabled = false;
            ProcessDocumentEnabled = false;
            CurrentDeviceName = "";
            Options = new string( '\0', 4096 );
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
                int nRetCode =(int)ImageScanStatus.MICR_ST_DEVICE_NOT_FOUND;
                nRetCode = MTMICRGetDevice( nTotalDev++, str1 );
                if ( nRetCode != ( int ) ImageScanStatus.MICR_ST_DEVICE_NOT_FOUND )
                    PrintStatus( "Found Device(s) :" );
                while ( nRetCode != (int) ImageScanStatus.MICR_ST_DEVICE_NOT_FOUND )
                {
                    strDeviceName = str1.ToString();
                    if ( strDeviceName.Length > 1 )
                    {
                        PrintStatus( strDeviceName );
                        this.DeviceListNames.Add( strDeviceName );
                        //comboDeviceName.Items.Add( strDeviceName );
                        m_nTotalDevice++;
                    }
                    nRetCode = (int) ImageScanStatus.MICR_ST_DEVICE_NOT_FOUND;
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

            g_strLogFileName = AppPath + "\\ExcellaLog.txt";

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
                sbLogText.AppendLine( strText );
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

        public bool BDeviceOpened { get; set; }
    }
}
