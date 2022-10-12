using RaceManager.Cars;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Cars.Effects
{
    public class WheelTrailRendererHandler : MonoBehaviour
    {
        [SerializeField] private WheelCollider _attachedWheelCollider;
        private CarController _carController;
        private TrailRenderer _trailRenderer;

        private void Awake()
        {
            _carController = GetComponentInParent<CarController>();
            _trailRenderer = GetComponent<TrailRenderer>();
            _trailRenderer.emitting = false;
        }

        private void Update()
        {
            WheelHit wheelhit;
            _attachedWheelCollider.GetGroundHit(out wheelhit);

            if (_carController.AreTiresScreeching(out float lateralVelocity, out bool isBraking) && wheelhit.normal != Vector3.zero)
                _trailRenderer.emitting = true;
            else
                _trailRenderer.emitting = false;

        }
    }
}
