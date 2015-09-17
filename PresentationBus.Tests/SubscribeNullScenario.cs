using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PresentationBus.Tests
{
    [TestClass]
    public class SubscribeNullScenario
    {
        private PresentationBus _bus;

        [TestMethod]
        public void SubscribeHandlesNull()
        {
            _bus = new PresentationBus();

            _bus.Subscribe(null);
        }
    }
}