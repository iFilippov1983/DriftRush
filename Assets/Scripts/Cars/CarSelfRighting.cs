using RaceManager.Tools;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace RaceManager.Cars
{
    /// <summary>
    /// Automatically puts the car the right way up, if it has come to rest upside-down or stuck. (Can be also used in debug purposes)
    /// </summary>
    public class CarSelfRighting : MonoBehaviour
    {
        private const float GroundingForce = 50000f;

        [SerializeField] private float _waitTimeStuck = 1f;             // time to wait before self righting
        [SerializeField] private float _velocityThreshold = 0.5f;       // the velocity below which the car is considered stationary for self-righting
        private float _stuckTimer;

        private CarAI _carAI;
        private Rigidbody _rigidbody;
        private Wheel[] _wheels;

        [ReadOnly]
        public Transform LastOkPoint;
        [ReadOnly]
        public Transform LastCheckpoint;

        public Action OnCarRespawn; 

        public void RightCar() => RespawnCar(LastOkPoint);
        public void GetToCheckpoint() => RespawnCar(LastCheckpoint, true);

        private void OnEnable()
        {
            _carAI = GetComponent<CarAI>();
            _rigidbody = GetComponent<Rigidbody>();

            _wheels = GetComponent<Car>().Wheels;
        }

        private void Update()
        {
            HandleSafety();
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

        private void RespawnCar(Transform positionToRespawnOn, bool resetVelocity = false)
        {
            if (positionToRespawnOn != null)
            {
                _carAI.StopDriving();
                if(resetVelocity)
                    _rigidbody.velocity = Vector3.zero;
                transform.position = positionToRespawnOn.position;
                transform.rotation = positionToRespawnOn.rotation;
                _carAI.StartEngine();
            }
            _stuckTimer = 0;

            OnCarRespawn?.Invoke();
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
