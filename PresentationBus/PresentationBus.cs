using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PresentationBus
{
    public class PresentationBus : IPresentationBus
    {
        private readonly Dictionary<Type, EventSubscribers> _subscribersByEventType;
        private readonly Dictionary<Type, RequestSubscribers> _subscribersByRequestType;
 
        public PresentationBus()
        {
            _subscribersByEventType = new Dictionary<Type, EventSubscribers>();
            _subscribersByRequestType = new Dictionary<Type, RequestSubscribers>();
        }

        public void Subscribe<T>(IHandlePresentationEvent<T> handler) where T : IPresentationEvent
        {
            SubscribeForEvents(typeof (T), handler);
        }
        public void Subscribe<T>(IHandlePresentationEventAsync<T> handler) where T : IPresentationEvent
        {
            SubscribeForEvents(typeof (T), handler);
        }

        public void Subscribe(IHandlePresentationEvents instance)
        {
            ForEachHandledEvent(instance, x => SubscribeForEvents(x, instance));
        }

        private void SubscribeForEvents(Type eventType, object instance)
        {
            EventSubscribers eventSubscribersForEventType;

            if (_subscribersByEventType.ContainsKey(eventType))
            {
                eventSubscribersForEventType = _subscribersByEventType[eventType];
            }
            else
            {
                eventSubscribersForEventType = new EventSubscribers();
                _subscribersByEventType.Add(eventType, eventSubscribersForEventType);
            }

            eventSubscribersForEventType.AddSubscriber(instance);
        }

        public void UnSubscribe<T>(IHandlePresentationEvent<T> handler) where T : IPresentationEvent
        {
            UnSubscribeForEvents(typeof (T), handler);
        }
        public void UnSubscribe<T>(IHandlePresentationEventAsync<T> handler) where T : IPresentationEvent
        {
            UnSubscribeForEvents(typeof (T), handler);
        }

        public void UnSubscribe(IHandlePresentationEvents instance)
        {
            ForEachHandledEvent(instance, x => UnSubscribeForEvents(x, instance));
        }

        private void UnSubscribeForEvents(Type eventType, object handler)
        {
            if (_subscribersByEventType.ContainsKey(eventType))
            {
                _subscribersByEventType[eventType].RemoveSubscriber(handler);
            }
        }

        public void Subscribe<TRequest, TResponse>(IHandlePresentationRequest<TRequest, TResponse> handler)
            where TRequest : IPresentationRequest<TResponse>
            where TResponse : IPresentationResponse
        {
            SubscribeForRequests(typeof (TRequest), handler);
        }
        public void Subscribe<TRequest, TResponse>(IHandlePresentationRequestAsync<TRequest, TResponse> handler)
            where TRequest : IPresentationRequest<TResponse>
            where TResponse : IPresentationResponse
        {
            SubscribeForRequests(typeof (TRequest), handler);
        }

        public void Subscribe(IHandlePresentationRequests instance)
        {
            ForEachHandledRequest(instance, x => SubscribeForRequests(x, instance));
        }

        private void SubscribeForRequests(Type requestType, object instance)
        {
            RequestSubscribers requestSubscribersForRequestType;

            if (_subscribersByRequestType.ContainsKey(requestType))
            {
                requestSubscribersForRequestType = _subscribersByRequestType[requestType];
            }
            else
            {
                requestSubscribersForRequestType = new RequestSubscribers();
                _subscribersByRequestType.Add(requestType, requestSubscribersForRequestType);
            }

            requestSubscribersForRequestType.AddSubscriber(instance);
        }

        public void UnSubscribe<TRequest, TResponse>(IHandlePresentationRequest<TRequest, TResponse> handler) 
            where TRequest : IPresentationRequest<TResponse>
            where TResponse : IPresentationResponse
        {
            UnSubscribeForRequests(typeof (TRequest), handler);
        }
        public void UnSubscribe<TRequest, TResponse>(IHandlePresentationRequestAsync<TRequest, TResponse> handler) 
            where TRequest : IPresentationRequest<TResponse>
            where TResponse : IPresentationResponse
        {
            UnSubscribeForRequests(typeof (TRequest), handler);
        }

        public void UnSubscribe(IHandlePresentationRequests instance)
        {
            ForEachHandledRequest(instance, x => UnSubscribeForRequests(x, instance));
        }

        private void UnSubscribeForRequests(Type requestType, object handler)
        {
            if (_subscribersByRequestType.ContainsKey(requestType))
            {
                _subscribersByRequestType[requestType].RemoveSubscriber(handler);
            }
        }

        public async Task PublishAsync<T>(T presentationEvent) where T : IPresentationEvent
        {
            var type = presentationEvent.GetType();
            var typeInfo = type.GetTypeInfo();
            foreach (var subscribedType in _subscribersByEventType.Keys.Where(t => t.GetTypeInfo().IsAssignableFrom(typeInfo)).ToArray())
            {
                await _subscribersByEventType[subscribedType].PublishEvent(presentationEvent);
            }
        }

        public async Task<IEnumerable<TResponse>> MulticastRequestAsync<TRequest, TResponse>(TRequest request)
            where TRequest : IPresentationRequest<TResponse>
            where TResponse : IPresentationResponse
        {
            var type = request.GetType();
            var typeInfo = type.GetTypeInfo();
            var results = new List<TResponse>();
            foreach (var subscribedType in _subscribersByRequestType.Keys.Where(t => t.GetTypeInfo().IsAssignableFrom(typeInfo)).ToArray())
            {
                var result = await _subscribersByRequestType[subscribedType].PublishRequest<TRequest,TResponse>(request);
                results.AddRange(result);
            }
            return results;
        }

        private void ForEachHandledEvent(IHandlePresentationEvents instance, Action<Type> callback)
        {
            var handlesEventsType = typeof(IHandlePresentationEvents);

            var type = instance.GetType();

            var interfaceTypes = type.GetTypeInfo().ImplementedInterfaces;
            foreach (var interfaceType in interfaceTypes.Where(x => x.IsConstructedGenericType && handlesEventsType.GetTypeInfo().IsAssignableFrom(x.GetTypeInfo())))
            {
                var eventType = interfaceType.GenericTypeArguments.First();
                callback(eventType);
            }
        }

        private void ForEachHandledRequest(IHandlePresentationRequests instance, Action<Type> callback)
        {
            var handlesRequestsType = typeof(IHandlePresentationRequests);

            var type = instance.GetType();

            var interfaceTypes = type.GetTypeInfo().ImplementedInterfaces;
            foreach (var interfaceType in interfaceTypes.Where(x => x.IsConstructedGenericType && handlesRequestsType.GetTypeInfo().IsAssignableFrom(x.GetTypeInfo())))
            {
                var eventType = interfaceType.GenericTypeArguments.First();
                callback(eventType);
            }
        }

        internal class EventSubscribers
        {
            private readonly List<WeakReference> _subscribers; 

            public EventSubscribers()
            {
                _subscribers = new List<WeakReference>();
            }

            public void AddSubscriber<T>(IHandlePresentationEvent<T> instance) where T : IPresentationEvent
            {
                AddSubscriber((object)instance);
            }
            public void AddSubscriber<T>(IHandlePresentationEventAsync<T> instance) where T : IPresentationEvent
            {
                AddSubscriber((object)instance);
            }
            public void AddSubscriber(object instance)
            {
                if (_subscribers.Any(s => s.Target == instance))
                    return;

                _subscribers.Add(new WeakReference(instance));
            }

            public void RemoveSubscriber<T>(IHandlePresentationEvent<T> instance) where T : IPresentationEvent
            {
                RemoveSubscriber((object)instance);
            }
            public void RemoveSubscriber(object instance)
            {
                var subscriber = _subscribers.SingleOrDefault(s => s.Target == instance);
                if (subscriber != null)
                {
                    _subscribers.Remove(subscriber);
                }
            }

            public async Task<bool> PublishEvent<T>(T presentationEvent) where T : IPresentationEvent
            {
                var anySubscribersStillListening = false;

                foreach (var subscriber in _subscribers.Where(s => s.Target != null))
                {
                    var syncHandler = subscriber.Target as IHandlePresentationEvent<T>;
                    if (syncHandler != null)
                        syncHandler.Handle(presentationEvent);
                        
                    var asyncHandler = subscriber.Target as IHandlePresentationEventAsync<T>;
                    if (asyncHandler != null)
                        await asyncHandler.HandleAsync(presentationEvent);

                    anySubscribersStillListening = true;
                }

                return anySubscribersStillListening;
            }
        }

        internal class RequestSubscribers
        {
            private readonly List<WeakReference> _subscribers; 

            public RequestSubscribers()
            {
                _subscribers = new List<WeakReference>();
            }

            public void AddSubscriber<TRequest, TResponse>(IHandlePresentationRequest<TRequest, TResponse> instance)
                where TRequest : IPresentationRequest<TResponse>
                where TResponse : IPresentationResponse
            {
                AddSubscriber((object)instance);
            }
            public void AddSubscriber<TRequest, TResponse>(IHandlePresentationRequestAsync<TRequest, TResponse> instance)
                where TRequest : IPresentationRequest<TResponse>
                where TResponse : IPresentationResponse
            {
                AddSubscriber((object)instance);
            }

            public void AddSubscriber(object instance)
            {
                if (_subscribers.Any(s => s.Target == instance))
                    return;

                _subscribers.Add(new WeakReference(instance));
            }

            public void RemoveSubscriber<TRequest, TResponse>(IHandlePresentationRequest<TRequest, TResponse> instance)
                where TRequest : IPresentationRequest<TResponse>
                where TResponse : IPresentationResponse
            {
                RemoveSubscriber((object)instance);
            }
            public void RemoveSubscriber(object instance)
            {
                var subscriber = _subscribers.SingleOrDefault(s => s.Target == instance);
                if (subscriber != null)
                {
                    _subscribers.Remove(subscriber);
                }
            }

            public async Task<IEnumerable<TResponse>> PublishRequest<TRequest, TResponse>(TRequest request)
                where TRequest : IPresentationRequest<TResponse>
                where TResponse : IPresentationResponse
            {
                var results = new List<TResponse>();

                foreach (var subscriber in _subscribers.Where(s => s.Target != null))
                {
                    var syncHandler = subscriber.Target as IHandlePresentationRequest<TRequest, TResponse>;
                    if (syncHandler != null)
                    {
                        results.Add(syncHandler.Handle(request));
                    }

                    var asyncHandler = subscriber.Target as IHandlePresentationRequestAsync<TRequest, TResponse>;
                    if (asyncHandler != null)
                    {
                        results.Add(await asyncHandler.HandleAsync(request));
                    }
                }

                return results;
            }
        }
    }
}