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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Rock.Apps.CheckScannerUtility.Models;
using Rock.Client;
using Rock.Client.Enums;
using Rock.Net;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// Interaction logic for CaptureAmountScanningPage.xaml
    /// </summary>
    public partial class CaptureAmountScanningPage : System.Windows.Controls.Page
    {
       
        private ScannedDocInfo _currentscannedDocInfo = null;
        private bool _isDoubleSided = false;


        public ObservableCollection<DisplayAccountValueModel> _displayAccountValuesContext { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="CaptureAmountScanningPage"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public CaptureAmountScanningPage( BatchPage value )
        {

            ScanningPageUtility.batchPage = value;
            InitializeComponent();
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
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            btnNext.IsEnabled = false;
            // set the uploadScannedItemClient to null and reconnect to ensure we have a fresh connection (just in case they changed the url, or if the connection died for some other reason)
            ScanningPageUtility.Initalize();
            InitalizeControls();
            ScanningPageUtility.UploadScannedItemClient = null;
            ScanningPageUtility.EnsureUploadScanRestClient();
            LoadAccountInfo();
            SyncAnyExistingTransaction();
            if ( ScanningPageUtility.KeepScanning )
            {
                // spCaptureAmount.Visibility = Visibility.Visible;
                ShowStartupPage();
                InitializeFirstScan();
            }
            else
            {
                //spCaptureAmount.Visibility = Visibility.Collapsed;
                lvAccounts.IsEnabled = false;
            }
        }

        private void InitalizeControls()
        {
            this.btnComplete.Visibility = Visibility.Hidden;
            spCheckDisplay.Visibility = Visibility.Collapsed;
            this.btnNext.Visibility = Visibility.Visible;
            this.btnNext.IsEnabled = false;
            SetAmountlegendInvalid( false );
            this.grdImageThumbnailsButtons.Visibility = Visibility.Collapsed;
        }

        private void SyncAnyExistingTransaction()
        {
            decimal sum = 0;
            ScanningPageUtility.KeepScanning = ( ScanningPageUtility.CurrentFinacialTransactions.Count < ScanningPageUtility.ItemsToProcess);
            foreach ( var financialTransacation in ScanningPageUtility.CurrentFinacialTransactions )
            {
                var transactionDeatails = financialTransacation.TransactionDetails;
                if (transactionDeatails != null )
                {
                    foreach ( var transactionDetail in transactionDeatails )
                    {
                        sum += transactionDetail.Amount;
                    }
                }
                
                  ScanningPageUtility.ItemsScanned++;
                  ScanningPageUtility.ItemsUploaded++;
            }

            ScanningPageUtility.TotalAmountScanned += sum;
            UpdateProgressBars( ScanningPageUtility.ItemsScanned );
       

        }

        /// <summary>
        /// Displays the scanned document information.
        /// </summary>
        /// <param name="scannedDocInfo">The scanned document information.</param>
        private void DisplayScannedDocInfo( ScannedDocInfo scannedDocInfo )
        {
            if ( scannedDocInfo.FrontImageData != null )
            {
                this.HideDisplayMessage();
                spCheckDisplay.Visibility = Visibility.Visible;

                lvAccounts.IsEnabled = true;
                BitmapImage bitmapImageFront = new BitmapImage();
                bitmapImageFront.BeginInit();
                bitmapImageFront.StreamSource = new MemoryStream( scannedDocInfo.FrontImageData );
                bitmapImageFront.EndInit();
                imgCheckFront.Source = bitmapImageFront;
                Rock.Wpf.WpfHelper.FadeIn( imgCheckFront, 100 );
            }
            else
            {
                imgCheckFront.Source = null;
            }

            if ( scannedDocInfo.BackImageData != null )
            {
                this._isDoubleSided = true;
                grdImageThumbnailsButtons.Visibility = Visibility.Visible;
                BitmapImage bitmapImageBack = new BitmapImage();
                bitmapImageBack.BeginInit();
                bitmapImageBack.StreamSource = new MemoryStream( scannedDocInfo.BackImageData );
                bitmapImageBack.EndInit();
                imgCheckBack.Source = bitmapImageBack;
                Rock.Wpf.WpfHelper.FadeIn( imgBackThumb, 100 );
            }
            else
            {
                this._isDoubleSided = false;
                grdImageThumbnailsButtons.Visibility = Visibility.Collapsed;

            }

            if ( scannedDocInfo.IsCheck )
            {
                CheckImages.Visibility = Visibility.Visible;
                this.lblRoutingAccountValue.Content = string.Format( "{0} / {1}", scannedDocInfo.RoutingNumber, scannedDocInfo.AccountNumber );
                //lblOtherData.Content = scannedDocInfo.OtherData;
                // spOtherData.Visibility = !string.IsNullOrWhiteSpace( scannedDocInfo.OtherData ) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private double GetImageWidth()
        {
            if ( _isDoubleSided )
            {
                return this.RenderSize.Width * .6;
            }
            return this.RenderSize.Width * .9;
        }


        private void ShowNoAmountMessage()
        {
            MessageBox.Show( "No values have been entered", "No Amount Entered", MessageBoxButton.OK );
            lvAccounts.SelectedIndex = 1;
            lvAccounts.SelectedIndex = 0;
        }
        private void BtnIgnoreAndUpload_Click( object sender, RoutedEventArgs e )
        {
            if (!HasAmount())
            {
                ShowNoAmountMessage();
                return;
            }
            HideUploadWarningPrompts();
            
            
            var scannedDocInfo = ScanningPageUtility.ConfirmUploadBadScannedDoc;
            scannedDocInfo.Upload = true;
            ScanningPageUtility.UploadScannedItem( scannedDocInfo,(itemCount)=> { this.UpdateProgressBars( ScanningPageUtility.ItemsUploaded ); } );
            this.btnNext.IsEnabled = true;
            this.BtnNext_Click( sender, e );

        }

        private bool HasAmount()
        {
            foreach ( DisplayAccountValueModel account in lvAccounts.Items )
            {
                if ( account.Amount != null && account.Amount > 0 )
                {
                    return true;

                }    
            }
            return false;    
        }

        /// <summary>
        /// Handles the Click event of the BtnSkipAndContinue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnSkipAndContinue_Click( object sender, RoutedEventArgs e )
        {
            HideUploadWarningPrompts();
            ScanningPageUtility.ConfirmUploadBadScannedDoc = null;
            ScanningPageUtility.ItemsSkipped++;
            ScanningPageUtility.ResumeScanning();
        }

        internal void rangerScanner_TransportIsDead( object sender, EventArgs e )
        {
            ScanningPageUtility.rangerScanner_TransportIsDead( sender, e,() => { DisplayMessage( "Warning", "labelStyleBannerTitle", "Click Next to begin", "labelStyleAlert", "Scanner is not ready. Verify that the scanner is powered on and connected." ); } );
        }

        /// <summary>
        /// Adds the scanned doc to a history of scanned docs, and shows info and status.
        /// </summary>
        /// <param name="scannedDocInfo">The scanned check info.</param>
        public void ShowScannedDocStatusAndUpload( ScannedDocInfo scannedDocInfo )
        {
            this.HideDisplayMessage();
            bool messsageHandled = false;

            DisplayScannedDocInfo( scannedDocInfo );

            var rockConfig = RockConfig.Load();

            bool scanningChecks = rockConfig.TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();

            // if they don't enable smart scan, don't warn about bad micr's. For example, they might be scanning a mixture of checks and envelopes
            if ( rockConfig.EnableSmartScan )
            {
                if ( scannedDocInfo.BadMicr )
                {
                    string message;
                    if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.RangerApi )
                    {
                        
                         message = @"Unable to read check information
                                Click 'Skip' to reject this check and continue scanning.
                                To retry this check, put the check back into the feed tray.

                                Click 'Upload' to upload the check as-is.
                                Click 'Stop' to reject this check and stop scanning.";
                    }
                    else
                    {
                        message = @"Unable to read check information
                                    Click 'Skip' to reject this check.    
                                    Click 'Upload' to upload the check as-is.";
                        
                    }
                    messsageHandled = true;

                    this.DisplayMessage( "Warning", mainMessageStyleKey: "WarningTextStyle", mainMessage: message );
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
                this.DisplayMessage( "Warning", mainMessageStyleKey: "labelStyleAlertInfo", mainMessage: "A check with the same account information and check number has already been scanned." );
            }

            if ( scannedDocInfo.Upload )
            {
                this._currentscannedDocInfo = scannedDocInfo;
            }
            else
            {
                ShowUploadWarnings( scannedDocInfo );      
            }
        }


        /// <summary>
        /// Hides the warnings messages and prompts
        /// </summary>
        private void HideUploadWarningPrompts( bool hideWarningMessages = true )
        {
            if ( !hideWarningMessages )
            {
                btnNext.IsEnabled = false;
            }
           
            lblScanItemUploadSuccess.Visibility = Visibility.Collapsed;
            this.HideDisplayMessage();
            pnlPromptForUpload.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Initializes the first scan.
        /// When the page loads an initial check will be scanned then paused to allow
        /// user to enter in Amount and Select Next button to proceed.
        /// </summary>
        private void InitializeFirstScan()
        {
            this.HideDisplayMessage();
            ScanningPageUtility.KeepScanning = true;
            lvAccounts.IsEnabled = true;
            ScanningPageUtility.ResumeScanning();
        }
        

        /// <summary>
        /// Loads the account information.
        /// GetData api/FinancialAccounts
        /// </summary>
        private void LoadAccountInfo()
        {
            RockConfig rockConfig = RockConfig.Load();
            InitializeProgressBarsOfSelectedBatch(rockConfig);
            var client = new RockRestClient( rockConfig.RockBaseUrl );
            client.Login( rockConfig.Username, rockConfig.Password );
            var allAccounts = client.GetData<List<FinancialAccount>>( "api/FinancialAccounts" );
            LoadFilteredAccountsIntoListView( allAccounts, rockConfig );

        }

        private void InitializeProgressBarsOfSelectedBatch(RockConfig rockConfig)
        {

            ScanningPageUtility.TotalAmountScanned = 0;
            var selectedBatch = ScanningPageUtility.batchPage.SelectedFinancialBatch;

            if ( ScanningPageUtility.batchPage.SelectedFinancialBatch != null )
            {
                if ( rockConfig.RequireControlAmount )
                {

                    ScanningPageUtility.BatchAmount = selectedBatch.ControlAmount;
                    this.pbControlAmounts.Minimum = 0;
                    this.pbControlAmounts.Maximum = 100;
                    this.spControlAmountProgressBar.Visibility = Visibility.Visible;

                }
                else
                {
                    this.spControlAmountProgressBar.Visibility = Visibility.Collapsed;

                }

                if ( rockConfig.RequireControlItemCount )
                {
                    //Control Amount
                    ScanningPageUtility.BatchAmount = selectedBatch.ControlAmount;
                    //Control Item Count
                    ScanningPageUtility.ItemsToProcess = selectedBatch.ControlItemCount;
                    this.pbControlItems.Minimum = 0;
                    this.pbControlItems.Maximum = ScanningPageUtility.ItemsToProcess != null ? ( int ) ScanningPageUtility.ItemsToProcess : 0;
                    this.spControlItemProgressbar.Visibility = Visibility.Visible;
                    this.UpdateProgressBarLegend();
                }
                else
                {
                    this.spControlItemProgressbar.Visibility = Visibility.Collapsed;
                }
            }

        }

        internal void ShowScannerStatus( RangerTransportStates xportState, Color statusColor, string status )
        {
            ScanningPageUtility.ShowScannerStatus( xportState, statusColor, status, ref this.shapeStatus );
        }

        /// <summary>
        /// Updates the progress bar legend.
        /// </summary>
        private void UpdateProgressBarLegend()
        {
            var remainingItems = ScanningPageUtility.ItemsToProcess - ScanningPageUtility.ItemsUploaded;
            if ( spControlItemProgressbar.Visibility == Visibility.Visible )
            {
                //Control Items
                this.lblItemScannedValue.Content = string.Format( "{0} of {1}", ScanningPageUtility.ItemsUploaded, ScanningPageUtility.ItemsToProcess );

            }

            if ( spControlAmountProgressBar.Visibility == Visibility.Visible )
            {
                var currentTotals = ScanningPageUtility.TotalAmountScanned + SumAllAccountEntries();
                if ( ScanningPageUtility.TotalAmountScanned > ScanningPageUtility.BatchAmount )
                {
                    pbControlAmounts.Value = 100;
                    SetAmountlegendInvalid( true );
                }

                var amountRemaining = string.Format( new System.Globalization.CultureInfo( "en-US" ), "{0:C}", ScanningPageUtility.BatchAmount - ScanningPageUtility.TotalAmountScanned );
                var currentTotal = string.Format( new System.Globalization.CultureInfo( "en-US" ), "{0:C}", currentTotals );
                this.lblAmountRemaininValue.Content = string.Format( "{0} of {1}", currentTotal, amountRemaining );
            }

            if ( remainingItems == 0 )
            {
                if ( ScanningPageUtility.BatchAmount != ScanningPageUtility.TotalAmountScanned )
                {
                    SetAmountlegendInvalid( true );
                }

                this.pbControlAmounts.Value = 100;
                this.btnComplete.Visibility = Visibility.Visible;
                this.btnNext.Visibility = Visibility.Hidden;
                ScanningPageUtility.KeepScanning = false;
            }
        }

        private void SetAmountlegendInvalid( bool isInvalid )
        {
            var defaultbrush = ( SolidColorBrush ) ( new BrushConverter().ConvertFrom( "#ee7725" ) );
            lblAmountRemaininValue.Foreground = isInvalid ? Brushes.Red : defaultbrush;
            pbControlAmounts.Foreground = isInvalid ? Brushes.Red : defaultbrush;
        }

        /// <summary>
        /// Filters the accounts by configured.
        /// Returns only the values checked in the Rock config settings
        /// </summary>
        /// <param name="allAccounts">All accounts.</param>
        /// <param name="rockConfig">The rock configuration.</param>
        private void LoadFilteredAccountsIntoListView( List<FinancialAccount> allAccounts, RockConfig rockConfig )
        {
            var filteredAccounts = allAccounts.Where( a => rockConfig.SelectedAccountForAmountsIds.Contains( a.Id ) );
            var displayAccountValues = new ObservableCollection<DisplayAccountValueModel>();
            int index = 0;
            foreach ( var account in filteredAccounts )
            {

                displayAccountValues.Add( new DisplayAccountValueModel { Account = account,AccountDisplayName = account.Name, Index = index } );
                index++;
            }

            this._displayAccountValuesContext = displayAccountValues;
            lvAccounts.ItemsSource = this._displayAccountValuesContext;
            lvAccounts.SelectedIndex = 0;

        }


        private decimal SumAllAccountEntries()
        {
            decimal sum = 0;
            if ( this._displayAccountValuesContext != null )
            {
                foreach ( DisplayAccountValueModel accountValue in this._displayAccountValuesContext )
                {
                    if ( accountValue.Amount != null )
                    {
                        sum += (decimal)accountValue.Amount;
                    }
                }
                ScanningPageUtility.TotalAmountScanned += sum;
            }
            return sum;
        }
      
        #region Button Click Events

        private void BtnClose_Click( object sender, System.Windows.RoutedEventArgs e )
        {
            this.NavigationService.Navigate( ScanningPageUtility.batchPage );
        }

        private void BtnComplete_Click( object sender, System.Windows.RoutedEventArgs e )
        {
            this.NavigationService.Navigate( ScanningPageUtility.batchPage );

        }

        private void BtnNext_Click( object sender, System.Windows.RoutedEventArgs e )
        {

            if (!HasAmount() && this.spAlert.Visibility != Visibility.Visible)
            {
                ShowNoAmountMessage();
                return;
            }

            HandleCurrentDocInfo( () =>
             {
                 this._currentscannedDocInfo = null;
                 this.ResetUIForNextScan();
                 var remainingItemsToScan = ScanningPageUtility.ItemsToProcess - ScanningPageUtility.ItemsUploaded;
                 if ( remainingItemsToScan > 0 )
                 {
                    ScanningPageUtility.ResumeScanning();
                 }
             } );

        }

        /// <summary>
        /// Clears the previous check values.
        /// Set the controls back to 0 for next check
        /// </summary>
        private void ResetUIForNextScan()
        {
            this.scrollViewerCheckImage.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            spCheckDisplay.Visibility = Visibility.Collapsed;
            foreach ( DisplayAccountValueModel item in lvAccounts.Items )
            {
                item.Amount = null;
            }
            lvAccounts.SelectedIndex = 0;
            this.lblRoutingAccountValue.Content = string.Empty;
            this.lblCheckNumber.Content = string.Empty;
        }

        private void HandleCurrentDocInfo( Action callback )
        {
            if ( this._currentscannedDocInfo != null )
            {
                ScanningPageUtility.UploadScannedItem( _currentscannedDocInfo,accounts:this._displayAccountValuesContext.ToList() );
                this.UpdateProgressBars( ScanningPageUtility.ItemsUploaded );
            }

            callback.DynamicInvoke();

        }

        #endregion

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

            btnClose.IsEnabled = true;

            if ( rangerFeedingStoppedReason != RangerFeedingStoppedReasons.FeedRequestFinished )
            {
                bool scanningChecks = RockConfig.Load().TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
                var noItemfound = string.Format( "No {0} detected in scanner. Make sure {0} are properly in the feed tray.", scanningChecks ? "checks" : "items" );

                this.spCheckDisplay.Visibility = Visibility.Collapsed;
                // show the Startup Info "Welcome" message if no check images are shown yet
                if ( spCheckDisplay.Visibility != Visibility.Visible )
                {

                    DisplayMessage( "Warning", "labelStyleBannerTitle", "Click Next to begin", "labelStyleAlert", noItemfound );
                    lvAccounts.IsEnabled = false;
                    btnNext.IsEnabled = true;
                    return;
                }

                // show a "No Items" warning if they clicked Start but it stopped because of MainHopperEmpty
                if ( rangerFeedingStoppedReason == RangerFeedingStoppedReasons.MainHopperEmpty )
                {
                    DisplayMessage( "Warning", mainMessage: noItemfound );

                    imgCheckFront.Source = null;
                    this.imgCheckBack.Visibility = Visibility.Collapsed;
                    this.imgCheckFront.Visibility = Visibility.Collapsed;
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
            //this.spAlert.Visibility = Visibility.Collapsed;
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportFeedingState", DateTime.Now.ToString( "o" ) ) );
            btnClose.IsEnabled = false;
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
                this.spAlert.Visibility = Visibility.Collapsed;

                RockConfig rockConfig = RockConfig.Load();

                ScannedDocInfo scannedDoc = new ScannedDocInfo();

                // mark it as Upload, but we'll set it to false if anything bad happens before we actually upload
                scannedDoc.Upload = true;
                scannedDoc.CurrencyTypeValue = ScanningPageUtility.batchPage.SelectedCurrencyValue;
                scannedDoc.SourceTypeValue = ScanningPageUtility.batchPage.SelectedSourceTypeValue;

                scannedDoc.FrontImageData = ScanningPageUtility.GetImageBytesFromRanger( RangerSides.TransportFront );

                if ( rockConfig.EnableRearImage )
                {
                    scannedDoc.BackImageData = ScanningPageUtility.GetImageBytesFromRanger( RangerSides.TransportRear );
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
                    ScanningPageUtility.ShowException( ex,this.lblExceptions );
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
                    bool scanningChecks = RockConfig.Load().TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
                    var promptToScanRearImage = string.Format( "No {0} detected in scanner. Make sure {0} are properly in the feed tray.", scanningChecks ? "checks" : "items" );

                    this.DisplayMessage( "Warning", mainMessageStyleKey: "labelStyleAlert", mainMessage: promptToScanRearImage);

                    HideUploadWarningPrompts( true );
                    DisplayScannedDocInfo( scannedDoc );
                }
                else
                {
                    // scanned both sides (or just the front if they don't want to scan both sides )
                    this.HideDisplayMessage();
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
                    ScanningPageUtility.ShowException( ex,this.lblExceptions );
                }
            }
            finally
            {
                ScanningPageUtility.batchPage.micrImage.ClearBuffer();
            }
        }

        #endregion

        /// <summary>
        /// Shows the upload warnings.
        /// </summary>
        private void ShowUploadWarnings( ScannedDocInfo scannedDocInfo )
        {
            var rockConfig = RockConfig.Load();

            ScanningPageUtility.ConfirmUploadBadScannedDoc = scannedDocInfo;
            lblScanItemUploadSuccess.Visibility = Visibility.Collapsed;
            pnlPromptForUpload.Visibility = scannedDocInfo.Duplicate || scannedDocInfo.BadMicr ? Visibility.Visible : Visibility.Collapsed;
            this.spAlert.Visibility = pnlPromptForUpload.Visibility;
            btnClose.IsEnabled = true;
            btnNext.IsEnabled = false;
            this.scrollViewerCheckImage.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        /// <summary>
        /// Shows the startup page.
        /// </summary>
        private void ShowStartupPage()
        {
           // var rockConfig = RockConfig.Load();
           // HideUploadWarningPrompts();
           // lblExceptions.Visibility = Visibility.Collapsed;
           // spAlert.Visibility = Visibility.Collapsed;

           // ScannedDocInfo sampleDocInfo = new ScannedDocInfo();
           // sampleDocInfo.CurrencyTypeValue = ScanningPageUtility.batchPage.CurrencyValueList.FirstOrDefault( a => a.Guid == RockConfig.Load().TenderTypeValueGuid.AsGuid() );
           // DisplayScannedDocInfo( sampleDocInfo );

           // bool scanningChecks = RockConfig.Load().TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
           // var messageCaption = string.Format( "No {0} detected in scanner. Make sure {0} are properly in the feed tray.", scanningChecks ? "checks" : "items" );
           //var message = string.Format( "Insert the {0} again facing the other direction to get an image of the back.", scanningChecks ? "check" : "item" );

  
        }
        private void UpdateProgressBars( int itemsUploaded )
        {
            if ( spControlItemProgressbar.Visibility == Visibility.Visible )
            {
                this.pbControlItems.Value = itemsUploaded;
            }
            if ( spControlAmountProgressBar.Visibility == Visibility.Visible )
            {

                this.pbControlAmounts.Value = ScanningPageUtility.GetPercentageAmountComplete();
            }

            this.UpdateProgressBarLegend();


        }
        /// <summary>
        /// Handles the Loaded event of the TextBox control.
        /// Need to Set Focus on the First Element is the list view and more specific the text box
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TextBox_Loaded( object sender, RoutedEventArgs e )
        {
            var textbox = sender as System.Windows.Controls.TextBox;
            if ( textbox != null )
            {
                var dataContext = textbox.DataContext as DisplayAccountValueModel;
                if ( dataContext != null )
                {
                    if ( dataContext.Index == 0 )
                    {
                        textbox.Focus();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the KeyDown event of the TextBox control.
        /// This allow for user to process enter key
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void TextBox_KeyDown( object sender, System.Windows.Input.KeyEventArgs e )
        {
            var textbox = sender as TextBox;
            if ( e.Key == Key.Return )
            {
                if ( !string.IsNullOrEmpty( textbox.Text ) )
                {
                    var dataContext = textbox.DataContext as DisplayAccountValueModel;
                    dataContext.Amount = decimal.Parse( textbox.Text );
                }
                if ( pnlPromptForUpload.Visibility != Visibility.Visible )
                {
                    this.BtnNext_Click( sender, e );
                }
            }

        }

        /// <summary>
        /// Handles the TextChanged event of the TextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void TextBox_TextChanged( object sender, TextChangedEventArgs e )
        {
            if ( spAlert.Visibility != Visibility.Visible )
            {
                btnNext.IsEnabled = true;
            }
        }

        private void BtnImageToggle_FrontBack_Click( object sender, RoutedEventArgs e )
        {
            var parameter = ( ( Button ) e.OriginalSource ).CommandParameter;
            switch ( parameter )
            {
                case "Front":
                    this.imgCheckBack.Visibility = Visibility.Collapsed;
                    this.imgCheckFront.Visibility = Visibility.Visible;
                    break;
                case "Back":
                    this.imgCheckBack.Visibility = Visibility.Visible;
                    this.imgCheckFront.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void Page_SizeChanged( object sender, SizeChangedEventArgs e )
        {
            this.lvAccounts.Height = this.RenderSize.Height - 90;
        }

        private void DisplayMessage( string messageType, string captionstyleKey = "", string captionMessage = "", string mainMessageStyleKey = "", string mainMessage = "" )
        {
            this.spAlert.Visibility = Visibility.Visible;
            Style captionStyle = null;
            switch ( messageType )
            {
                case "Warning":
                   spAlert.Style = Application.Current.Resources["stackPanelWarningtStyle"] as Style;
                   lblAlertCaption.Style = Application.Current.Resources["WarningCaptionSytle"] as Style;
                    lblAlertCaption.Content = "Warning!";
                    borderAlertBorder.Style = Application.Current.Resources["borderWarningStyle"] as Style;
                    break;
                case "Alert":
                    spAlert.Style = Application.Current.Resources["stackPanelAlertStyle"] as Style;
                    lblAlertCaption.Style = Application.Current.Resources["alertCaptionSytle"] as Style;
                    lblAlertCaption.Content = "Alert!";
                    borderAlertBorder.Style = Application.Current.Resources["borderAlertgStyle"] as Style;
                    lblAlertSubMessage.Style = Application.Current.Resources["AlertTextStyle"] as Style;
                    break;
                default:
                    break;
            }

            //Handle Caption
            if ( !string.IsNullOrEmpty( captionstyleKey ) )
            {
                captionStyle = Application.Current.Resources[captionstyleKey] as Style;
                if ( captionStyle != null )
                {
                    this.lblAlertCaptionMessage.Style = captionStyle;
                }
            }

            if ( !string.IsNullOrEmpty( captionMessage ) )
            {
                this.spAlertMessage.Visibility = Visibility.Visible;
                this.lblAlertCaptionMessage.Content = captionMessage;
            }

            if ( !string.IsNullOrEmpty( mainMessageStyleKey ) )
            {
                var subStyle = Application.Current.Resources[mainMessageStyleKey] as Style;
                if ( subStyle != null )
                {
                    this.lblAlertSubMessage.Style = subStyle;
                }
            }

            if ( !string.IsNullOrEmpty( mainMessage ) )
            {
                this.spAlertMessage.Visibility = Visibility.Visible;
                this.lblAlertSubMessage.Content = mainMessage;

            }
        }
        private void HideDisplayMessage()
        {
            this.spAlertMessage.Visibility = Visibility.Collapsed;
        }

        private void btnOptions_Click( object sender, RoutedEventArgs e )
        {
            ScanningPageUtility.batchPage.NavigateToOptionsPage();
        }
    }
}
