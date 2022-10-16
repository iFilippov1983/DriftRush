using UnityEngine;

namespace RaceManager.Cars
{
    public class PlayerControl : MonoBehaviour
    {
        private Car _car;
        private CarAI _carAI;

        private bool _accelerating;

        private void Awake()
        {
            _car = GetComponent<Car>();
            _carAI = GetComponent<CarAI>();
            _carAI.PlayerDriving = true;
        }

        //public void Initialize(CarAI carAI, CarConfig carConfig)
        //{
        //    _carAI = carAI;
        //    _carAI.PlayerDriving = true;
        //    _carAI.DesiredSpeed = _carConfig.CruiseSpeed;
        //    _carAI.StopAvoiding();
        //}

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                Accelerate();
            if (Input.GetMouseButtonUp(0))
                Cruise();
        }

        private void Accelerate()
        {
            if (_accelerating)
                return;
            _carAI.DesiredSpeed = _car.CarConfig.MaxSpeed;
            _accelerating = true;
        }

        private void Cruise()
        {
            if (!_accelerating)
                return;
            _carAI.DesiredSpeed = _car.CarConfig.CruiseSpeed;
            _accelerating = false;
        }
    }
}

