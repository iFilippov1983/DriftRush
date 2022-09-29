using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RaceManager.Root
{
    public class RaceEventsHub : Singleton<RaceEventsHub>
    {
        private static readonly Dictionary<RaceEventType, UnityEvent> _events = new Dictionary<RaceEventType, UnityEvent>();

        public void Subscribe(RaceEventType eventType, UnityAction listener)
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

        public void Unsunscribe(RaceEventType eventType, UnityAction listener)
        {
            UnityEvent thisEvent;
            if (_events.TryGetValue(eventType, out thisEvent))
            { 
                thisEvent.RemoveListener(listener);
            }
        }

        public void Notify(RaceEventType eventType)
        {
            UnityEvent thisEvent;
            if (_events.TryGetValue(eventType, out thisEvent))
            {
                thisEvent.Invoke();
            }
        }
    }
}
