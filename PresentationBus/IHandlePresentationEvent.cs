using System.Threading.Tasks;

namespace PresentationBus
{
    public interface IHandlePresentationEvent<in T> : IHandlePresentationEvents
        where T : IPresentationEvent
    {
        void Handle(T presentationEvent);
    }

    public interface IHandlePresentationEventAsync<in T> : IHandlePresentationEvents
        where T : IPresentationEvent
    {
        Task HandleAsync(T presentationEvent);
    }
}