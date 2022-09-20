using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointsTrack : MonoBehaviour
{
    [SerializeField] private Transform _checkpointsTransform;
    [SerializeField] private List<Transform> _carsTransformList;

    private List<CheckpointSingle> _checkpointSingleList;
    private List<int> _nextCheckpointIndexList;

    [SerializeField] private bool _loopTrack;

    

    private void Awake()
    {
        _checkpointSingleList = new List<CheckpointSingle>();
        foreach (Transform t in _checkpointsTransform)
        {
            var cps = t.GetComponent<CheckpointSingle>();
            cps.SetCheckpoinsTrack(this);
            _checkpointSingleList.Add(cps);
        }

        _nextCheckpointIndexList = new List<int>();
        foreach (Transform t in _carsTransformList)
            _nextCheckpointIndexList.Add(0);
    }

    public void CarThoughCheckpoint(CheckpointSingle checkpointSingle, Transform carTransform, Action finishCallback = null)
    { 
        int carIndex = _carsTransformList.IndexOf(carTransform);
        int nextCheckpointSingleIndex = _nextCheckpointIndexList[carIndex];

        if (_checkpointSingleList.IndexOf(checkpointSingle) == nextCheckpointSingleIndex)
        {
            if (_loopTrack)
            {
                _nextCheckpointIndexList[carIndex] = (nextCheckpointSingleIndex + 1) % _checkpointSingleList.Count;
            }
            else
            {
                nextCheckpointSingleIndex++;
                if (nextCheckpointSingleIndex >= _checkpointSingleList.Count)
                {
                    finishCallback?.Invoke();
                    return;
                }
                    

                _nextCheckpointIndexList[carIndex] = nextCheckpointSingleIndex;
            }
        }
    }

    public Transform GetNextTargetFor(Transform carTransform)
    {
        int carIndex = _carsTransformList.IndexOf(carTransform);
        int nextCheckpointSingleIndex = _nextCheckpointIndexList[carIndex];
        return _checkpointSingleList[nextCheckpointSingleIndex].transform;
    }
}
