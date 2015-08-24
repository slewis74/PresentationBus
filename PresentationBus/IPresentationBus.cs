using System.Collections.Generic;
using System.Threading.Tasks;

namespace PresentationBus
{
    public interface IPresentationBus
    {
        void Subscribe<T>(IHandlePresentationEvent<T> handler) where T : IPresentationEvent;
        void Subscribe<T>(IHandlePresentationEventAsync<T> handler) where T : IPresentationEvent;
        void Subscribe(IHandlePresentationEvents instance);

        void Subscribe<TRequest, TResponse>(IHandlePresentationRequest<TRequest, TResponse> handler)
            where TRequest : IPresentationRequest<TResponse>
            where TResponse : IPresentationResponse;
        void Subscribe<TRequest, TResponse>(IHandlePresentationRequestAsync<TRequest, TResponse> handler)
            where TRequest : IPresentationRequest<TResponse>
            where TResponse : IPresentationResponse;
        void Subscribe(IHandlePresentationRequests instance);

        void UnSubscribe<T>(IHandlePresentationEvent<T> handler) where T : IPresentationEvent;
        void UnSubscribe<T>(IHandlePresentationEventAsync<T> handler) where T : IPresentationEvent;
        void UnSubscribe(IHandlePresentationEvents instance);

        void UnSubscribe<TRequest, TResponse>(IHandlePresentationRequest<TRequest, TResponse> handler)
            where TRequest : IPresentationRequest<TResponse>
            where TResponse : IPresentationResponse;
        void UnSubscribe<TRequest, TResponse>(IHandlePresentationRequestAsync<TRequest, TResponse> handler)
            where TRequest : IPresentationRequest<TResponse>
            where TResponse : IPresentationResponse;
        void UnSubscribe(IHandlePresentationRequests instance);

        Task PublishAsync<T>(T presentationEvent) where T : IPresentationEvent;

        Task<IEnumerable<TResponse>> MulticastRequestAsync<TRequest, TResponse>(TRequest request)
            where TRequest : IPresentationRequest<TResponse>
            where TResponse : IPresentationResponse;
    }
}