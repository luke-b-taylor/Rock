using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ImageScanInteropBuilder
{
    /// <summary>
    /// Current MagTek USB Provider and Options Defined for  Image Safe Support at this time
    /// Code will support EXCELLA_ETHER,EXCELLA_USB, STX_ETHER,STX_USB Mag Tek Devices
    ///
    /// </summary>
    public class MagTekUsbScanner:IDisposable
    {
        #region Private Members

        private string _deviceName = string.Empty;
        private const int DEFAULT_STRING_BUFFER_SIZE = 4096;
        private MagTekImageScanInterop _magTekImageScanInterop;
        private static int nTotalDocProcessed = 0;
        private MagTekImageScanInterop MagTekImageScanInterop
        {
            get
            {
                if ( _magTekImageScanInterop == null )
                {
                    _magTekImageScanInterop = new MagTekImageScanInterop();
                }
                return _magTekImageScanInterop;
            }
        }


        #endregion

        #region Public Members

        public event EventHandler<oCheckData> OnDocumentProcessComplete;
        /// <summary>
        /// Sets the image data.
        /// Call from Process Document
        /// </summary>
        public struct oCheckData
        {
            public bool HasError { get; set; }

            /// <summary>
            /// Gets or sets the image data.
            /// </summary>
            /// <value>
            /// The image data.
            /// </value>
            public byte[] ImageData { get; set; }
        
            public string RoutingNumber { get; set; }

            /// <summary>
            /// Gets or sets the account number.
            /// </summary>
            /// <value>
            /// The account number.
            /// </value>
            public string AccountNumber { get; set; }

            /// <summary>
            /// Gets or sets the check number.
            /// </summary>
            /// <value>
            /// The check number.
            /// </value>
            public string CheckNumber { get; set; }

            /// <summary>
            /// Any other MICR data that isn't the Routing, AccountNumber or CheckNumber
            /// </summary>
            /// <value>
            /// The other data.
            /// </value>
            public string OtherData { get; set; }
            public string ScannedCheckMicrData { get; set; }

            /// <summary>
            /// Gets the masked account number.
            /// </summary>
            /// <value>
            /// The masked account number.
            /// </value>
            public string MaskedAccountNumber
            {
                get
                {
                    if ( !string.IsNullOrEmpty(AccountNumber) && AccountNumber.Length > 4)
                    {
                        int length = AccountNumber.Length;
                        string result = new string( 'x', length - 4 ) + AccountNumber.Substring( length - 4 );
                        return result;
                    }
                    return string.Empty;
                }
            }

            /// <summary>
            /// Gets or sets the errors.
            /// </summary>
            /// <value>
            /// The errors.
            /// </value>
            public StringBuilder Errors { get; set; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MagTekUsbScanner"/> class.
        /// </summary>
        /// <param name="exectionPath">The exection path.</param>
        public MagTekUsbScanner( string exectionPath )
        {
            if ( ErrorLog == null )
            {
                ErrorLog = new StringBuilder();
            }
            if ( string.IsNullOrEmpty( exectionPath ) )
            {
                ErrorLog.Append( "MagTekScanner missing parameter execution path" );
                return;
            }

            MagTekImageScanInterop.Init( exectionPath );
        }

        /// <summary>
        /// Gets a value indicating whether this instance has error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has error; otherwise, <c>false</c>.
        /// </value>
        public bool HasError { get; set; }
        /// <summary>
        /// Gets or sets the error log.
        /// </summary>
        /// <value>
        /// The error log.
        /// </value>
        public StringBuilder ErrorLog { get; set; }

        /// <summary>
        /// Converts to talprocessed.
        /// </summary>
        /// <value>
        /// The total processed.
        /// </value>
        public int TotalProcessed { get { return nTotalDocProcessed; } }
        
        /// <summary>
        /// Gets or sets the exection path.
        /// </summary>
        /// <value>
        /// The exection path.
        /// </value>
        public string ExectionPath { get; set; }

        /// <summary>
        /// Gets a value indicating whether [port open].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [port open]; otherwise, <c>false</c>.
        /// </value>
        public bool PortOpen
        {
            get
            {
                if ( string.IsNullOrEmpty( MagTekImageScanInterop.CurrentDeviceName ) )
                {
                    var currentDevice = this.GetDeviceList().Where( x => x.Contains( "ImageSafe" ) ).FirstOrDefault();
                    if ( string.IsNullOrEmpty( currentDevice ) )
                    {
                        ErrorLog.Append( string.Format( "No ImageSafe devices found." ) );
                        return false;
                    }
                    MagTekImageScanInterop.CurrentDeviceName = currentDevice;
                }

                return MagTekImageScanInterop.OpenDevice( MagTekImageScanInterop.CurrentDeviceName );
            }
        }

        /// <summary>
        /// Closes the port.
        /// </summary>
        /// <returns></returns>
        public bool ClosePort()
        {

            var isClosed = false;

            if ( !MagTekImageScanInterop.DeviceOpened )
            { return true; }

            if ( _deviceName != null )
            {
                isClosed = MagTekImageScanInterop.CloseDevice( _deviceName );
            }

            return isClosed;
        }

        /// <summary>
        /// Gets the device list.
        /// </summary>
        /// <returns></returns>
        public List<string> GetDeviceList()
        {

            return _magTekImageScanInterop.DeviceListNames;
        }

        public string QueryDevice(string queryInfo) {

            StringBuilder strResults = new StringBuilder();

            int nRet;
            int nLength = 4096;
            strResults.Capacity = 4096;

            switch ( queryInfo )
            {
                case "DeviceCapabilities":
                    nRet = _magTekImageScanInterop.MICRQueryInfo( _magTekImageScanInterop.CurrentDeviceName, "DeviceCapabilities", strResults, ref nLength );
                    break;
                case "DeviceStatus":
                    nRet = _magTekImageScanInterop.MICRQueryInfo( _magTekImageScanInterop.CurrentDeviceName, "DeviceStatus", strResults, ref nLength );
                    break;
                case "DeviceUsage":
                    nRet = _magTekImageScanInterop.MICRQueryInfo( _magTekImageScanInterop.CurrentDeviceName, "DeviceUsage", strResults, ref nLength );
                    break;


            }

            return strResults.ToString();
        }

        /// <summary>
        /// Processes the document.
        /// </summary>
        /// <returns></returns>
        public void ProcessDocument()
        {
            var checkData = new oCheckData();
            EventHandler<oCheckData> handler = OnDocumentProcessComplete;

            // first time through get the device name and open the device
            if (!string.IsNullOrEmpty( MagTekImageScanInterop.CurrentDeviceName ) )
            {
                var deviceList = this.GetDeviceList();
                if ( deviceList != null && deviceList.Count > 0 )
                {

                    var imageSafeName = deviceList.Where( x => x.Contains( "ImageSafe" ) ).First();
                    if ( imageSafeName != null )
                    {
                        MagTekImageScanInterop.CurrentDeviceName = imageSafeName;
                    }
                    MagTekImageScanInterop.OpenDevice( MagTekImageScanInterop.CurrentDeviceName );
                }

                //Process Document
                if ( MagTekImageScanInterop.DeviceOpened )
                {
                    int nRet = -1;
                    string strLog;
                    string strTmp;

                    ++nTotalDocProcessed;

                    MagTekImageScanInterop.ProcessInit();
                    nRet = SetupOptions();

                    if ( nRet != ( int ) ImageScanStatus.MICR_ST_OK )
                    {
                        strLog = "Setup Options FAILED...";
                        MagTekImageScanInterop.PrintStatus(strLog);
                        this.ErrorLog.AppendLine( strLog );
                        checkData.Errors = this.ErrorLog;
                    }

                    strLog = "Begin Process Options Info...";
                    MagTekImageScanInterop.PrintStatus( strLog );
                    this.ErrorLog.AppendLine( strLog );
                    MagTekImageScanInterop.PrintStatus( MagTekImageScanInterop.Options );

                    strLog = "End Process Options Info...";
                    this.ErrorLog.AppendLine( strLog );
                    MagTekImageScanInterop.PrintStatus( strLog );

                    StringBuilder strResponse = new StringBuilder();
                    strResponse.Capacity = 4096;
                    int nResponseLength = 4096;
                    MagTekImageScanInterop.DocInfo = "";

                    nRet = MagTekImageScanInterop.MICRProcessCheck( MagTekImageScanInterop.CurrentDeviceName, MagTekImageScanInterop.Options, strResponse, ref nResponseLength );
                    if ( nRet == ( int ) ImageScanStatus.MICR_ST_OK )
                    {
                        MagTekImageScanInterop.DocInfo = strResponse.ToString();

                        MagTekImageScanInterop.PrintStatus( MagTekImageScanInterop.DocInfo );

                        nRet = MagTekImageScanInterop.MICRGetValue( MagTekImageScanInterop.DocInfo, "CommandStatus", "ReturnCode", strResponse, ref nResponseLength );
                        strTmp = strResponse.ToString();

                        int nReturnCode = Convert.ToInt32( strTmp );
                        strLog = "Process Check return code " + nReturnCode;

                        MagTekImageScanInterop.PrintStatus( strLog );

                        if ( nReturnCode == 0 )
                        {
                            if ( MagTekImageScanInterop.DocType == DocType.CHECK )
                            {
                                SetImageData(ref checkData);
                                SetCheckData(ref checkData);
                                handler( this,checkData);
                                return;
                            }
                            else
                            {
                                this.ErrorLog.AppendLine( strLog );
                                MagTekImageScanInterop.PrintStatus( "Process Check Feeder not Set to Check." );
                                this.HandleError( handler, "Process Check Feeder not Set to Check.",checkData );
                                return;
                            }

                        }
                        else if ( nReturnCode == 250 )
                        {
                            this.ErrorLog.AppendLine( strLog );
                            MagTekImageScanInterop.PrintStatus( "Check Waiting Timeout!" );
                            this.HandleError( handler, "Check Waiting Timeout!", checkData );
                            return;
                        }
                        else
                        {
                            MagTekImageScanInterop.PrintStatus( "Process Check FAILED!" );
                            this.ErrorLog.AppendLine( "Process Check FAILED!" );
                            this.HandleError( handler, strLog, checkData );
                            return;
                        }
                    }
                    else
                    {
                        strLog = "MTMICRProcessCheck return code " + nRet;
                        MagTekImageScanInterop.PrintStatus( strLog );
                        this.HandleError(handler,strLog, checkData );
                        return;
                    }

                }
            }

            this.HandleError( handler, "No Current Device Name found.", checkData );
        }

        private void HandleError(EventHandler<oCheckData> handler, string message, oCheckData checkData )
        {
            checkData.HasError = true;
            this.ErrorLog.AppendLine( message );
            checkData.Errors = this.ErrorLog;
            handler( this, checkData );
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// Setups the options.
        /// </summary>
        /// <returns></returns>
        private int SetupOptions()
        {
            int nRet;

            StringBuilder strOptions = new StringBuilder();
            strOptions.Capacity = 4096;

            int nActualLength = 4096;

            nRet = MagTekImageScanInterop.MICRSetValue( strOptions, "Application", "Transfer", "HTTP", ref nActualLength );

            if ( nRet != ( int ) ImageScanStatus.MICR_ST_OK )
                return -1;

            nRet = MagTekImageScanInterop.MICRSetValue( strOptions, "Application", "DocUnits", "ENGLISH", ref nActualLength );

            if ( nRet != ( int ) ImageScanStatus.MICR_ST_OK )
                return -1;

            nRet = MagTekImageScanInterop.MICRSetValue( strOptions, "ProcessOptions", "DocFeedTimeout", "10000", ref nActualLength );

            if ( nRet != ( int ) ImageScanStatus.MICR_ST_OK )
                return -1;


            if ( MagTekImageScanInterop.DocType == DocType.MSR )
            {
                nRet = MagTekImageScanInterop.MICRSetValue( strOptions, "ProcessOptions", "DocFeed", "MSR", ref nActualLength );
                if ( nRet != ( int ) ImageScanStatus.MICR_ST_OK )
                    return -1;
                MagTekImageScanInterop.Options = strOptions.ToString();
            }
            else
            {
                nRet = MagTekImageScanInterop.MICRSetValue( strOptions, "ProcessOptions", "DocFeed", "MANUAL", ref nActualLength );
                if ( nRet != ( int ) ImageScanStatus.MICR_ST_OK )
                    return -1;

                nRet = MagTekImageScanInterop.MICRSetValue( strOptions, "ImageOptions", "Number", "1", ref nActualLength );
                if ( nRet != ( int ) ImageScanStatus.MICR_ST_OK )
                    return -1;

                nRet = MagTekImageScanInterop.MICRSetIndexValue( strOptions, "ImageOptions", "ImageSide", 1, "FRONT", ref nActualLength );
                if ( nRet != ( int ) ImageScanStatus.MICR_ST_OK )
                    return -1;

                nRet = MagTekImageScanInterop.MICRSetIndexValue( strOptions, "ImageOptions", "ImageColor", 1, "GRAY8", ref nActualLength );
                if ( nRet != ( int ) ImageScanStatus.MICR_ST_OK )
                    return -1;


                nRet = MagTekImageScanInterop.MICRSetIndexValue( strOptions, "ImageOptions", "Resolution", 1, "200x200", ref nActualLength );
                if ( nRet != ( int ) ImageScanStatus.MICR_ST_OK )
                    return -1;

                nRet = MagTekImageScanInterop.MICRSetIndexValue( strOptions, "ImageOptions", "Compression", 1, "JPEG", ref nActualLength );
                if ( nRet != ( int ) ImageScanStatus.MICR_ST_OK )
                    return -1;

                nRet = MagTekImageScanInterop.MICRSetIndexValue( strOptions, "ImageOptions", "FileType", 1, "JPG", ref nActualLength );
                if ( nRet != ( int ) ImageScanStatus.MICR_ST_OK )
                    return -1;


                nRet = MagTekImageScanInterop.MICRSetValue( strOptions, "ProcessOptions", "ReadMICR", "E13B", ref nActualLength );
                if ( nRet != ( int ) ImageScanStatus.MICR_ST_OK )
                    return -1;

                nRet = MagTekImageScanInterop.MICRSetValue( strOptions, "ProcessOptions", "MICRFmtCode", "6200", ref nActualLength );
                if ( nRet != ( int ) ImageScanStatus.MICR_ST_OK )
                    return -1;

                MagTekImageScanInterop.Options = strOptions.ToString();

            }

            return ( int ) ImageScanStatus.MICR_ST_OK;

        }
        private void SetImageData(ref oCheckData checkData)
        {
            int nRet;
            StringBuilder strResponse = new StringBuilder();
            strResponse.Capacity = 4096;
            int nResponseLength = 4096;
            int nImageSize;
            string strTmp;
            string strImageID;
            MagTekImageScanInterop.PrintStatus( MagTekImageScanInterop.DocInfo );

            nRet = MagTekImageScanInterop.MICRGetIndexValue( MagTekImageScanInterop.DocInfo, "ImageInfo", "ImageSize", 1, strResponse, ref nResponseLength );
            strTmp = strResponse.ToString();
            nImageSize = Convert.ToInt32( strTmp );

            if ( nImageSize > 0 )
            {

                nRet = MagTekImageScanInterop.MICRGetIndexValue( MagTekImageScanInterop.DocInfo, "ImageInfo", "ImageURL", 1, strResponse, ref nResponseLength );
                strImageID = strResponse.ToString();
                string strLog = "Image size =" + nImageSize + "ImageID = " + strImageID;
                Trace.WriteLine( strLog );
                MagTekImageScanInterop.PrintStatus( strLog );

                byte[] imageBuf = new byte[nImageSize];

                nRet = MagTekImageScanInterop.MICRGetImage( MagTekImageScanInterop.CurrentDeviceName, strImageID, imageBuf, ref nImageSize );
                if ( nRet == ( int ) ImageScanStatus.MICR_ST_OK )
                {
                    checkData.ImageData = imageBuf;
                    int nActualSize = nImageSize;
                    strLog = "NumOfBytes to write =" + nActualSize;

                    Trace.WriteLine( strLog );
                    MagTekImageScanInterop.PrintStatus( strLog );
                    IntPtr pOverlapped = IntPtr.Zero;
                }
                else
                {
                    MagTekImageScanInterop.PrintStatus( "GetImage FAILED" );
                }

            }
        }
        /// <summary>
        /// Sets the check data.
        /// Called from Process Document
        /// </summary>
        private void SetCheckData(ref oCheckData checkData)
        {
            int nRet;
            StringBuilder strResponse = new StringBuilder();
            strResponse.Capacity = 4096;
            int nResponseLength = 4096;
            string strTmp;
            if ( MagTekImageScanInterop.DocType == DocType.MSR )
            {
                checkData.OtherData = string.Empty;
                checkData.RoutingNumber = string.Empty;
                checkData.AccountNumber = string.Empty;
                checkData.CheckNumber = string.Empty;
                
            }
            else
            {
                nRet = MagTekImageScanInterop.MICRGetValue( MagTekImageScanInterop.DocInfo, "DocInfo", "MICRRaw", strResponse, ref nResponseLength );
                strTmp = strResponse.ToString();
                checkData.ScannedCheckMicrData = strTmp;

                nRet = MagTekImageScanInterop.MICRGetValue( MagTekImageScanInterop.DocInfo, "DocInfo", "MICRTransit", strResponse, ref nResponseLength );
                strTmp = strResponse.ToString();
                checkData.RoutingNumber = strTmp;

                nRet = MagTekImageScanInterop.MICRGetValue( MagTekImageScanInterop.DocInfo, "DocInfo", "MICRAcct", strResponse, ref nResponseLength );
                strTmp = strResponse.ToString();
                checkData.AccountNumber = strTmp;

                nRet = MagTekImageScanInterop.MICRGetValue( MagTekImageScanInterop.DocInfo, "DocInfo", "MICRSerNum", strResponse, ref nResponseLength );
                strTmp = strResponse.ToString();
                checkData.CheckNumber = strTmp;
            }
        }

        #endregion
        #region IDispose

        bool disposed = false;
        public void Dispose( bool disposing )
        {
            if ( disposed )
                return;

            if ( disposing )
            {
                this.ErrorLog = null;
            }

            this._magTekImageScanInterop = null;
            disposed = true;
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        #endregion

    }
}
