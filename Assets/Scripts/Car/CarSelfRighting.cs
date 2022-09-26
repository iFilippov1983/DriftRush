using System;
using UnityEngine;

namespace RaceManager.Vehicles
{
    [RequireComponent(typeof(CarAIControl))]
    public class CarSelfRighting : MonoBehaviour, IObserver<Vector3>
    {
        // Automatically put the car the right way up, if it has come to rest upside-down of stuck.
        [SerializeField] private float _waitTime = 4f;            // time to wait before self righting
        [SerializeField] private float _velocityThreshold = 1f;   // the velocity below which the car is considered stationary for self-righting
        [SerializeField, Range(1, 10)] private float _checkPeriod = 5f;
        private float _stuckDistance = 0.01f;
        private float _checkTimer;
        private float _stuckTimer;
        private bool _isStuck;

        private CarAIControl _carAI;
        private Vector3? _lastOkPosition;
        //private float _lastOkTime;
        private Rigidbody _rigidbody;


        private void Start()
        {
            _carAI = GetComponent<CarAIControl>();
            _rigidbody = GetComponent<Rigidbody>();
            _checkTimer = _checkPeriod;
            _isStuck = false;
        }


        private void Update()
        {
            //_checkTimer -= Time.deltaTime;

            //if (_checkTimer < 0 && !_isStuck)
            //{
            //    _lastOkPosition = _rigidbody.transform;
            //    _checkTimer = _checkPeriod;
            //}

            HandleSafety();

            //if (_rigidbody.velocity.magnitude < _velocityThreshold)
            //{
            //    _isStuck = true;
            //    $"Stuck: {gameObject.name}".Log(StringConsoleLog.Color.Red);
            //} 
            //else
            //    _isStuck = false;

            //if (transform.up.y > 0f || _rigidbody.velocity.magnitude > _velocityThreshold)
            //{
            //    _lastOkTime = Time.time;
            //}

            //if (Time.time > _lastOkTime + _waitTime)
            //{
            //    RightCar();
            //}
        }


        private void HandleSafety()
        {
            if (_carAI.isDriving && _lastOkPosition != null)
            {
                //if (Vector3.Distance(transform.position, _lastOkPosition.position) < _stuckDistance)
                if (_rigidbody.velocity.magnitude < _velocityThreshold)
                {
                    _isStuck = true;
                    _stuckTimer += Time.deltaTime;
                    if (_stuckTimer > _waitTime)
                    {
                        RightCar();
                    }
                }
                else
                {
                    _isStuck = false;
                    _stuckTimer = 0;
                }
            }
        }

        private void RightCar()
        {
            // set the correct orientation for the car, and lift it off the ground a little
            if (_lastOkPosition != null)
            {
                transform.position = _lastOkPosition.Value;
                $"Returned to position {_lastOkPosition}".Log(StringConsoleLog.Color.Green);
            }

            transform.position += Vector3.up;
            transform.rotation = Quaternion.LookRotation(_lastOkPosition.Value);
            _isStuck = false;
            _stuckTimer = 0;
        }

        public void OnCompleted() => throw new NotImplementedException();


        public void OnError(Exception error) => throw error;

        public void OnNext(Vector3 vector)
        {
            _lastOkPosition = vector;
        }
    }
}
