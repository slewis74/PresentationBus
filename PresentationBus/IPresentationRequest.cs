namespace PresentationBus
{
    public interface IPresentationRequest : IPresentationEvent
    {
        /// <summary>
        /// Specifies whether the event must be handled.
        /// </summary>
        bool MustBeHandled { get; }

        /// <summary>
        /// Indicates whether the event has been handled.
        /// </summary>
        bool IsHandled { get; set; }
    }
}