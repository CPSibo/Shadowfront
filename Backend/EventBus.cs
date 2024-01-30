using System;
using System.Collections.Generic;

namespace Shadowfront.Backend
{
    public interface IEventType
    { }

    public static class EventBus
    {
        private static Dictionary<Type, List<object>> _subscribers = [];

        public static void Subscribe<TEvent>(Action<TEvent> handler)
            where TEvent : IEventType
        {
            var eventType = typeof(TEvent);

            if (!_subscribers.ContainsKey(eventType))
                _subscribers.Add(eventType, []);

            _subscribers[eventType].Add(handler);
        }

        public static void Unsubscribe<TEvent>(Action<TEvent> handler)
            where TEvent : IEventType
        {
            var eventType = typeof(TEvent);

            if (!_subscribers.TryGetValue(eventType, out var handlers))
                return;

            handlers.Remove(handler);
        }

        public static void Emit<TEvent>(TEvent ev)
            where TEvent : IEventType
        {
            var eventType = typeof(TEvent);

            if (!_subscribers.TryGetValue(eventType, out var handlers))
                return;

            foreach(var handler in handlers)
            {
                var typedHandler = handler as Action<TEvent> 
                    ?? throw new Exception($"Handler is of unexpected type. Expected Action<${eventType.FullName}>. Received ${handler.GetType().FullName}");

                typedHandler.Invoke(ev);
            }
        }
    }
}
