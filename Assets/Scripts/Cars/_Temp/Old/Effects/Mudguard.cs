using System;
using UnityEngine;

namespace RaceManager.Alt
{
    // this script is specific to the supplied Sample Assets car, which has mudguards over the front wheels
    // which have to turn with the wheels when steering is applied.

    public class Mudguard : MonoBehaviour
    {
        //public CarController carController; // car controller to get the steering angle

        private Quaternion _originalRotation;
        private float _currentSteerAngle;

        private void Start()
        {
            _originalRotation = transform.localRotation;
        }


        private void Update()
        {
            //transform.localRotation = _originalRotation*Quaternion.Euler(0, carController.CurrentSteerAngle, 0);
            transform.localRotation = _originalRotation * Quaternion.Euler(0, _currentSteerAngle, 0);
        }

        public void UpdateSteerAngle(float value) => _currentSteerAngle = value;
    }
}
