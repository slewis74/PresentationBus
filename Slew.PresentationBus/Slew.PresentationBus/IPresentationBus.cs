using System.Threading.Tasks;

namespace Slew.PresentationBus
{
    public interface IPresentationBus
    {
        void Subscribe<T>(IHandlePresentationEvent<T> handler) where T : IPresentationEvent;
        void Subscribe<T>(IHandlePresentationEventAsync<T> handler) where T : IPresentationEvent;
        void Subscribe(IHandlePresentationEvents instance);

        void UnSubscribe<T>(IHandlePresentationEvent<T> handler) where T : IPresentationEvent;
        void UnSubscribe(IHandlePresentationEvents instance);

        Task PublishAsync<T>(T presentationEvent) where T : IPresentationEvent;
    }
}