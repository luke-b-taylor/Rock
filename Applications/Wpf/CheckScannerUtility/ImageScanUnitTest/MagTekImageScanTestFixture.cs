using System;
using System.Linq;
using System.Threading.Tasks;
using ImageScanInteropBuilder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageScanUnitTest
{
    /// <summary>
    /// Manually enable each of these test by removing the Ignore Attribute
    /// The intent of this test fixture is to test actually MagTek Image Safe functionality
    /// With an Actual MagTek ImageSafe Scanner
    /// </summary>
    [TestClass]
    public class MagTekImageScanTestFixture
    {
        private TestContext m_testContext;
        private MagTekUsbScanner _magTekImageScan;
        private MagTekUsbScanner.oCheckData _checkData;

        [TestInitialize]
        public void TestInitailize()
        {
            _magTekImageScan = new MagTekUsbScanner( TestContext.TestDir );
        }
        [TestMethod]
        [Ignore]
        public void GetDeviceNames()
        {
            //Arrange
           
            //Act
            var deviceList = _magTekImageScan.GetDeviceList();
            //Assert
            Assert.IsNotNull( deviceList );
        }

        [TestMethod]
        [Ignore]  
        public void PortOpen_Returns_True()
        {
            Task task = Task.Run(() => {

                var deviceList = _magTekImageScan.GetDeviceList();
                Assert.IsNotNull( deviceList );
                Assert.IsTrue( deviceList.Count > 0 );
                var deviceName = deviceList.Where( x => x.Contains( "ImageSafe" ) );
                //Act
                var result = _magTekImageScan.PortOpen;
                _magTekImageScan.ClosePort();

                //Assert
                Assert.IsTrue( result );


            } );
            //Arrange

            TimeSpan ts = TimeSpan.FromMilliseconds( 150);
            Task.WaitAll( task );
      
            task.Dispose();
        }

        /// <summary>
        /// Note: you must attach a MagTek image Safe and have a check in tray
        /// </summary>
        [TestMethod]
        [Ignore]
        public void ProcessDocumentReturnsTrue()
        {
            _magTekImageScan.OnDocumentProcessComplete += (s,e) => {
                
                Assert.IsNotNull(e);
                Assert.IsFalse(e.HasError );
                Assert.IsNotNull( e.AccountNumber );
                Assert.IsNotNull( e.CheckNumber );
                Assert.IsNotNull( e.ImageData );
                Assert.IsNotNull( e.MaskedAccountNumber );
                Assert.IsNotNull( e.RoutingNumber );
                Assert.IsNotNull( e.OtherData );
            };
            _magTekImageScan.ProcessDocument();
        }

        [TestMethod]
        [Ignore]
        public void ProcessDocumentReturnsNoCheck()
        {
            _magTekImageScan.OnDocumentProcessComplete += ( s, e ) => {

                Assert.IsNotNull( e );
                Assert.IsTrue( e.HasError );
                Assert.IsNotNull( e.Errors );
                Assert.IsTrue( e.Errors.ToString().Contains("") );
                Assert.IsNull( e.AccountNumber );
                Assert.IsNull( e.CheckNumber );
                Assert.IsNull( e.ImageData );
                Assert.IsNull( e.RoutingNumber );
                Assert.IsNull( e.OtherData );
            };
            _magTekImageScan.ProcessDocument();
        }

        public TestContext TestContext
        {
            get { return m_testContext; }
            set { m_testContext = value; }
        }
        [TestCleanup]
        public void TestCleanup()
        {
            _magTekImageScan = null;      
        }
    }
}

