using UnityEngine;

namespace RaceManager.Cars
{
    public class CarPlayerControl : MonoBehaviour
    {
        [SerializeField]
        private CarSettings _carSettings;
        private CarAIControl _carAIControl;
        //private CarController _carController;

        //private void Awake()
        //{
        //    _carAIControl = GetComponent<CarAIControl>();
        //    _carAIControl.PlayerDriving = true;
        //}

        public void Initialize(CarAIControl carAI, CarSettings carSettings)
        {
            //_carController = carController;
            _carSettings = carSettings;
            _carAIControl = carAI;
            _carAIControl.PlayerDriving = true;
            _carAIControl.DesiredSpeed = _carSettings.CruiseRBVelocityMagnitude;
            _carAIControl.StopAvoiding();
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
            _carAIControl.DesiredSpeed = _carSettings.MaxRBVelocityMagnitude;
        }

        private void Cruise()
        {
            _carAIControl.DesiredSpeed = _carSettings.CruiseRBVelocityMagnitude;
        }
    }
}

