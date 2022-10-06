using RaceManager.Cars;
using UnityEngine;

namespace RaceManager.Waypoints
{
    public class RespawnPoint : MonoBehaviour
    {
        private Car _car;

        public void AttachCar(Car car) => _car = car;
        public void DetachCar() => _car = null;
        public bool IsOccupied => _car != null;
        public string CarId
        {
            get
            {
                if (IsOccupied) return _car.ID;
                else return string.Empty;    
            }
        }
    }
}
