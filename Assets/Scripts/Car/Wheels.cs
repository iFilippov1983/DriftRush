using System;
using System.Collections.Generic;
using UnityEngine;

public class Wheels : MonoBehaviour, IObserver<CarEngine.MoveValues>
{
    [SerializeField] private GameObject _wheelFL;
    [SerializeField] private GameObject _wheelFR;
    [SerializeField] private GameObject _wheelBL;
    [SerializeField] private GameObject _wheelBR;

    private List<GameObject> _wheels;
    private Vector3 _steering;
    private float _turnSpeed;
    private float _rotation;

    private void Awake()
    {
        _wheels = new List<GameObject>();
        _wheels.Add(_wheelFL);
        _wheels.Add(_wheelFR);
        _wheels.Add(_wheelBL);
        _wheels.Add(_wheelBR);
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        RotateWheels();
        TurnWheels();
    }

    private void RotateWheels()
    {
        foreach (var w in _wheels)
        {
            w.transform.RotateAround(w.transform.position, w.transform.right, _rotation);
        }
    }

    private void TurnWheels()
    {
        _wheelFL.transform.localRotation.Set(0f, _steering.y * _turnSpeed, 0f, 1f);
        _wheelFR.transform.localRotation.Set(0f, _steering.y * _turnSpeed, 0f, 1f);

        //_wheelFL.transform.Rotate(0f, _steering.y * _turnSpeed, 0f, Space.Self);
        //_wheelFR.transform.Rotate(0f, _steering.y * _turnSpeed, 0f, Space.Self);

        //Vector3 rot = _wheelFL.transform.localRotation.eulerAngles;
        //rot.y += _steering.y * _turnSpeed;
        //Quaternion quaternion = new Quaternion(0f, rot.y, 0f, 0f);
        //_wheelFL.transform.localRotation = quaternion;
        //_wheelFR.transform.localRotation = quaternion;
    }

    public void OnNext(CarEngine.MoveValues values)
    {
        _steering = values.MotorAngularVelosity;
        _turnSpeed = values.TurnSpeed;

        _rotation = values.Speed;

        $"Rotation: {_rotation}".Log(Color.red);
        $"V: {_steering.y * _turnSpeed}".Log(Color.red);
        if (_rotation > 360f || _rotation < 0 || values.Speed == 0)
            _rotation = 0;
    }

    public void OnCompleted() => throw new NotImplementedException();
    public void OnError(Exception error) => throw error;
}
