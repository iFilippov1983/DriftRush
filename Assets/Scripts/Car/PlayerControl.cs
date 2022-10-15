using UnityEngine;

namespace RaceManager.Cars
{
    public class PlayerControl : MonoBehaviour
    {
<<<<<<< Updated upstream
        [SerializeField]
        private CarSettings _carSettings;
        private CarAI _carAI;

        private void Awake()
        {
=======
        private Car _car;
        private CarAI _carAI;

        bool _accelerating;

        private void Awake()
        {
            _car = GetComponent<Car>();
>>>>>>> Stashed changes
            _carAI = GetComponent<CarAI>();
            _carAI.PlayerDriving = true;
        }

<<<<<<< Updated upstream
        //public void Initialize(CarAIControl carAI, CarSettings carSettings)
        //{
        //    //_carController = carController;
        //    _carSettings = carSettings;
        //    _carAIControl = carAI;
        //    _carAIControl.PlayerDriving = true;
        //    _carAIControl.DesiredSpeed = _carSettings.CruiseRBVelocityMagnitude;
        //    _carAIControl.StopAvoiding();
=======
        //public void Initialize(CarAI carAI, CarConfig carConfig)
        //{
        //    _carConfig = carConfig;
        //    _carAI = carAI;
        //    _carAI.PlayerDriving = true;
        //    _carAI.DesiredSpeed = _carConfig.CruiseSpeed;
        //    _carAI.StopAvoiding();
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
            _carAI.DesiredSpeed = _carSettings.MaxRBVelocityMagnitude;
=======
            if(_accelerating)
                return;
            _carAI.DesiredSpeed = _car.CarConfig.MaxSpeed;
            _accelerating = true;
>>>>>>> Stashed changes
        }

        private void Cruise()
        {
<<<<<<< Updated upstream
            _carAI.DesiredSpeed = _carSettings.CruiseRBVelocityMagnitude;
=======
            if(!_accelerating)
                return;
            _carAI.DesiredSpeed = _car.CarConfig.CruiseSpeed;
            _accelerating = false;
>>>>>>> Stashed changes
        }
    }
}

