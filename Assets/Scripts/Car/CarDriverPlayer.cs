﻿using UnityEngine;

[RequireComponent(typeof(CarDriverAI))]
public class CarDriverPlayer : MonoBehaviour
{
    private CarDriverAI _carDriverAI;

    private void Awake()
    {
        _carDriverAI = GetComponent<CarDriverAI>();
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
        "Accelerating".Log(StringConsoleLog.Color.Green);
        _carDriverAI.Cruising = false;
    }

    private void Cruise()
    {
        "Cruising".Log(StringConsoleLog.Color.Yellow);
        _carDriverAI.Cruising = true;
    }
}
