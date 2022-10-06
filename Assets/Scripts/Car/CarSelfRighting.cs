using Sirenix.OdinInspector;
using UnityEngine;

namespace RaceManager.Cars
{
    public class CarSelfRighting : MonoBehaviour
    {
        // Automatically put the car the right way up, if it has come to rest upside-down or stuck.
        [SerializeField] private float _waitTime = 2f;              // time to wait before self righting
        [SerializeField] private float _velocityThreshold = 0.7f;   // the velocity below which the car is considered stationary for self-righting
        private float _stuckTimer;

        private CarAIControl _carAI;
        private Rigidbody _rigidbody;
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
                _carAI.StopDriving();
                transform.position = LastOkPoint.position;
                //transform.position += Vector3.up / 4f;
                transform.rotation = LastOkPoint.rotation;
                $"Returned to position {LastOkPoint.position}".Log(StringConsoleLog.Color.Green);
                _carAI.StartEngine();
            }
            _stuckTimer = 0;
        }
    }
}
