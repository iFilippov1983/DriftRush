using RaceManager.Race;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Vehicles
{
    public class Driver : MonoBehaviour
    {
        public DriverType DriverType;

        private CarAIControl _carAI;
        private CarController _carController;
        private void Awake()
        {
            _carController = GetComponent<CarController>();
            _carAI = GetComponent<CarAIControl>();
            _carAI.SetAccelAmount(_carController.Acceleration);
        }

        private void OnEnable()
        {
            RaceEventsHub.Subscribe(RaceEventType.START, StartRace);
            RaceEventsHub.Subscribe(RaceEventType.STOP, StopRace);
        }

        private void OnDisable()
        {
            RaceEventsHub.Unsunscribe(RaceEventType.START, StartRace);
            RaceEventsHub.Unsunscribe(RaceEventType.STOP, StopRace);
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


