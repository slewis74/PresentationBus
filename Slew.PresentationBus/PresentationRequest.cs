namespace Slew.PresentationBus
{
    public class PresentationRequest : IPresentationRequest
    {
        public PresentationRequest()
        {
            // requests are generally for something to be done, so should require at least
            // 1 subscriber to handle them.
            MustBeHandled = true;
        }

        public bool IsHandled { get; set; }
        public bool MustBeHandled { get; protected set; }
    }
}