namespace PresentationBus
{
    public interface IPresentationRequest<in TRequest, TResponse>
        where TRequest : IPresentationRequest<TRequest, TResponse>
        where TResponse : IPresentationResponse
    {
    }
}