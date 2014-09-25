using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slew.PresentationBus.Tests
{
    [TestClass]
    public class RequestWithoutSubscribersScenario
    {
        private PresentationBus _bus;

        [TestInitialize]
        public void SetUp()
        {
            _bus = new PresentationBus();
        }

        [TestMethod]
        public async Task GivenARequestAndNoSubscribersAnExceptionIsThrown()
        {
            var exceptionWasThrown = false;
            try
            {
                await _bus.PublishAsync(new TestRequest());
            }
            catch (InvalidOperationException)
            {
                exceptionWasThrown = true;
            }
            Assert.IsTrue(exceptionWasThrown);
        }

        public class TestRequest : PresentationRequest
        { }
    }
}