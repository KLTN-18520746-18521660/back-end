using NUnit.Framework;
using CoreApi;
using System.Threading;

namespace test.CoreApi
{
    class tstCoreApiBase : Program
    {
        Thread main;
        public void InitWithHttps()
        {
            string[] args = { "ssl" };
            Program.Main(args);
        }
        [SetUp]
        public void Setup()
        {
            string[] args = { "ssl" };
            main = new Thread(InitWithHttps);
            main.Start();
        }
        [TearDown]
        public void TearDown()
        {
            main.Interrupt();
            main.Join();
        }
        [Test]
        public void TestConfig()
        {
            Assert.IsTrue(true);
        }
    }
}
