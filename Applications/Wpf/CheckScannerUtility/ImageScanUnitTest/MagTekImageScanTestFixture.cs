using System;
using System.Linq;
using System.Threading.Tasks;
using ImageScanInteropBuilder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageScanUnitTest
{
    [TestClass]
    public class MagTekImageScanTestFixture
    {
        private TestContext m_testContext;
        private MagTekScanner _magTekImageScan;

        [TestInitialize]
        public void TestInitailize()
        {
            _magTekImageScan = new MagTekScanner( TestContext.TestDir );
        }
        [TestMethod]
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
        public void ProcessDocument()
        {

           var success =  _magTekImageScan.ProcessDocument();
            Assert.IsTrue( success );
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

