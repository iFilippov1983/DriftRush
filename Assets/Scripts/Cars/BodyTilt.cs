using Sirenix.OdinInspector;
using UnityEngine;

namespace RaceManager.Cars
{
	/// <summary>
	/// To tilt the car body (Visually only).
	/// </summary>

	[RequireComponent(typeof(Car))]
	public class BodyTilt : MonoBehaviour
	{
		[Title("Tilt")]
		[SerializeField] private bool _useTilt = true;

        [Tooltip("Link to car body.")]
		[SerializeField] private Transform _body;

        [Tooltip("Max tilt angle of car body.")]
		[SerializeField] private float _maxAngle = 10;     
        
        [Tooltip("Rotation angle multiplier when moving forward.")]
		[SerializeField] private float _angleVelocityMultiplayer = 0.2f;    
        
        [Tooltip("Rotation angle multiplier when moving backwards.")]
		[SerializeField] private float _rearAngleVelocityMultiplayer = 0.4f;   
        
        [Tooltip("The speed at which the maximum tilt is reached.")]
		[SerializeField] private float _maxTiltOnSpeed = 60;      
        
		[Space]
		[Title("AntiRoll")]
		[SerializeField] private bool _useAntiRoll = false;
        [SerializeField] private WheelCollider _wheelL;
        [SerializeField] private WheelCollider _wheelR;
        [SerializeField] private float _antiRoll = 5000.0f;

        private Car _car;
        private Rigidbody _carRb;
        private float _angle;

        void Start()
        {
            _carRb = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
			_car = GetComponent<Car>();
		}

        private void Update()
		{
            if (_useTilt)
                TiltBody();
		}

        void FixedUpdate()
        {
            if(_useAntiRoll)
                HandleAntiRoll();
        }

        private void TiltBody()
		{
            if (_car.CarDirection == 1)
                _angle = -_car.VelocityAngle * _angleVelocityMultiplayer;
            else if (_car.CarDirection == -1)
            {
                _angle = MathExtentions.Repeat(_car.VelocityAngle + 180, -180, 180) * _rearAngleVelocityMultiplayer;
            }
            else
            {
                _angle = 0;
            }

            _angle *= Mathf.Clamp01(_car.SpeedInDesiredUnits / _maxTiltOnSpeed);
            _angle = Mathf.Clamp(_angle, -_maxAngle, _maxAngle);
            _body.localRotation = Quaternion.AngleAxis(_angle, Vector3.forward);
        }

        private void HandleAntiRoll()
        {
            WheelHit hit;
            float travelL = 1.0f;
            float travelR = 1.0f;


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

            float antiRollForce = (travelL - travelR) * _antiRoll;

            if (groundedL)
                _carRb.AddForceAtPosition(_wheelL.transform.up * -antiRollForce, _wheelL.transform.position);

            if (groundedR)
                _carRb.AddForceAtPosition(_wheelR.transform.up * antiRollForce, _wheelR.transform.position);
        }
    }
}
