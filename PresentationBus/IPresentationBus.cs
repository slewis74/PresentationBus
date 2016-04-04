using System.Collections.Generic;
using System.Threading.Tasks;

namespace PresentationBus
{
    public interface IPresentationBus
    {
        Task SendAsync<TCommand>(TCommand command) where TCommand : IPresentationCommand;

        Task PublishAsync<TEvent>(TEvent presentationEvent) where TEvent : IPresentationEvent;

        Task<TResponse> RequestAsync<TRequest, TResponse>(IPresentationRequest<TRequest, TResponse> request)
            where TRequest : IPresentationRequest<TRequest, TResponse>
            where TResponse : IPresentationResponse;

        Task<IEnumerable<TResponse>> MulticastRequestAsync<TRequest, TResponse>(IPresentationRequest<TRequest, TResponse> request)
            where TRequest : IPresentationRequest<TRequest, TResponse>
            where TResponse : IPresentationResponse;
    }
}