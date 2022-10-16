using RaceManager.Tools;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using PG_Physics.Wheel;

namespace RaceManager.Cars
{
    public class CarSelfRighting : MonoBehaviour
    {
        private const float GroundingForce = 50000f;

        // Automatically put the car the right way up, if it has come to rest upside-down or stuck.
        [SerializeField] private float _waitTimeStuck = 1f;              // time to wait before self righting
        [SerializeField] private float _velocityThreshold = 0.5f;   // the velocity below which the car is considered stationary for self-righting
        private float _stuckTimer;

        private CarAI _carAI;
        private Rigidbody _rigidbody;
        private Wheel[] _wheels;
        [ReadOnly]
        public Transform LastOkPoint;

        //public void Initialize(CarAIControl carAI, Rigidbody carRigidbody)
        //{ 
        //    _carAI = carAI;
        //    _rigidbody = carRigidbody;
        //}

        private void OnEnable()
        {
            _carAI = GetComponent<CarAI>();
            _rigidbody = GetComponent<Rigidbody>();

            _wheels = GetComponent<Car>().Wheels;
        }

        internal void Setup(Wheel[] wheels)
        {
            _wheels = wheels;
        }

        private void Update()
        {
            HandleSafety();
        }

        private void FixedUpdate()
        {
            //GroundingControl();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer(Layer.OffTrack))
            {
                RightCar();
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private void HandleSafety()
        {
            if (_carAI.isDriving && LastOkPoint != null)
            {
                if (_rigidbody.velocity.magnitude < _velocityThreshold)
                {
                    _stuckTimer += Time.deltaTime;
                    if (_stuckTimer > _waitTimeStuck)
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

        public void RightCar()
        {
            if (LastOkPoint != null)
            {
                _carAI.StopDriving();
                transform.position = LastOkPoint.position;
                transform.rotation = LastOkPoint.rotation;
                //$"Returned to position {LastOkPoint.position}".Log(StringConsoleLog.Color.Green);
                _carAI.StartEngine();
            }
            _stuckTimer = 0;
        }


        private void GroundingControl()
        {
            for (int i = 0; i < _wheels.Length; i++)
            {
                if (_wheels[i].WheelCollider.isGrounded == false)
                {
                    _wheels[i].WheelCollider.attachedRigidbody.AddForceAtPosition(Vector3.down * GroundingForce, _wheels[i].WheelCollider.transform.position, ForceMode.Force);
                    //Debug.Log($"Force added to: {_wheelColliders[i].name}");
                }
            }
        }

        private bool CarIsFlipedOrFlying()
        {
            bool allWheelsOnGround = false;
            for (int i = 0; i < _wheels.Length; i++)
            {
                if (_wheels[i].WheelCollider.isGrounded) allWheelsOnGround = true;
            }

            return allWheelsOnGround;
        }
    }
}
