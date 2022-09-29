using UnityEngine;

namespace RaceManager.Vehicles
{
    public abstract class CarAI : MonoBehaviour
    {
        public abstract string GetID();
        public abstract void StartEngine();
        public abstract void StopEngine();
    }
}
