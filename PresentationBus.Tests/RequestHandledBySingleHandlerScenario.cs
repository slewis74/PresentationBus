using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PresentationBus.Tests
{
    [TestClass]
    public class RequestHandledBySingleHandlerScenario
    {
        private PresentationBus _bus;
        private IHandlePresentationMessages _subscriber;

        [TestInitialize]
        public void SetUp()
        {
            _bus = new PresentationBus();
            _subscriber = new TestSubscriber();

            _bus.Subscribe(_subscriber);
        }

        [TestMethod]
        public async Task GivenASubscriberThatHandlesTheRequestTheResponseIsReturned()
        {
            var results = await _bus.MulticastRequest(new TestRequest());
            Assert.AreEqual("Harry", results.Single().Name);
        }

        public class TestRequest : PresentationRequest<TestRequest, TestResponse>
        { }

        public class TestResponse : IPresentationResponse
        {
            public string Name { get; set; }
        }

        public class TestSubscriber : IHandlePresentationRequest<TestRequest, TestResponse>
        {
            public TestResponse Handle(TestRequest presentationEvent)
            {
                return new TestResponse { Name = "Harry" };
            }
        }
    }
}