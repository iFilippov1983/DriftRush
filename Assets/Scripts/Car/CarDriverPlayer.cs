﻿using UnityEngine;

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
        Cruise();
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
        
    }

    private void Cruise()
    { 
    
    }
}