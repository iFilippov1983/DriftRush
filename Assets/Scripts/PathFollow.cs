using Navigation;
using System;
using UnityEngine;

public class PathFollow : MonoBehaviour
{
    [SerializeField] private Path _path;
    [SerializeField] private int _firstTargetIndex = 0;
    private ICarDriver _carDriver;
    private Waypoint _currentWaypoint;

    private void Awake()
    {
        //_carDriver = GetComponent<CarDriverAI>();
    }

    private void Start()
    {
        _currentWaypoint = _path.GetNearestWaypoint(_firstTargetIndex);
        _carDriver.TargetReachedEvent.AddListener(SetTarget);
        _carDriver.SetTargetPosition(_currentWaypoint.Position);
    }

    private void OnDestroy()
    {
        _carDriver.TargetReachedEvent.RemoveAllListeners();
    }

    private void SetTarget()
    {
        var wp = _path.GetNearestWaypoint(_firstTargetIndex);
        if (wp != null)
        {
            _currentWaypoint = wp;
            _carDriver.SetTargetPosition(_currentWaypoint.Position);
        }
    }
}
