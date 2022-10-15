using RaceManager.Tools;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

namespace RaceManager.Cars
{
    public class CarSelfRighting : MonoBehaviour
    {
        private const float GroundingForce = 50000f;

        // Automatically put the car the right way up, if it has come to rest upside-down or stuck.
        [SerializeField] private float _waitTimeStuck = 1f;              // time to wait before self righting
        [SerializeField] private float _velocityThreshold = 0.7f;   // the velocity below which the car is considered stationary for self-righting
        private float _stuckTimer;

        private CarAIControl _carAI;
        private Rigidbody _rigidbody;
        private WheelCollider[] _wheelColliders;
        [ReadOnly]
        public Transform LastOkPoint;

        //public void Initialize(CarAIControl carAI, Rigidbody carRigidbody)
        //{ 
        //    _carAI = carAI;
        //    _rigidbody = carRigidbody;
        //}

        private void Start()
        {
            _carAI = GetComponent<CarAIControl>();
            _rigidbody = GetComponent<Rigidbody>();

            _wheels = GetComponent<Car>().Wheels;
        }

        internal void Setup(WheelCollider[] wheelColliders)
        {
            _wheelColliders = wheelColliders;
        }

        private void Update()
        {
            HandleSafety();
        }

        private void FixedUpdate()
        {
            GroundingControl();
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
            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                if (_wheelColliders[i].isGrounded == false)
                {
                    _wheelColliders[i].attachedRigidbody.AddForceAtPosition(Vector3.down * GroundingForce, _wheelColliders[i].transform.position, ForceMode.Force);
                    //Debug.Log($"Force added to: {_wheelColliders[i].name}");
                }
            }
        }

        private bool CarIsFlipedOrFlying()
        {
            bool allWheelsOnGround = false;
            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                if (_wheelColliders[i].isGrounded) allWheelsOnGround = true;
            }

            return allWheelsOnGround;
        }
    }
}
