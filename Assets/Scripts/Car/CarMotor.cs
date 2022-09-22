using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMotor : MonoBehaviour
{
    private CarDriverAI _carDriverAI;

    public CarDriverAI CarDriverAI => _carDriverAI;

    private void Awake()
    {
        _carDriverAI = GetComponentInParent<CarDriverAI>();
    }
}
