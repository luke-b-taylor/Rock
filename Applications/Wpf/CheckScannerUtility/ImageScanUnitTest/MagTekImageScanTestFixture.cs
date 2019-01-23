using System;
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
        public void PortOpen_Returns_True()
        {
            //Act
            var result = _magTekImageScan.PortOpen;
            //Assert
            Assert.IsTrue( result );

        }


        public TestContext TestContext
        {
            get { return m_testContext; }
            set { m_testContext = value; }
        }
        [TestCleanup]
        public void TestCleanup()
        {
            _magTekImageScan.ClosePort();

            this._magTekImageScan = null;
        }
    }
}

