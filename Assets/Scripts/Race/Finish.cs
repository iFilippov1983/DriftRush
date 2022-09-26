using RaceManager.Vehicles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Race
{
    public class Finish : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            CarAIControl carAI;
            if (carAI = other.GetComponentInParent<CarAIControl>())
            {
                carAI.StopCar();
            }
        }
    }
}

