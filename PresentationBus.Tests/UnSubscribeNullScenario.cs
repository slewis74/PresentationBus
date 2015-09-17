using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PresentationBus.Tests
{
    [TestClass]
    public class UnSubscribeNullScenario
    {
        private PresentationBus _bus;

        [TestMethod]
        public void UnSubscribeHandlesNull()
        {
            _bus = new PresentationBus();

            _bus.UnSubscribe(null);
        }
    }
}