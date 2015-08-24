namespace PresentationBus
{
    public interface IPresentationRequest<TResponse>
        where TResponse : IPresentationResponse
    {
    }
}