using UnityEngine;

namespace RaceManager.Cars
{
    public class CarPlayerControl : MonoBehaviour
    {
        private CarAIControl _carAIControl;
        private CarController _carController;

        //private void Awake()
        //{
        //    _carController = GetComponent<CarController>();
        //    _carAIControl = GetComponent<CarAIControl>();
        //    _carAIControl.PlayerDriving = true;
        //}

        public void Initialize(CarController carController, CarAIControl carAI)
        {
            _carController = carController;
            _carAIControl = carAI;
            _carAIControl.PlayerDriving = true;
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
            _carAIControl.DesiredSpeed = _carController.MaxSpeed;
        }

        private void Cruise()
        {
            _carAIControl.DesiredSpeed = _carAIControl.CruiseSpeed;
        }
    }
}

