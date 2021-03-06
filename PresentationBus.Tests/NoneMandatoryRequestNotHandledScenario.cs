using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PresentationBus.Tests
{
    [TestClass]
    public class NoneMandatoryRequestNotHandledScenario
    {
        private PresentationBus _bus;

        [TestInitialize]
        public void SetUp()
        {
            _bus = new PresentationBus();
        }

        [TestMethod]
        public async Task GivenANoneMandatoryRequestAndNoSubscribersThenNoExceptionIsThrown()
        {
            var exceptionWasThrown = false;
            try
            {
                await _bus.MulticastRequestAsync(new TestRequest());
            }
            catch (InvalidOperationException)
            {
                exceptionWasThrown = true;
            }
            Assert.IsFalse(exceptionWasThrown);
        }

        public class TestRequest : PresentationRequest<TestRequest, TestResponse>
        {
            public TestRequest()
            {
            }
        }

        public class TestResponse : IPresentationResponse
        { }
    }
}