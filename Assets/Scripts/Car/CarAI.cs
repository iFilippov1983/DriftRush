using UnityEngine;

namespace RaceManager.Cars
{
    public abstract class CarAI : MonoBehaviour
    {
        public abstract string GetID();
        public abstract void StartEngine();
        public abstract void StopEngine();
    }
}
