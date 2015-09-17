using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PresentationBus.Tests
{
    [TestClass]
    public class RequestHandledBySyncAndAsyncHandlersScenario
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
            var result = await _bus.Request(new TestRequest());
            Assert.AreEqual("Harry", result.Name);
        }

        [TestMethod]
        public async Task GivenSubscribersThatHandleTheRequestFredIsReturned()
        {
            var result = await _bus.Request(new TestRequestA());
            Assert.AreEqual("Fred", result.Name);
        }

        public class TestRequest : PresentationRequest<TestRequest, TestResponse>
        { }

        public class TestResponse : IPresentationResponse
        {
            public string Name { get; set; }
        }
        public class TestRequestA : PresentationRequest<TestRequestA, TestResponseA>
        { }

        public class TestResponseA : IPresentationResponse
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
        public class TestSubscriber2 : IHandlePresentationRequestAsync<TestRequestA, TestResponseA>
        {
            public async Task<TestResponseA> HandleAsync(TestRequestA request)
            {
                var result = await Task.Run(() => new TestResponseA { Name = "Fred" });
                return result;
            }
        }
    }
}