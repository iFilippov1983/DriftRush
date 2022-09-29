using RaceManager.Vehicles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Root
{
    public class Driver : MonoBehaviour
    {
        [SerializeField] private DriverType _driverType;
        [SerializeField] private CarAIControl _carAI;

        private void OnEnable()
        {
            RaceEventsHub.Instance.Subscribe(RaceEventType.START, StartRace);
            RaceEventsHub.Instance.Subscribe(RaceEventType.STOP, StopRace);
        }

        private void OnDisable()
        {
            RaceEventsHub.Instance.Unsunscribe(RaceEventType.START, StartRace);
            RaceEventsHub.Instance.Unsunscribe(RaceEventType.STOP, StopRace);
        }

        private void StartRace()
        { 
        
        }

        private void StopRace()
        { 
        
        }

        private void GetCar()
        { 
            
        }
    }
}


