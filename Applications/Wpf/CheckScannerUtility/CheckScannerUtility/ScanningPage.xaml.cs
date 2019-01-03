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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Rock.Client;
using Rock.Client.Enums;
using Rock.Net;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// Interaction logic for ScanningPage.xaml
    /// </summary>
    public partial class ScanningPage : System.Windows.Controls.Page
    {



        public BatchPage batchPage { get { return ScanningPageUtility.batchPage; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScanningPage"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ScanningPage( BatchPage value )
        {
            InitializeComponent();
            ScanningPageUtility.batchPage = value;

            try
            {
                var config = System.Configuration.ConfigurationManager.OpenExeConfiguration( System.Configuration.ConfigurationUserLevel.None );
                ScanningPageUtility.DebugLogFilePath = config.AppSettings.Settings["DebugLogFilePath"].Value;
                bool isDirectory = !string.IsNullOrWhiteSpace( ScanningPageUtility.DebugLogFilePath ) && Directory.Exists( ScanningPageUtility.DebugLogFilePath );
                if ( isDirectory )
                {
                    ScanningPageUtility.DebugLogFilePath = Path.Combine( ScanningPageUtility.DebugLogFilePath, "CheckScanner.log" );
                }
            }
            catch
            {
                // ignore any exceptions
            }
        }
        /// <summary>
        /// Adds the scanned doc to a history of scanned docs, and shows info and status.
        /// </summary>
        /// <param name="scannedDocInfo">The scanned check info.</param>
        public void ShowScannedDocStatusAndUpload( ScannedDocInfo scannedDocInfo )
        {
            lblExceptions.Visibility = Visibility.Collapsed;

            DisplayScannedDocInfo( scannedDocInfo );

            var rockConfig = RockConfig.Load();

            bool scanningChecks = rockConfig.TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();

            // if they don't enable smart scan, don't warn about bad micr's. For example, they might be scanning a mixture of checks and envelopes
            if ( rockConfig.EnableSmartScan )
            {
                if ( scannedDocInfo.BadMicr )
                {
                    if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.RangerApi )
                    {
                        lblScanCheckWarningBadMicr.Content = @"Unable to read check information
    Click 'Skip' to reject this check and continue scanning. To retry this check, put the check back into the feed tray.   
    Click 'Upload' to upload the check as-is.
    Click 'Stop' to reject this check and stop scanning.";
                    }
                    else
                    {
                        lblScanCheckWarningBadMicr.Content = @"Unable to read check information
    Click 'Skip' to reject this check.    
    Click 'Upload' to upload the check as-is.";
                    }
                }
            }
            else
            {
                // if Enable Smart Scan is disabled, upload even if there is a bad or missing MICR
                if ( !scannedDocInfo.Upload )
                {
                    scannedDocInfo.Upload = true;
                }
            }

            if ( scannedDocInfo.Upload && ScanningPageUtility.IsDuplicateScan( scannedDocInfo ) )
            {
                scannedDocInfo.Duplicate = true;
                scannedDocInfo.Upload = false;
            }

            if ( scannedDocInfo.Upload )
            {
                ScanningPageUtility.UploadScannedItem( scannedDocInfo );
                if ( ScanningPageUtility.KeepScanning )
                {
                    ScanningPageUtility.ResumeScanning();
                }
            }
            else
            {
                ShowUploadWarnings( scannedDocInfo );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnIgnoreAndUpload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnIgnoreAndUpload_Click( object sender, RoutedEventArgs e )
        {
            HideUploadWarningPrompts();
            var scannedDocInfo = ScanningPageUtility.ConfirmUploadBadScannedDoc;
            scannedDocInfo.Upload = true;
            ScanningPageUtility.UploadScannedItem( scannedDocInfo );
            ScanningPageUtility.ResumeScanning();
        }

        /// <summary>
        /// Handles the Click event of the btnSkipAndContinue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSkipAndContinue_Click( object sender, RoutedEventArgs e )
        {
            HideUploadWarningPrompts();
            ScanningPageUtility.ConfirmUploadBadScannedDoc = null;
            ScanningPageUtility.ItemsSkipped++;
            ShowUploadStats();
            ScanningPageUtility.ResumeScanning();
        }

        /// <summary>
        /// Shows the upload warnings.
        /// </summary>
        private void ShowUploadWarnings( ScannedDocInfo scannedDocInfo )
        {
            var rockConfig = RockConfig.Load();
            ScanningPageUtility.ConfirmUploadBadScannedDoc = scannedDocInfo;
            lblScanCheckWarningDuplicate.Visibility = scannedDocInfo.Duplicate ? Visibility.Visible : Visibility.Collapsed;
            lblScanCheckWarningBadMicr.Visibility = scannedDocInfo.BadMicr ? Visibility.Visible : Visibility.Collapsed;
            lblScanItemUploadSuccess.Visibility = Visibility.Collapsed;
            pnlPromptForUpload.Visibility = scannedDocInfo.Duplicate || scannedDocInfo.BadMicr ? Visibility.Visible : Visibility.Collapsed;
            btnStart.IsEnabled = false;
            btnStopScanning.IsEnabled = true;
        }

        /// <summary>
        /// Hides the warnings messages and prompts
        /// </summary>
        private void HideUploadWarningPrompts( bool hideWarningMessages = true )
        {
            if ( hideWarningMessages )
            {
                lblScanCheckWarningDuplicate.Visibility = Visibility.Collapsed;
                lblScanCheckWarningBadMicr.Visibility = Visibility.Collapsed;
            }
            else
            {
                // update the bad micr warning to not include the btn instructions
                lblScanCheckWarningBadMicr.Content = @"Unable to read check information";
            }

            lblScanItemUploadSuccess.Visibility = Visibility.Collapsed;
            pnlPromptForUpload.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shows the upload success.
        /// </summary>
        private void ShowUploadSuccess()
        {
            lblScanCheckWarningDuplicate.Visibility = Visibility.Collapsed;
            lblScanCheckWarningBadMicr.Visibility = Visibility.Collapsed;
            lblScanItemUploadSuccess.Visibility = Visibility.Visible;
            pnlPromptForUpload.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Displays the scanned document information.
        /// </summary>
        /// <param name="scannedDocInfo">The scanned document information.</param>
        private void DisplayScannedDocInfo( ScannedDocInfo scannedDocInfo )
        {
            if ( scannedDocInfo.FrontImageData != null )
            {
                BitmapImage bitmapImageFront = new BitmapImage();
                bitmapImageFront.BeginInit();
                bitmapImageFront.StreamSource = new MemoryStream( scannedDocInfo.FrontImageData );
                bitmapImageFront.EndInit();
                imgFront.Source = bitmapImageFront;
                Rock.Wpf.WpfHelper.FadeIn( imgFront, 100 );
                lblFront.Visibility = Visibility.Visible;
            }
            else
            {
                lblFront.Visibility = Visibility.Hidden;
                imgFront.Source = null;
            }

            if ( scannedDocInfo.BackImageData != null )
            {
                BitmapImage bitmapImageBack = new BitmapImage();
                bitmapImageBack.BeginInit();
                bitmapImageBack.StreamSource = new MemoryStream( scannedDocInfo.BackImageData );
                bitmapImageBack.EndInit();
                imgBack.Source = bitmapImageBack;
                Rock.Wpf.WpfHelper.FadeIn( imgBack, 100 );
                lblBack.Visibility = Visibility.Visible;
                colBackImage.Width = new GridLength( 1, GridUnitType.Star );
            }
            else
            {
                imgBack.Source = null;
                lblBack.Visibility = Visibility.Hidden;
                colBackImage.Width = new GridLength( 0, GridUnitType.Star );
            }

            if ( scannedDocInfo.IsCheck )
            {
                pnlChecks.Visibility = Visibility.Visible;
                lblRoutingNumber.Content = scannedDocInfo.RoutingNumber ?? "--";
                lblAccountNumber.Content = scannedDocInfo.AccountNumber ?? "--";
                lblCheckNumber.Content = scannedDocInfo.CheckNumber ?? "--";
                lblOtherData.Content = scannedDocInfo.OtherData;
                spOtherData.Visibility = !string.IsNullOrWhiteSpace( scannedDocInfo.OtherData ) ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                pnlChecks.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            // set the uploadScannedItemClient to null and reconnect to ensure we have a fresh connection (just in case they changed the url, or if the connection died for some other reason)
            ScanningPageUtility.UploadScannedItemClient = null;
            ScanningPageUtility.EnsureUploadScanRestClient();
            ScanningPageUtility.Initalize();
            ShowStartupPage();
            ShowUploadStats();
            StartScanning();
            lblScanItemCountInfo.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shows the startup page.
        /// </summary>
        private void ShowStartupPage()
        {
            var rockConfig = RockConfig.Load();
            HideUploadWarningPrompts();
            lblExceptions.Visibility = Visibility.Collapsed;
            lblStartupInfo.Visibility = Visibility.Visible;
            lblNoItemsFound.Visibility = Visibility.Collapsed;
            lblScannerNotReady.Visibility = Visibility.Collapsed;

            ScannedDocInfo sampleDocInfo = new ScannedDocInfo();
            sampleDocInfo.CurrencyTypeValue = ScanningPageUtility.batchPage.CurrencyValueList.FirstOrDefault( a => a.Guid == RockConfig.Load().SourceTypeValueGuid.AsGuid() );
            DisplayScannedDocInfo( sampleDocInfo );

            bool scanningChecks = RockConfig.Load().TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
            lblNoItemsFound.Content = string.Format( "No {0} detected in scanner. Make sure {0} are properly in the feed tray.", scanningChecks ? "checks" : "items" );
            lblScanBackInstructions.Content = string.Format( "Insert the {0} again facing the other direction to get an image of the back.", scanningChecks ? "check" : "item" );
            lblScanBackInstructions.Visibility = Visibility.Collapsed;
            if ( RockConfig.Load().ScannerInterfaceType == RockConfig.InterfaceType.MICRImageRS232 )
            {
                lblStartupInfo.Content = string.Format( "Ready to scan next {0}.", scanningChecks ? "check" : "item" );

                // no need for a stop/start button when in MagTek mode
                btnStart.Visibility = Visibility.Hidden;
                btnStopScanning.Visibility = Visibility.Hidden;
            }
            else
            {
                btnStart.Visibility = Visibility.Visible;
                btnStopScanning.Visibility = Visibility.Visible;
                lblStartupInfo.Content = "Click Start to begin";
            }
        }

        /// <summary>
        /// Shows the scanner status.
        /// </summary>
        /// <param name="xportStates">The xport states.</param>
        public void ShowScannerStatus( RangerTransportStates xportStates, System.Windows.Media.Color statusColor, string statusText )
        {
            ScanningPageUtility.ShowScannerStatus( xportStates, statusColor, statusText, ref shapeStatus );
        }

        #region Ranger (Canon CR50/80) Scanner Events

        /// <summary>
        /// Rangers the scanner_ transport feeding stopped.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void rangerScanner_TransportFeedingStopped( object sender, AxRANGERLib._DRangerEvents_TransportFeedingStoppedEvent e )
        {
            RangerFeedingStoppedReasons rangerFeedingStoppedReason = ( RangerFeedingStoppedReasons ) e.reason;

            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportFeedingStopped, reason:", DateTime.Now.ToString( "o" ), rangerFeedingStoppedReason.ConvertToString() ) );
            if ( pnlPromptForUpload.Visibility != Visibility.Visible )
            {
                btnStart.IsEnabled = true;
            }

            btnClose.IsEnabled = true;
            if ( pnlPromptForUpload.Visibility != Visibility.Visible )
            {
                btnStopScanning.IsEnabled = false;
            }

            if ( ScanningPageUtility.ItemsScanned == 0 || rangerFeedingStoppedReason == RangerFeedingStoppedReasons.MainHopperEmpty)
            {
                // show the Startup Info "Welcome" message if no check images are shown yet
                if ( lblFront.Visibility != Visibility.Visible )
                {
                    lblStartupInfo.Visibility = Visibility.Visible;
                }

                // show a "No Items" warning if they clicked Start but it stopped because of MainHopperEmpty
                if ( rangerFeedingStoppedReason == RangerFeedingStoppedReasons.MainHopperEmpty )
                {
                    imgBack.Source = null;
                    imgFront.Source = null;
                    lblCheckNumber.Content = string.Empty;
                    lblRoutingNumber.Content = string.Empty;
                    lblAccountNumber.Content = string.Empty;
                    lblOtherData.Content = string.Empty;
                    lblNoItemsFound.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Handles the TransportNewItem event of the rangerScanner control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void rangerScanner_TransportNewItem( object sender, EventArgs e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportNewItem", DateTime.Now.ToString( "o" ) ) );
            ScanningPageUtility.ItemsScanned++;
        }

        /// <summary>
        /// Handles the TransportFeedingState event of the rangerScanner control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void rangerScanner_TransportFeedingState( object sender, EventArgs e )
        {
            lblStartupInfo.Visibility = Visibility.Collapsed;
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportFeedingState", DateTime.Now.ToString( "o" ) ) );
            lblNoItemsFound.Visibility = Visibility.Collapsed;
            btnStart.IsEnabled = false;
            btnClose.IsEnabled = false;
            btnStopScanning.IsEnabled = true;
        }

        /// <summary>
        /// Rangers the scanner_ transport item in pocket.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void rangerScanner_TransportSetItemOutput( object sender, AxRANGERLib._DRangerEvents_TransportSetItemOutputEvent e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportSetItemOutput", DateTime.Now.ToString( "o" ) ) );
            var currentPage = Application.Current.MainWindow.Content;

            if ( currentPage != this )
            {
                // only accept scans when the scanning page is showing
                ScanningPageUtility.batchPage.micrImage.ClearBuffer();
                return;
            }

            try
            {
                lblStartupInfo.Visibility = Visibility.Collapsed;

                RockConfig rockConfig = RockConfig.Load();

                ScannedDocInfo scannedDoc = new ScannedDocInfo();

                // mark it as Upload, but we'll set it to false if anything bad happens before we actually upload
                scannedDoc.Upload = true;
                scannedDoc.CurrencyTypeValue = ScanningPageUtility.batchPage.SelectedCurrencyValue;
                scannedDoc.SourceTypeValue = ScanningPageUtility.batchPage.SelectedSourceTypeValue;

                scannedDoc.FrontImageData = GetImageBytesFromRanger( RangerSides.TransportFront );

                if ( rockConfig.EnableRearImage )
                {
                    scannedDoc.BackImageData = GetImageBytesFromRanger( RangerSides.TransportRear );
                }

                if ( scannedDoc.IsCheck )
                {
                    string checkMicr = ScanningPageUtility.batchPage.rangerScanner.GetMicrText( 1 );
                    ScanningPageUtility.WriteToDebugLog( string.Format( "[{0}] - '{1}'", DateTime.Now.ToString( "o" ), checkMicr ) );
                    string remainingMicr = checkMicr;
                    string accountNumber = string.Empty;
                    string routingNumber = string.Empty;
                    string checkNumber = string.Empty;

                    // there should always be two transit symbols ('d').  The transit number is between them
                    int transitSymbol1 = remainingMicr.IndexOf( ( char ) RangerE13BMicrSymbols.E13B_TransitSymbol );
                    int transitSymbol2 = remainingMicr.LastIndexOf( ( char ) RangerE13BMicrSymbols.E13B_TransitSymbol );
                    int transitStart = transitSymbol1 + 1;
                    int transitLength = transitSymbol2 - transitSymbol1 - 1;
                    if ( transitLength > 0 )
                    {
                        routingNumber = remainingMicr.Substring( transitStart, transitLength );
                        remainingMicr = remainingMicr.Remove( transitStart - 1, transitLength + 2 );
                    }

                    char[] separatorSymbols = new char[] { ( char ) RangerE13BMicrSymbols.E13B_TransitSymbol, ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol, ( char ) RangerE13BMicrSymbols.E13B_AmountSymbol };

                    // the last 'On-Us' symbol ('c') signifies the end of the account number
                    int lastOnUsPosition = remainingMicr.LastIndexOf( ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol );
                    if ( lastOnUsPosition > 0 )
                    {
                        int accountNumberDigitPosition = lastOnUsPosition - 1;

                        // read all digits to the left of the last 'OnUs' until you run into another seperator symbol
                        while ( accountNumberDigitPosition >= 0 )
                        {
                            char accountNumberDigit = remainingMicr[accountNumberDigitPosition];
                            if ( separatorSymbols.Contains( accountNumberDigit ) )
                            {
                                break;
                            }
                            else
                            {
                                accountNumber = accountNumberDigit + accountNumber;
                                accountNumber = accountNumber.Trim();
                            }

                            accountNumberDigitPosition--;
                        }

                        remainingMicr = remainingMicr.Remove( accountNumberDigitPosition + 1, lastOnUsPosition - accountNumberDigitPosition );
                    }

                    // any remaining digits that aren't the account number and transit number are probably the check number
                    string[] remainingMicrParts = remainingMicr.Split( new char[] { ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol, ' ' }, StringSplitOptions.RemoveEmptyEntries );
                    string otherData = null;
                    if ( remainingMicrParts.Any() )
                    {
                        // Now that we've indentified Routing and AccountNumber, the remaining MICR part is probably the CheckNumber. However, there might be multiple Parts left. We'll have to make a best guess on which chunk is the CheckNumber.
                        // In those cases, assume the 'longest' chunk to the CheckNumber. (Other chunks tend to be short 1 or 2 digit numbers that mean something special to the bank)
                        checkNumber = remainingMicrParts.OrderBy( p => p.Length ).Last();

                        // throw any remaining data into 'otherData' (a reject symbol could be in the other data)
                        remainingMicr = remainingMicr.Replace( ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol, ' ' );
                        remainingMicr = remainingMicr.Replace( checkNumber, string.Empty );
                        otherData = remainingMicr;
                    }

                    scannedDoc.RoutingNumber = routingNumber;
                    scannedDoc.AccountNumber = accountNumber;
                    scannedDoc.CheckNumber = checkNumber;
                    scannedDoc.OtherData = otherData;

                    scannedDoc.ScannedCheckMicrData = checkMicr;

                    // look for the "can't read" symbol (or completely blank read ) to detect if the check micr couldn't be read
                    // from http://www.sbulletsupport.com/forum/index.php?topic=172.0
                    if ( checkMicr.Contains( ( char ) RangerCommonSymbols.RangerRejectSymbol ) || string.IsNullOrWhiteSpace( checkMicr ) )
                    {
                        scannedDoc.BadMicr = true;
                        scannedDoc.Upload = false;
                    }
                }

                ShowScannedDocStatusAndUpload( scannedDoc );
            }
            catch ( Exception ex )
            {
                if ( ex is AggregateException )
                {
                    ScanningPageUtility.ShowException( ( ex as AggregateException ).Flatten(),this.lblExceptions );
                }
                else
                {
                    ScanningPageUtility.ShowException( ex, this.lblExceptions );
                }
            }
        }

        #endregion

        #region Scanner (MagTek MICRImage RS232) Events

        private ScannedDocInfo _currentMagtekScannedDoc { get; set; }

        /// <summary>
        /// Handles the MicrDataReceived event of the micrImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void micrImage_MicrDataReceived( object sender, System.EventArgs e )
        {
            var currentPage = Application.Current.MainWindow.Content;

            if ( currentPage != this )
            {
                // only accept scans when the scanning page is showing
                ScanningPageUtility.batchPage.micrImage.ClearBuffer();
                return;
            }

            lblStartupInfo.Visibility = Visibility.Collapsed;

            // from MagTek Sample Code
            object dummy = null;
            string routingNumber = ScanningPageUtility.batchPage.micrImage.FindElement( 0, "T", 0, "TT", ref dummy );
            string accountNumber = ScanningPageUtility.batchPage.micrImage.FindElement( 0, "TT", 0, "A", ref dummy );
            string checkNumber = ScanningPageUtility.batchPage.micrImage.FindElement( 0, "A", 0, "12", ref dummy );
            short trackNumber = 0;
            var rawMICR = ScanningPageUtility.batchPage.micrImage.GetTrack( ref trackNumber );

            ScannedDocInfo scannedDoc = null;
            var rockConfig = RockConfig.Load();
            bool scanningMagTekBackImage = false;

            if ( _currentMagtekScannedDoc != null && _currentMagtekScannedDoc.BackImageData == null && rockConfig.PromptToScanRearImage )
            {
                //// if we didn't get a routingnumber, and we are expecting a back scan, use the scan as the back image
                //// However, if we got a routing number, assuming we are scanning a new check regardless
                if ( string.IsNullOrWhiteSpace( routingNumber ) )
                {
                    scanningMagTekBackImage = true;
                }
                else
                {
                    scanningMagTekBackImage = false;
                }
            }

            if ( scanningMagTekBackImage )
            {
                scannedDoc = _currentMagtekScannedDoc;
            }
            else
            {
                scannedDoc = new ScannedDocInfo();
                scannedDoc.CurrencyTypeValue = ScanningPageUtility.batchPage.SelectedCurrencyValue;
                scannedDoc.SourceTypeValue = ScanningPageUtility.batchPage.SelectedSourceTypeValue;

                if ( scannedDoc.IsCheck )
                {
                    scannedDoc.ScannedCheckMicrData = rawMICR;
                    scannedDoc.RoutingNumber = routingNumber;
                    scannedDoc.AccountNumber = accountNumber;
                    scannedDoc.CheckNumber = checkNumber;

                    ScanningPageUtility.WriteToDebugLog( string.Format( "[{0}] - '{1}'", DateTime.Now.ToString( "o" ), scannedDoc.ScannedCheckMicrData ) );
                }

                // set the _currentMagtekScannedDoc in case we are going to scan the back of the image
                _currentMagtekScannedDoc = scannedDoc;
            }

            string imagePath = Path.GetTempPath();
            string docImageFileName = Path.Combine( imagePath, string.Format( "scanned_item_{0}.tif", Guid.NewGuid() ) );
            if ( File.Exists( docImageFileName ) )
            {
                File.Delete( docImageFileName );
            }

            try
            {
                string statusMsg = string.Empty;
                ScanningPageUtility.batchPage.micrImage.TransmitCurrentImage( docImageFileName, ref statusMsg );
                if ( !File.Exists( docImageFileName ) )
                {
                    throw new Exception( "Unable to retrieve image" );
                }

                if ( scanningMagTekBackImage )
                {
                    scannedDoc.BackImageData = File.ReadAllBytes( docImageFileName );
                }
                else
                {
                    scannedDoc.FrontImageData = File.ReadAllBytes( docImageFileName );

                    // MagTek puts the symbol '?' for parts of the MICR that it can't read
                    bool gotValidMicr = !string.IsNullOrWhiteSpace( scannedDoc.AccountNumber ) && !scannedDoc.AccountNumber.Contains( '?' )
                        && !string.IsNullOrWhiteSpace( scannedDoc.RoutingNumber ) && !scannedDoc.RoutingNumber.Contains( '?' )
                        && !string.IsNullOrWhiteSpace( scannedDoc.CheckNumber ) && !scannedDoc.CheckNumber.Contains( '?' );

                    if ( scannedDoc.IsCheck && !gotValidMicr )
                    {
                        scannedDoc.BadMicr = true;
                    }
                }

                if ( scannedDoc.BackImageData == null && rockConfig.PromptToScanRearImage )
                {
                    // scanning the front image, but still need to scan the back
                    lblScanBackInstructions.Visibility = Visibility.Visible;
                    HideUploadWarningPrompts( true );
                    DisplayScannedDocInfo( scannedDoc );
                }
                else
                {
                    // scanned both sides (or just the front if they don't want to scan both sides )
                    lblScanBackInstructions.Visibility = Visibility.Collapsed;
                    scannedDoc.Upload = !scannedDoc.IsCheck || !( scannedDoc.BadMicr || scannedDoc.Duplicate );
                    this.ShowScannedDocStatusAndUpload( scannedDoc );
                }

                File.Delete( docImageFileName );
            }
            catch ( Exception ex )
            {
                if ( ex is AggregateException )
                {
                    ScanningPageUtility.ShowException( ( ex as AggregateException ).Flatten(),this.lblExceptions );
                }
                else
                {
                    ScanningPageUtility.ShowException( ex, this.lblExceptions );
                }
            }
            finally
            {
                ScanningPageUtility.batchPage.micrImage.ClearBuffer();
            }
        }

        #endregion

        #region Image Upload related

        /// <summary>
        /// Gets the doc image.
        /// </summary>
        /// <param name="side">The side.</param>
        /// <returns></returns>
        private byte[] GetImageBytesFromRanger( RangerSides side )
        {
            RangerImageColorTypes colorType = RockConfig.Load().ImageColorType;

            int imageByteCount;
            imageByteCount = ScanningPageUtility.batchPage.rangerScanner.GetImageByteCount( ( int ) side, ( int ) colorType );
            if ( imageByteCount > 0 )
            {
                byte[] imageBytes = new byte[imageByteCount];

                // create the pointer and assign the Ranger image address to it
                IntPtr imgAddress = new IntPtr( ScanningPageUtility.batchPage.rangerScanner.GetImageAddress( ( int ) side, ( int ) colorType ) );

                // Copy the bytes from unmanaged memory to managed memory
                Marshal.Copy( imgAddress, imageBytes, 0, imageByteCount );

                return imageBytes;
            }
            else
            {
                return null;
            }
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnClose_Click( object sender, RoutedEventArgs e )
        {
            this.NavigationService.Navigate( ScanningPageUtility.batchPage );
        }

        #region Upload Scanned Items
       
        /// <summary>
        /// Shows the upload stats.
        /// </summary>
        private void ShowUploadStats()
        {
            List<string> statsList = new List<string>();
            if ( ScanningPageUtility.ItemsUploaded > 0 )
            {
                statsList.Add( string.Format( "Uploaded: {0}", ScanningPageUtility.ItemsUploaded ) );
            }

            if ( ScanningPageUtility.ItemsSkipped > 0 )
            {
                statsList.Add( string.Format( "Skipped: {0}", ScanningPageUtility.ItemsSkipped ) );
            }

            lblScanItemCountInfo.Visibility = statsList.Any() ? Visibility.Visible : Visibility.Collapsed;
            lblScanItemCountInfo.Content = string.Join( ", ", statsList );
        }

        //
        /// <summary>
        /// Handles the Click event of the btnStart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStart_Click( object sender, RoutedEventArgs e )
        {
            lblScannerNotReady.Visibility = Visibility.Collapsed;

            RangerTransportStates[] xportStatesNotConnected = new RangerTransportStates[] { RangerTransportStates.TransportShutDown, RangerTransportStates.TransportShuttingDown, RangerTransportStates.TransportExceptionInProgress };

            var transportState = ( RangerTransportStates ) ScanningPageUtility.batchPage.rangerScanner.GetTransportState();
            if ( xportStatesNotConnected.Contains( transportState ) )
            {
                ScanningPageUtility.batchPage.ConnectToScanner();
            }
            else
            {
                StartScanning();
            }
        }

        /// <summary>
        /// Handles the TransportIsDead event of the rangerScanner control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void rangerScanner_TransportIsDead( object sender, EventArgs e )
        {
            ScanningPageUtility.rangerScanner_TransportIsDead( sender, e, this.lblScannerNotReady );
        }

        /// <summary>
        /// Starts the scanning as soon as items are in the hopper
        /// </summary>
        public void StartScanning()
        {
            ScanningPageUtility.KeepScanning = true;
            ScanningPageUtility.ResumeScanning();
        }

        /// <summary>
        /// Handles the Click event of the btnStopScanning control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStopScanning_Click( object sender, RoutedEventArgs e )
        {
            HideUploadWarningPrompts( false );
            StopScanning();
            btnStart.IsEnabled = true;
        }

        /// <summary>
        /// Stops the scanning.
        /// </summary>
        private void StopScanning()
        {
            ScanningPageUtility.KeepScanning = false;
            if ( ScanningPageUtility.batchPage.rangerScanner != null )
            {
                // remove the StartRangerFeedingWhenReady (in case it is assigned) so it doesn't restart after getting into ReadyToFeed state
                ScanningPageUtility.batchPage.rangerScanner.TransportReadyToFeedState -= StartRangerFeedingWhenReady;
                ScanningPageUtility.batchPage.rangerScanner.StopFeeding();
            }
        }


        /// <summary>
        /// Starts the ranger feeding when ready.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void StartRangerFeedingWhenReady( object sender, AxRANGERLib._DRangerEvents_TransportReadyToFeedStateEvent e )
        {
            // only fire this event once
            ScanningPageUtility.batchPage.rangerScanner.TransportReadyToFeedState -= StartRangerFeedingWhenReady;

            if ( ScanningPageUtility.KeepScanning )
            {
                ScanningPageUtility.batchPage.rangerScanner.StartFeeding( FeedSource.FeedSourceMainHopper, FeedItemCount.FeedOne );
            }
        }

        #endregion
    }
}
