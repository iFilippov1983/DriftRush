using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRollBar : MonoBehaviour
{
    [Title("Attach left and right wheels from one axle")]
    [SerializeField] private WheelCollider _wheelL;
    [SerializeField] private WheelCollider _wheelR;
    [SerializeField] private Rigidbody _carRb;
    [SerializeField] private float _antiroll = 5000f;

    private void FixedUpdate()
    {
        WheelHit hit;
        float travelL = 1f;
        float travelR = 1f;

        bool groundedL = _wheelL.GetGroundHit(out hit);
        if (groundedL)
        {
            travelL = (-_wheelL.transform.InverseTransformPoint(hit.point).y - _wheelL.radius) / _wheelL.suspensionDistance;
        }

        bool groundedR = _wheelR.GetGroundHit(out hit);
        if (groundedR)
        { 
            travelR = (-_wheelR.transform.InverseTransformPoint(hit.point).y - _wheelR.radius) / _wheelR.suspensionDistance;
        }

        float antiRollForce = (travelL + travelR) * _antiroll;

        if (groundedL)
            _carRb.AddForceAtPosition(_wheelL.transform.up * -antiRollForce, _wheelL.transform.position);

        if (groundedR)
            _carRb.AddForceAtPosition(_wheelR.transform.up * antiRollForce, _wheelR.transform.position);
    }
}
