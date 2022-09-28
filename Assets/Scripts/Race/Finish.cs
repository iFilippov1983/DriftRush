using RaceManager.Vehicles;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Race
{
    public class Finish : MonoBehaviour
    {
        private List<int> ids = new List<int>();

        private void OnTriggerEnter(Collider other)
        {
            CarAIControl carAI;
            carAI = other.GetComponentInParent<CarAIControl>();
            bool contains = false;
            if (carAI != null)
                contains = ids.Contains(carAI.gameObject.GetInstanceID());
            
            if (carAI && !contains)
            {
                ids.Add(carAI.gameObject.GetInstanceID());
                carAI.StopCar();
            }
        }

        private void OnDestroy()
        {
            ids.Clear();
        }
    }
}

