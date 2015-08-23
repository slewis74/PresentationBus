using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PresentationBus.Tests
{
    [TestClass]
    public class RequestNotHandledScenario
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
        public async Task GivenASubscriberThatDoesntHandleTheRequestCorrectlyThenAnExceptionIsThrown()
        {
            var exceptionWasThrown = false;
            try
            {
                await _bus.PublishAsync(new TestRequest());
            }
            catch (InvalidOperationException)
            {
                exceptionWasThrown = true;
            }
            Assert.IsTrue(exceptionWasThrown);
        }

        public class TestRequest : PresentationRequest
        { }

        public class TestSubscriber : IHandlePresentationRequest<TestRequest>
        {
            public int HandledCount { get; set; }

            public void Handle(TestRequest presentationEvent)
            {
                HandledCount++;
            }
        }
    }
}