using RaceManager.Cars;
using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Sirenix.OdinInspector;

namespace RaceManager.Waypoints
{
    [RequireComponent(typeof(BoxCollider))]
    public class Waypoint : MonoBehaviour, IObservable<string>, IObserver<string>
    {
        public int Number;
        public bool isFinishLine = false;
        public bool isBrakeCheckpointA = false;
        public bool isBrakeCheckpointB = false;
        public bool isDriftCheckpointA = false;
        public bool isDriftCheckpointB = false;
        public bool isRaceLinePoint = false;
        [ReadOnly]
        public Waypoint NextWaypoint;

        public float RecomendedSpeed { get; set; }

        private const int NumberOfCarsOnTrack = 6;

        [SerializeField]
        private RespawnPoint[] _respawnPoints = new RespawnPoint[NumberOfCarsOnTrack];
        private List<string> _carIDs = new List<string>();
        private List<IObserver<string>> _observers = new List<IObserver<string>>();

        public Action<Waypoint> OnPassed;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Car car))
            {
                SetRespawnPosition(car);
                SetAndNotify(car, other);
            }
        }

        private void SetRespawnPosition(Car car)
        {
            string id = car.ID;
            //Made in purpose to avoid multiple triggering
            bool contains = _carIDs.Contains(id);
            
            if (!contains)
            {
                _carIDs.Add(id);
                NotifyObservers(id);

                for (int i = 0; i < _respawnPoints.Length; i++)
                {
                    if (_respawnPoints[i].IsOccupied) continue;
                    _respawnPoints[i].AttachCar(car);
                    car.CarSelfRighting.LastOkPoint = _respawnPoints[i].transform;
                    return;
                }
            }
        }

        private void SetAndNotify(Car car, Collider other)
        {
            if (isRaceLinePoint)
            {
                car.CarSelfRighting.LastCheckpoint = transform;
            }

            bool hasFeature = isBrakeCheckpointA || isBrakeCheckpointB || isDriftCheckpointA || isDriftCheckpointB || isRaceLinePoint;

            if (hasFeature)
            {
                bool isPlayer = other.TryGetComponent(out PlayerControl playerControl);

                if (isPlayer)
                    OnPassed?.Invoke(this);
            }
        }

        private void OnDestroy()
        {
            _carIDs.Clear();
            _observers.Clear();
        }

        private void NotifyObservers(string id)
        { 
            foreach (var observer in _observers)
                observer.OnNext(id);
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            _observers.Add(observer);
            return Disposable.Empty;
        }

        public void OnNext(string id)
        {
            for (int i = 0; i < _respawnPoints.Length; i++)
            {
                if (_respawnPoints[i].CarId.Equals(id))
                {
                    _respawnPoints[i].DetachCar();
                    _carIDs.Remove(id);
                    break;
                } 
            }
        }

        public void OnCompleted() => throw new NotImplementedException();
        public void OnError(Exception error) => throw error;
    }
}
