using UnityEngine;

[RequireComponent(typeof(CarDriverAI))]
public class CarDriverPlayer : MonoBehaviour
{
    private CarDriverAI _carDriverAI;

    private void Awake()
    {
        _carDriverAI = GetComponent<CarDriverAI>();
    }

    private void Start()
    {
        _carDriverAI.manualTurn = true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            SwitchTurningMode();
        if (Input.GetMouseButtonUp(0))
            SwitchTurningMode();
    }

    private void SwitchTurningMode()
    {
        _carDriverAI.manualTurn = !_carDriverAI.manualTurn;
    }
}
