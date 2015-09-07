using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PresentationBus.Tests
{
    [TestClass]
    public class UnSubscribedScenario
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
        public async Task GivenASubscriberThatHasBeenUnsubscribedTheEventDoesntGetHandled()
        {
            _bus.UnSubscribe(_subscriber);

            await _bus.Publish(new TestEvent());

            Assert.AreEqual(0, TestSubscriber.HandledCount);
        }

        public class TestEvent : PresentationEvent
        { }

        public class TestSubscriber : IHandlePresentationEvent<TestEvent>
        {
            public static int HandledCount { get; set; }

            public void Handle(TestEvent presentationEvent)
            {
                HandledCount++;
            }
        }
    }
}