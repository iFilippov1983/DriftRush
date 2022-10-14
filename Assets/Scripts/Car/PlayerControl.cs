using UnityEngine;

namespace RaceManager.Cars
{
    public class PlayerControl : MonoBehaviour
    {
        [SerializeField]
        private CarSettings _carSettings;
        private CarAI _carAI;

        private void Awake()
        {
            _carAI = GetComponent<CarAI>();
            _carAI.PlayerDriving = true;
        }

        //public void Initialize(CarAIControl carAI, CarSettings carSettings)
        //{
        //    //_carController = carController;
        //    _carSettings = carSettings;
        //    _carAIControl = carAI;
        //    _carAIControl.PlayerDriving = true;
        //    _carAIControl.DesiredSpeed = _carSettings.CruiseRBVelocityMagnitude;
        //    _carAIControl.StopAvoiding();
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
            _carAI.DesiredSpeed = _carSettings.MaxRBVelocityMagnitude;
        }

        private void Cruise()
        {
            _carAI.DesiredSpeed = _carSettings.CruiseRBVelocityMagnitude;
        }
    }
}

