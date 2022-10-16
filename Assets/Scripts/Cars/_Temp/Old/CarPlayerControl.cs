using UnityEngine;

namespace RaceManager.Alt
{
    public class CarPlayerControl : MonoBehaviour
    {
        [SerializeField]
        private CarSettings _carConfig;
        private CarAIControl _carAI;
        //private CarController _carController;

        //private void Awake()
        //{
        //    _carAIControl = GetComponent<CarAIControl>();
        //    _carAIControl.PlayerDriving = true;
        //}

        public void Initialize(CarAIControl carAI, CarSettings carSettings)
        {
            //_carController = carController;
            _carConfig = carSettings;
            _carAI = carAI;
            _carAI.PlayerDriving = true;
            _carAI.DesiredSpeed = _carConfig.CruiseRBVelocityMagnitude;
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

