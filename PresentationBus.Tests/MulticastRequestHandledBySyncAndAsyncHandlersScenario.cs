using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PresentationBus.Tests
{
    [TestClass]
    public class MulticastRequestHandledBySyncAndAsyncHandlersScenario
    {
        private PresentationBus _bus;
        private IHandlePresentationMessages _subscriber1;
        private IHandlePresentationMessages _subscriber2;

        [TestInitialize]
        public void SetUp()
        {
            _bus = new PresentationBus();
            _subscriber1 = new TestSubscriber1();
            _subscriber2 = new TestSubscriber2();

            _bus.Subscribe(_subscriber1);
            _bus.Subscribe(_subscriber2);
        }

        [TestMethod]
        public async Task GivenSubscribersThatHandleTheRequestHarryIsReturned()
        {
            var results = await _bus.MulticastRequestAsync(new TestRequest());
            Assert.IsNotNull(results.SingleOrDefault(x => x.Name == "Harry"));
        }

        [TestMethod]
        public async Task GivenSubscribersThatHandleTheRequestFredIsReturned()
        {
            var results = await _bus.MulticastRequestAsync(new TestRequest());
            Assert.IsNotNull(results.SingleOrDefault(x => x.Name == "Fred"));
        }

        public class TestRequest : PresentationRequest<TestRequest, TestResponse>
        { }

        public class TestResponse : IPresentationResponse
        {
            public string Name { get; set; }
        }

        public class TestSubscriber1 : IHandlePresentationRequest<TestRequest, TestResponse>
        {
            public TestResponse Handle(TestRequest presentationEvent)
            {
                return new TestResponse { Name = "Harry" };
            }
        }
        public class TestSubscriber2 : IHandlePresentationRequestAsync<TestRequest, TestResponse>
        {
            public async Task<TestResponse> HandleAsync(TestRequest request)
            {
                var result = await Task.Run(() => new TestResponse { Name = "Fred" });
                return result;
            }
        }
    }
}