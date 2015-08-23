using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PresentationBus.Tests
{
    [TestClass]
    public class HasSubscriberScenario
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
        public async Task GivenASubscriberTheEventGetsHandled()
        {
            await _bus.PublishAsync(new TestEvent());
        }

        public class TestEvent : PresentationEvent
        { }

        public class TestSubscriber : IHandlePresentationEvent<TestEvent>
        {
            public int HandledCount { get; set; }

            public void Handle(TestEvent presentationEvent)
            {
                HandledCount++;
            }
        }
    }
}