using UnityEngine;
using RaceManager.Cars;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private CarSettings _carSettings;
    [SerializeField] private DriverSettings _driverSettings;
    private CarMovementController _carController;
    private Vector3 _inputVector;

    private void Awake()
    {
        _carController = GetComponent<CarMovementController>();
        _carController.maxSpeed = _driverSettings.CruiseSpeed;
    }

    private void Update()
    {
        //_inputVector.x = Input.GetAxis("Horizontal");
        //_inputVector.z = Input.GetAxis("Vertical");
        //_carController.SetInputVector(_inputVector);

        if (Input.GetMouseButtonDown(0))
            Accelerate();
        if (Input.GetMouseButtonUp(0))
            Cruise();
    }

    private void Accelerate()
    {
        _carController.maxSpeed = _carSettings.MaxSpeed;
    }

    private void Cruise()
    {
        _carController.maxSpeed = _driverSettings.CruiseSpeed;
    }
}
