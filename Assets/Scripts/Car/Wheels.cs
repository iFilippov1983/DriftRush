using System;
using UnityEngine;

public class Wheels : MonoBehaviour, IObserver<CarDriver.MoveValues>
{
    [SerializeField] private WheelCollider _wheelFL;
    [SerializeField] private WheelCollider _wheelFR;
    [SerializeField] private WheelCollider _wheelBL;
    [SerializeField] private WheelCollider _wheelBR;

    private WheelFrictionCurve _curve;

    private void Awake()
    {
        
    }

    private void RotateWheels()
    { 
    
    }

    private void TurnWheels()
    { 
    
    }

    public void OnNext(CarDriver.MoveValues value)
    {
        throw new NotImplementedException();
    }

    public void OnCompleted() => throw new NotImplementedException();
    public void OnError(Exception error) => throw error;
}
