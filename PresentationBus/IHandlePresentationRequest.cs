using System.Threading.Tasks;

namespace PresentationBus
{
    public interface IHandlePresentationRequest<in TRequest, out TResponse> : IHandlePresentationRequests
        where TRequest : IPresentationRequest<TRequest, TResponse>
        where TResponse : IPresentationResponse
    {
        TResponse Handle(TRequest request);
    }

    public interface IHandlePresentationRequestAsync<in TRequest, TResponse> : IHandlePresentationRequests
        where TRequest : IPresentationRequest<TRequest, TResponse>
        where TResponse : IPresentationResponse
    {
        Task<TResponse> HandleAsync(TRequest request);
    }
}