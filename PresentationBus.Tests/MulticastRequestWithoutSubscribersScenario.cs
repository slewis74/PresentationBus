using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PresentationBus.Tests
{
    public class MulticastRequestWithoutSubscribersScenario
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
            var results = await _bus.MulticastRequestAsync(new TestRequest());
            Assert.IsFalse(results.Any());
        }

        public class TestRequest : PresentationRequest<TestRequest, TestResponse>
        { }

        public class TestResponse : IPresentationResponse
        { }
    }
}