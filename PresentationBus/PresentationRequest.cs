namespace PresentationBus
{
    public class PresentationRequest<TResponse> : IPresentationRequest<TResponse>
        where TResponse : IPresentationResponse
    {
        public PresentationRequest()
        {
        }
    }
}