using System;
using UnityEngine;

namespace RaceManager.Cars.Effects
{
    public class BrakeLight : MonoBehaviour
    {
        //public CarController car; // reference to the car controller, must be dragged in inspector

        private Renderer _renderer;
        private float _brakeInput;


        private void Start()
        {
            _renderer = GetComponent<Renderer>();
        }


        private void Update()
        {
            // enable the Renderer when the car is braking, disable it otherwise.
            //_renderer.enabled = car.BrakeInput > 0f;
            _renderer.enabled = _brakeInput > 0f;
        }

        public void SetBrakeInput(float value) => _brakeInput = value;
    }
}
