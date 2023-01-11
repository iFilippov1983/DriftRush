using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace RaceManager.Root
{
    public class EventsHub<T> where T : Enum
    {
        private static readonly Dictionary<T, UnityEvent> _events = new Dictionary<T, UnityEvent>();

        public static void Subscribe(T eventType, UnityAction listener)
        {
            UnityEvent thisEvent;
            if (_events.TryGetValue(eventType, out thisEvent))
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new UnityEvent();
                thisEvent.AddListener(listener);
                _events.Add(eventType, thisEvent);
            }
        }

        public static void Unsunscribe(T eventType, UnityAction listener)
        {
            UnityEvent thisEvent;
            if (_events.TryGetValue(eventType, out thisEvent))
            {
                thisEvent.RemoveListener(listener);
            }
        }

        public static void BroadcastNotification(T eventType)
        {
            UnityEvent thisEvent;
            if (_events.TryGetValue(eventType, out thisEvent))
            {
                thisEvent.Invoke();
            }
        }
    }

    public class EventsHub<T, TValue> where T : Enum 
    {
        private static readonly Dictionary<T, UnityEvent<TValue>> _events = new Dictionary<T, UnityEvent<TValue>>();

        public static void Subscribe(T eventType, UnityAction<TValue> listener)
        {
            UnityEvent<TValue> thisEvent;
            if (_events.TryGetValue(eventType, out thisEvent))
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new UnityEvent<TValue>();
                thisEvent.AddListener(listener);
                _events.Add(eventType, thisEvent);
            }
        }

        public static void Unsunscribe(T eventType, UnityAction<TValue> listener)
        {
            UnityEvent<TValue> thisEvent;
            if (_events.TryGetValue(eventType, out thisEvent))
            {
                thisEvent.RemoveListener(listener);
            }
        }

        public static void BroadcastNotification(T eventType, TValue arg)
        {
            UnityEvent<TValue> thisEvent;
            if (_events.TryGetValue(eventType, out thisEvent))
            {
                thisEvent.Invoke(arg);
            }
        }
    }
}
