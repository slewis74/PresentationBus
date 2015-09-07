using System.Collections.Generic;
using System.Threading.Tasks;

namespace PresentationBus
{
    public interface IPresentationBus
    {
        Task Send<TCommand>(TCommand command) where TCommand : IPresentationCommand;

        Task Publish<TEvent>(TEvent presentationEvent) where TEvent : IPresentationEvent;

        Task<TResponse> Request<TRequest, TResponse>(IPresentationRequest<TRequest, TResponse> request)
            where TRequest : IPresentationRequest<TRequest, TResponse>
            where TResponse : IPresentationResponse;

        Task<IEnumerable<TResponse>> MulticastRequest<TRequest, TResponse>(IPresentationRequest<TRequest, TResponse> request)
            where TRequest : IPresentationRequest<TRequest, TResponse>
            where TResponse : IPresentationResponse;
    }
}