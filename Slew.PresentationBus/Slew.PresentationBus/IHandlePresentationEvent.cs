using System.Threading.Tasks;

namespace Slew.PresentationBus
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