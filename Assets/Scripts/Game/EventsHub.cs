using RaceManager.Race;
using RaceManager.Vehicles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Game
{
    public class EventsHub
    {
        public static readonly Dictionary<CarState, Delegate> CarEvents = new Dictionary<CarState, Delegate>();
        public static readonly Dictionary<RaceEventType, Delegate> RaceEvents = new Dictionary<RaceEventType, Delegate>();

        

        
    }
}
