using RaceManager.Vehicles;
using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace RaceManager.Waypoints
{
    [RequireComponent(typeof(BoxCollider))]
    public class Waypoint : MonoBehaviour, IObservable<int>, IObserver<int>
    {
        public int ID;

        //private List<int> _gameObjectIds = new List<int>();
        private Dictionary<int, CarSelfRighting> _selfRightings = new Dictionary<int, CarSelfRighting>();
        private List<IObserver<int>> _observers = new List<IObserver<int>>();

        private void OnTriggerEnter(Collider other)
        {
            CarSelfRighting csr;
            csr = other.GetComponentInParent<CarSelfRighting>();
            int id = csr.gameObject.GetInstanceID();
            bool contains = false;
            if (csr != null)
                contains = _selfRightings.ContainsKey(id);

            if (csr && !contains)
            {
                _selfRightings.Add(id, csr);
                NotifyObservers(id);

                if (_selfRightings.Count <= 1)
                {
                    csr.LastOkPoint = transform;
                    Debug.Log($"<color=blue>{gameObject.name} set new respawn point {transform.position} to {csr.gameObject.name}</color>");
                    return;
                }

                SetPointsWithOffset(other);
            }
        }

        private void SetPointsWithOffset(Collider collider)
        {
            Vector3 initialPos = this.transform.position;
            Transform transform = this.transform;

            float singleOffsetValue = collider.bounds.size.x;
            Vector3 newPos = transform.position;
            newPos.x -= singleOffsetValue / 2 * (_selfRightings.Count - 1);
            foreach (var pair in _selfRightings)
            {
                transform.position = newPos;
                pair.Value.LastOkPoint = transform;
                Debug.Log($"<color=yellow>{gameObject.name} set new respawn point {transform.position} to {pair.Value.gameObject.name}</color>");
                newPos.x += singleOffsetValue;
            }

            transform.position = initialPos;
        }

        private void OnDestroy()
        {
            _selfRightings.Clear();
        }

        private void NotifyObservers(int idValue)
        { 
            foreach (var observer in _observers)
                observer.OnNext(idValue);
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            _observers.Add(observer);
            return Disposable.Empty;
        }

        public void OnNext(int value)
        {
            if (_selfRightings.ContainsKey(value))
            { 
                _selfRightings.Remove(value);
            }
        }

        public void OnCompleted() => throw new NotImplementedException();
        public void OnError(Exception error) => throw error;
    }
}
