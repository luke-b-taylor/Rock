using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ImageScanInteropBuilder
{
    public class MagTekScanner
    {
        /// <summary>
        /// Structure of Check Data
        /// </summary>
        public struct oCheckData
        {
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
                    int length = AccountNumber.Length;
                    string result = new string( 'x', length - 4 ) + AccountNumber.Substring( length - 4 );
                    return result;
                }
            }
        }

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
        private void SetImageData()
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
                if ( nRet == MagTekConstants.MICR_ST_OK )
                {
                    this.ImageData = imageBuf;


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
        private void SetCheckData()
        {
            int nRet;
            StringBuilder strResponse = new StringBuilder();
            strResponse.Capacity = 4096;
            int nResponseLength = 4096;
            string strTmp;
            var checkData = new oCheckData();
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
                checkData.OtherData = strTmp;

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

        #region Public Members
        public MagTekScanner( string exectionPath )
        {
            if ( string.IsNullOrEmpty( exectionPath ) )
            {
                new ArgumentNullException( "executionPath", "MagTekScanner missing parameter execution path" );
            }

            MagTekImageScanInterop.Init( exectionPath );
        }
        public int TotalProcessed { get { return nTotalDocProcessed; } }
        public byte[] ImageData { get; set; }
        public string ExectionPath { get; set; }
        public bool PortOpen
        {
            get
            {
                return MagTekImageScanInterop.OpenDevice(MagTekImageScanInterop.CurrentDeviceName);
            }
        }


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
        public List<string> GetDeviceList()
        {

            return _magTekImageScanInterop.DeviceListNames;
        }
        public void ProcessDocument()
        {
            int nRet = -1;
            string strLog;
            string strTmp;

            ++nTotalDocProcessed;

            MagTekImageScanInterop.ProcessInit();
            nRet = SetupOptions();

            if ( nRet != MagTekConstants.MICR_ST_OK )
            {
                strLog = "Setup Options FAILED...";
                MagTekImageScanInterop.PrintStatus( strLog );
                return;
            }

            strLog = "Begin Process Options Info...";
            MagTekImageScanInterop.PrintStatus( strLog );
            MagTekImageScanInterop.PrintStatus( MagTekImageScanInterop.Options );

            strLog = "End Process Options Info...";
            MagTekImageScanInterop.PrintStatus( strLog );

            StringBuilder strResponse = new StringBuilder();
            strResponse.Capacity = 4096;
            int nResponseLength = 4096;
            MagTekImageScanInterop.DocInfo = "";

            nRet = MagTekImageScanInterop.MICRProcessCheck( MagTekImageScanInterop.CurrentDeviceName, MagTekImageScanInterop.Options, strResponse, ref nResponseLength );
            if ( nRet == MagTekConstants.MICR_ST_OK )
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
                        SetImageData();
                        SetCheckData();
                    }
                    else
                    {
                        MagTekImageScanInterop.PrintStatus( "Process Check Feeder not Set to Check." );
                    }


                }
                else if ( nReturnCode == 250 )
                {

                    MagTekImageScanInterop.PrintStatus( "Check Waiting Timeout!" );

                }
                else
                {
                    MagTekImageScanInterop.PrintStatus( "Process Check FAILED!" );
                }


            }
            else
            {
                strLog = "MTMICRProcessCheck return code " + nRet;

                MagTekImageScanInterop.PrintStatus( strLog );
            }


        }

        public oCheckData CheckData { get; set; }
        #endregion

        #region Private Methods
        private int SetupOptions()
        {

            int nRet;

            StringBuilder strOptions = new StringBuilder();
            strOptions.Capacity = 4096;

            int nActualLength = 4096;

            nRet = MagTekImageScanInterop.MICRSetValue( strOptions, "Application", "Transfer", "HTTP", ref nActualLength );

            if ( nRet != MagTekConstants.MICR_ST_OK )
                return -1;

            nRet = MagTekImageScanInterop.MICRSetValue( strOptions, "Application", "DocUnits", "ENGLISH", ref nActualLength );

            if ( nRet != MagTekConstants.MICR_ST_OK )
                return -1;

            nRet = MagTekImageScanInterop.MICRSetValue( strOptions, "ProcessOptions", "DocFeedTimeout", "10000", ref nActualLength );

            if ( nRet != MagTekConstants.MICR_ST_OK )
                return -1;


            if ( MagTekImageScanInterop.DocType == DocType.MSR )
            {
                nRet = MagTekImageScanInterop.MICRSetValue( strOptions, "ProcessOptions", "DocFeed", "MSR", ref nActualLength );
                if ( nRet != MagTekConstants.MICR_ST_OK )
                    return -1;
                MagTekImageScanInterop.Options = strOptions.ToString();
            }
            else
            {
                nRet = MagTekImageScanInterop.MICRSetValue( strOptions, "ProcessOptions", "DocFeed", "MANUAL", ref nActualLength );
                if ( nRet != MagTekConstants.MICR_ST_OK )
                    return -1;

                nRet = MagTekImageScanInterop.MICRSetValue( strOptions, "ImageOptions", "Number", "1", ref nActualLength );
                if ( nRet != MagTekConstants.MICR_ST_OK )
                    return -1;

                nRet = MagTekImageScanInterop.MICRSetIndexValue( strOptions, "ImageOptions", "ImageSide", 1, "FRONT", ref nActualLength );
                if ( nRet != MagTekConstants.MICR_ST_OK )
                    return -1;

                nRet = MagTekImageScanInterop.MICRSetIndexValue( strOptions, "ImageOptions", "ImageColor", 1, "GRAY8", ref nActualLength );
                if ( nRet != MagTekConstants.MICR_ST_OK )
                    return -1;


                nRet = MagTekImageScanInterop.MICRSetIndexValue( strOptions, "ImageOptions", "Resolution", 1, "200x200", ref nActualLength );
                if ( nRet != MagTekConstants.MICR_ST_OK )
                    return -1;

                nRet = MagTekImageScanInterop.MICRSetIndexValue( strOptions, "ImageOptions", "Compression", 1, "JPEG", ref nActualLength );
                if ( nRet != MagTekConstants.MICR_ST_OK )
                    return -1;

                nRet = MagTekImageScanInterop.MICRSetIndexValue( strOptions, "ImageOptions", "FileType", 1, "JPG", ref nActualLength );
                if ( nRet != MagTekConstants.MICR_ST_OK )
                    return -1;


                nRet = MagTekImageScanInterop.MICRSetValue( strOptions, "ProcessOptions", "ReadMICR", "E13B", ref nActualLength );
                if ( nRet != MagTekConstants.MICR_ST_OK )
                    return -1;

                nRet = MagTekImageScanInterop.MICRSetValue( strOptions, "ProcessOptions", "MICRFmtCode", "6200", ref nActualLength );
                if ( nRet != MagTekConstants.MICR_ST_OK )
                    return -1;

                MagTekImageScanInterop.Options = strOptions.ToString();

            }

            return MagTekConstants.MICR_ST_OK;

        }
        #endregion

    }
}
