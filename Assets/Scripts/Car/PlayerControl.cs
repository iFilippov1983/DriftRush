using UnityEngine;

namespace RaceManager.Cars
{
    public class PlayerControl : MonoBehaviour
    {
        [SerializeField]
        private CarConfig _carConfig;
        private CarAI _carAI;

        //private void Awake()
        //{
        //    _carAI = GetComponent<CarAI>();
        //    _carAI.PlayerDriving = true;
        //}

        public void Initialize(CarAI carAI, CarConfig carConfig)
        {
            _carConfig = carConfig;
            _carAI = carAI;
            _carAI.PlayerDriving = true;
            _carAI.DesiredSpeed = _carConfig.CruiseSpeed;
            _carAI.StopAvoiding();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                Accelerate();
            if (Input.GetMouseButtonUp(0))
                Cruise();
        }

        private void Accelerate()
        {
            _carAI.DesiredSpeed = _carConfig.MaxRBVelocityMagnitude;
        }

        private void Cruise()
        {
            _carAI.DesiredSpeed = _carConfig.CruiseRBVelocityMagnitude;
        }
    }
}

