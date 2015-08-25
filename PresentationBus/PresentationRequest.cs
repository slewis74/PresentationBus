using System;

namespace PresentationBus
{
    public class PresentationRequest<TRequest, TResponse> : IPresentationRequest<TRequest, TResponse>
        where TRequest : IPresentationRequest<TRequest, TResponse>
        where TResponse : IPresentationResponse
    {
        public PresentationRequest()
        {
            // The self referencing in the TRequest is so the calls to IPresentationBus.MulticastRequestAsync can
            // imply the types from the request object instance.
            if (GetType() != typeof(TRequest))
                throw new ArgumentException("Requests must reference their own type as the first generic type parameter");
        }
    }
}