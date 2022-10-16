using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Alt
{
    public class CarMovementController : MonoBehaviour
    {
        public float maxSpeed = 20f;
        public float accelerationFactor = 30f;
        public float turnFactor = 3.5f;
        [Range(0f, 1f)]
        public float driftFactor = 0.95f;

        private const float MagnitudeDevider = 8f;
        private const float StoppingDragFactor = 3f;
        private const float SkidThreshold = 4f;
        private const float Downforce = 1f;

        private float _accelerationInput = 0f;
        private float _steeringInput = 0f;
        private float _rotationAngle = 90f;
        private float _velocityVsForward = 0f;

        private Rigidbody _carRb;

        private void Awake()
        {
            _carRb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            ApplyEngineForce();
            KillOrthogonalVelocity();
            ApplySteering();
            AddDownForce();
        }

        private void ApplyEngineForce()
        {
            //Calculates how much "forward" we are going in terms of the direction of our velocity
            _velocityVsForward = Vector3.Dot(transform.forward, _carRb.velocity);

            //Limit so we cannot go faster then the max speed in forward direction
            if (_velocityVsForward > maxSpeed && _accelerationInput > 0)
                return;

            //Limit so we cannot go faster then 50% of max speed in reverce direction
            if (_velocityVsForward < -maxSpeed * 0.5f && _accelerationInput < 0)
                return;

            //Limit so we cannot go faster in any direction while accelerating
            if (_carRb.velocity.sqrMagnitude > maxSpeed * maxSpeed && _accelerationInput > 0)
                return;

            _carRb.drag = _accelerationInput == 0
                ? Mathf.Lerp(_carRb.drag, StoppingDragFactor, Time.fixedDeltaTime * StoppingDragFactor)
                : 0f;

            Vector3 engineForceVector = transform.forward * _accelerationInput * accelerationFactor;
            _carRb.AddForce(engineForceVector, ForceMode.Force);
        }

        private void ApplySteering()
        {
            float minSpeedBeforeAllowTurnungFactor = _carRb.velocity.magnitude / MagnitudeDevider;
            minSpeedBeforeAllowTurnungFactor = Mathf.Clamp01(minSpeedBeforeAllowTurnungFactor);

            _rotationAngle += _steeringInput * turnFactor * minSpeedBeforeAllowTurnungFactor * Mathf.Sign(_accelerationInput);

            _carRb.MoveRotation(Quaternion.AngleAxis(_rotationAngle, Vector3.up));
        }

        private void KillOrthogonalVelocity()
        {
            Vector3 forwardVelocity = transform.forward * Vector3.Dot(_carRb.velocity, transform.forward);
            Vector3 rightVelocity = transform.right * Vector3.Dot(_carRb.velocity, transform.right);
            _carRb.velocity = forwardVelocity + rightVelocity * driftFactor;
        }

        // this is used to add more grip //in relation to speed
        private void AddDownForce() => _carRb.AddForce(-_carRb.transform.up * Downforce * _carRb.velocity.magnitude * 0.5f);
        private float GetLateralVelocity() => Vector3.Dot(transform.right, _carRb.velocity);
        public float GetVelocityMagnitude() => _carRb.velocity.magnitude;

        public bool IsTireScreeching(out float lateralVelocity, out bool isBraking)
        {
            lateralVelocity = GetLateralVelocity();
            isBraking = false;

            if (_accelerationInput <= 0 && _velocityVsForward >= 0)
            {
                isBraking = true;
                return true;
            }

            if (Mathf.Abs(lateralVelocity) > SkidThreshold)
                return true;

            return false;
        }


        public void SetInputVector(Vector3 inputVector)
        {
            _steeringInput = inputVector.x;
            _accelerationInput = inputVector.z;
        }
    }
}

