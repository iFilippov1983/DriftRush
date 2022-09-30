using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RaceManager.Vehicles
{
    [RequireComponent(typeof(CarAIControl))]
    public class CarSelfRighting : MonoBehaviour
    {
        // Automatically put the car the right way up, if it has come to rest upside-down of stuck.
        [SerializeField] private float _waitTime = 2f;            // time to wait before self righting
        [SerializeField] private float _velocityThreshold = 1f;   // the velocity below which the car is considered stationary for self-righting
        private float _stuckTimer;

        private CarAIControl _carAI;
        private Rigidbody _rigidbody;
        [ReadOnly]
        public Transform LastOkPoint;

        private void Start()
        {
            _carAI = GetComponent<CarAIControl>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            HandleSafety();
        }

        private void HandleSafety()
        {
            if (_carAI.isDriving && LastOkPoint != null)
            {
                if (_rigidbody.velocity.magnitude < _velocityThreshold)
                {
                    _stuckTimer += Time.deltaTime;
                    if (_stuckTimer > _waitTime)
                    {
                        RightCar();
                    }
                }
                else
                {
                    _stuckTimer = 0;
                }
            }
        }

        private void RightCar()
        {
            if (LastOkPoint != null)
            {
                transform.position = LastOkPoint.position;
                transform.rotation = LastOkPoint.rotation;
                $"Returned to position {LastOkPoint.position}".Log(StringConsoleLog.Color.Green);
            }

            transform.position += Vector3.back;
            transform.position += Vector3.up;
            
            _stuckTimer = 0;
        }
    }
}
