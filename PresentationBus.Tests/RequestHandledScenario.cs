using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PresentationBus.Tests
{
    [TestClass]
    public class RequestHandledScenario
    {
        private PresentationBus _bus;
        private TestSubscriber _subscriber;

        [TestInitialize]
        public void SetUp()
        {
            _bus = new PresentationBus();
            _subscriber = new TestSubscriber();

            _bus.Subscribe(_subscriber);
        }

        [TestMethod]
        public async Task GivenASubscriberThatHandlesTheRequestCorrectlyThenNoErrorOccurs()
        {
            await _bus.PublishAsync(new TestRequest());
            Assert.AreEqual(1, TestSubscriber.HandledCount);
        }

        public class TestRequest : PresentationRequest
        { }

        public class TestSubscriber : IHandlePresentationRequest<TestRequest>
        {
            public static int HandledCount { get; set; }

            public void Handle(TestRequest presentationEvent)
            {
                HandledCount++;
                presentationEvent.IsHandled = true;
            }
        }
    }
}