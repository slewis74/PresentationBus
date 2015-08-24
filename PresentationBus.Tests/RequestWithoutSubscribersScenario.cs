using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PresentationBus.Tests
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
        public async Task GivenARequestAndNoSubscribersAnEmptyResponseListIsReturned()
        {
            var results = await _bus.MulticastRequestAsync<TestRequest, TestResponse>(new TestRequest());
            Assert.IsFalse(results.Any());
        }

        public class TestRequest : PresentationRequest<TestResponse>
        { }

        public class TestResponse : IPresentationResponse
        { }
    }
}