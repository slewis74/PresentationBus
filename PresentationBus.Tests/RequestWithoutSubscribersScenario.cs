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
        public async Task GivenARequestAndNoSubscribersANullIsReturned()
        {
            var result = await _bus.RequestAsync(new TestRequest());
            Assert.IsNull(result);
        }

        public class TestRequest : PresentationRequest<TestRequest, TestResponse>
        { }

        public class TestResponse : IPresentationResponse
        { }
    }
}