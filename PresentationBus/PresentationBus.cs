using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PresentationBus
{
    public class PresentationBus : IPresentationBus, IPresentationBusConfiguration
    {
        private readonly Dictionary<Type, EventSubscribers> _subscribersByEventType;
        private readonly Dictionary<Type, RequestSubscribers> _subscribersByRequestType;
        private readonly Dictionary<Type, CommandSubscribers> _subscribersByCommandType;

        public PresentationBus()
        {
            _subscribersByEventType = new Dictionary<Type, EventSubscribers>();
            _subscribersByRequestType = new Dictionary<Type, RequestSubscribers>();
            _subscribersByCommandType = new Dictionary<Type, CommandSubscribers>();
        }

        public void Subscribe(IHandlePresentationMessages instance)
        {
            var handlesEvents = instance as IHandlePresentationEvents;
            if (handlesEvents != null)
                DoSubscribe(handlesEvents);

            var handlesCommands = instance as IHandlePresentationCommands;
            if (handlesCommands != null)
                DoSubscribe(handlesCommands);

            var handlesRequests = instance as IHandlePresentationRequests;
            if (handlesRequests != null)
                DoSubscribe(handlesRequests);
        }

        public void UnSubscribe(IHandlePresentationMessages instance)
        {
            var handlesEvents = instance as IHandlePresentationEvents;
            if (handlesEvents != null)
                DoUnSubscribe(handlesEvents);

            var handlesCommands = instance as IHandlePresentationCommands;
            if (handlesCommands != null)
                DoUnSubscribe(handlesCommands);

            var handlesRequests = instance as IHandlePresentationRequests;
            if (handlesRequests != null)
                DoUnSubscribe(handlesRequests);
        }

        private void DoSubscribe(IHandlePresentationEvents instance)
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

        private void DoUnSubscribe(IHandlePresentationEvents instance)
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

        private void DoSubscribe(IHandlePresentationCommands instance)
        {
            ForEachHandledCommand(instance, x => SubscribeForCommands(x, instance));
        }

        private void SubscribeForCommands(Type commandType, object instance)
        {
            CommandSubscribers commandSubscribersForCommandType;

            if (_subscribersByCommandType.ContainsKey(commandType))
            {
                commandSubscribersForCommandType = _subscribersByCommandType[commandType];
            }
            else
            {
                commandSubscribersForCommandType = new CommandSubscribers();
                _subscribersByCommandType.Add(commandType, commandSubscribersForCommandType);
            }

            commandSubscribersForCommandType.AddSubscriber(instance);
        }

        private void DoUnSubscribe(IHandlePresentationCommands instance)
        {
            ForEachHandledCommand(instance, x => UnSubscribeForCommands(x, instance));
        }

        private void UnSubscribeForCommands(Type commandType, object handler)
        {
            if (_subscribersByCommandType.ContainsKey(commandType))
            {
                _subscribersByCommandType[commandType].RemoveSubscriber(handler);
            }
        }

        private void DoSubscribe(IHandlePresentationRequests instance)
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

        private void DoUnSubscribe(IHandlePresentationRequests instance)
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

        public async Task PublishAsync<TEvent>(TEvent presentationEvent) where TEvent : IPresentationEvent
        {
            var type = presentationEvent.GetType();
            var typeInfo = type.GetTypeInfo();
            foreach (var subscribedType in _subscribersByEventType.Keys.Where(t => t.GetTypeInfo().IsAssignableFrom(typeInfo)).ToArray())
            {
                await _subscribersByEventType[subscribedType].PublishAsync(presentationEvent).ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task SendAsync<TCommand>(TCommand presentationEvent) where TCommand : IPresentationCommand
        {
            var type = presentationEvent.GetType();
            var typeInfo = type.GetTypeInfo();
            foreach (var subscribedType in _subscribersByCommandType.Keys.Where(t => t.GetTypeInfo().IsAssignableFrom(typeInfo)).ToArray())
            {
                await _subscribersByCommandType[subscribedType].SendAsync(presentationEvent).ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task<TResponse> RequestAsync<TRequest, TResponse>(IPresentationRequest<TRequest, TResponse> request)
            where TRequest : IPresentationRequest<TRequest, TResponse>
            where TResponse : IPresentationResponse
        {
            var type = request.GetType();
            var typeInfo = type.GetTypeInfo();
            var result = default(TResponse);
            foreach (var subscribedType in _subscribersByRequestType.Keys.Where(t => t.GetTypeInfo().IsAssignableFrom(typeInfo)).ToArray())
            {
                var results = await _subscribersByRequestType[subscribedType].PublishRequestAsync<TRequest, TResponse>((TRequest)request).ConfigureAwait(continueOnCapturedContext: false);
                result = results.FirstOrDefault();
                if (result != null)
                {
                    return result;
                }
            }
            return result;
        }

        public async Task<IEnumerable<TResponse>> MulticastRequestAsync<TRequest, TResponse>(IPresentationRequest<TRequest, TResponse> request)
            where TRequest : IPresentationRequest<TRequest, TResponse>
            where TResponse : IPresentationResponse
        {
            var type = request.GetType();
            var typeInfo = type.GetTypeInfo();
            var results = new List<TResponse>();
            foreach (var subscribedType in _subscribersByRequestType.Keys.Where(t => t.GetTypeInfo().IsAssignableFrom(typeInfo)).ToArray())
            {
                var result = await _subscribersByRequestType[subscribedType].PublishRequestAsync<TRequest,TResponse>((TRequest)request).ConfigureAwait(continueOnCapturedContext: false);
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

        private void ForEachHandledCommand(IHandlePresentationCommands instance, Action<Type> callback)
        {
            var handlesCommandsType = typeof(IHandlePresentationCommands);

            var type = instance.GetType();

            var interfaceTypes = type.GetTypeInfo().ImplementedInterfaces;
            foreach (var interfaceType in interfaceTypes.Where(x => x.IsConstructedGenericType && handlesCommandsType.GetTypeInfo().IsAssignableFrom(x.GetTypeInfo())))
            {
                var commandType = interfaceType.GenericTypeArguments.First();
                callback(commandType);
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

            public async Task<bool> PublishAsync<TEvent>(TEvent presentationEvent) where TEvent : IPresentationEvent
            {
                var anySubscribersStillListening = false;

                foreach (var subscriber in _subscribers.Where(s => s.Target != null))
                {
                    var syncHandler = subscriber.Target as IHandlePresentationEvent<TEvent>;
                    if (syncHandler != null)
                        syncHandler.Handle(presentationEvent);
                        
                    var asyncHandler = subscriber.Target as IHandlePresentationEventAsync<TEvent>;
                    if (asyncHandler != null)
                        await asyncHandler.HandleAsync(presentationEvent).ConfigureAwait(continueOnCapturedContext: false);

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
                where TRequest : IPresentationRequest<TRequest, TResponse>
                where TResponse : IPresentationResponse
            {
                AddSubscriber((object)instance);
            }
            public void AddSubscriber<TRequest, TResponse>(IHandlePresentationRequestAsync<TRequest, TResponse> instance)
                where TRequest : IPresentationRequest<TRequest, TResponse>
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
                where TRequest : IPresentationRequest<TRequest, TResponse>
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

            public async Task<IEnumerable<TResponse>> PublishRequestAsync<TRequest, TResponse>(TRequest request)
                where TRequest : IPresentationRequest<TRequest, TResponse>
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
                        results.Add(await asyncHandler.HandleAsync(request).ConfigureAwait(continueOnCapturedContext: false));
                    }
                }

                return results;
            }
        }

        internal class CommandSubscribers
        {
            private readonly List<WeakReference> _subscribers;

            public CommandSubscribers()
            {
                _subscribers = new List<WeakReference>();
            }

            public void AddSubscriber<T>(IHandlePresentationCommand<T> instance) where T : IPresentationCommand
            {
                AddSubscriber((object)instance);
            }
            public void AddSubscriber<T>(IHandlePresentationCommandAsync<T> instance) where T : IPresentationCommand
            {
                AddSubscriber((object)instance);
            }
            public void AddSubscriber(object instance)
            {
                if (_subscribers.Any(s => s.Target == instance))
                    return;

                _subscribers.Add(new WeakReference(instance));
            }

            public void RemoveSubscriber<T>(IHandlePresentationCommand<T> instance) where T : IPresentationCommand
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

            public async Task<bool> SendAsync<TEvent>(TEvent presentationEvent) where TEvent : IPresentationCommand
            {
                var anySubscribersStillListening = false;

                foreach (var subscriber in _subscribers.Where(s => s.Target != null))
                {
                    var syncHandler = subscriber.Target as IHandlePresentationCommand<TEvent>;
                    if (syncHandler != null)
                        syncHandler.Handle(presentationEvent);

                    var asyncHandler = subscriber.Target as IHandlePresentationCommandAsync<TEvent>;
                    if (asyncHandler != null)
                        await asyncHandler.HandleAsync(presentationEvent).ConfigureAwait(continueOnCapturedContext: false);

                    anySubscribersStillListening = true;
                }

                return anySubscribersStillListening;
            }
        }
    }
}