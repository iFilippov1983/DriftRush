using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CheckpointsTrack : MonoBehaviour
{
    [SerializeField] private Transform _checkpointsTransform;
    [SerializeField] private Transform _sideFinishPointsTransform;
    [SerializeField] private List<Transform> _carsTransformList;

    private List<CheckpointSingle> _checkpointSingleList;
    private List<int> _nextCheckpointIndexList;
    private List<Transform> _sidePointsList;

    [SerializeField] private bool _loopTrack;

    private void Awake()
    {
        _checkpointSingleList = SetCheckpointSingleLists();
        _nextCheckpointIndexList = SetCheckpointIndexList();
        _sidePointsList = SetSidePointsList();
    }

    public bool CarThoughCheckpoint(CheckpointSingle checkpointSingle, Transform carTransform, Action<Transform> finishCallback = null)
    { 
        bool notFinished = true;
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
                    int index = Random.Range(0, _sidePointsList.Count);
                    Transform sidePoint = _sidePointsList[index];
                    finishCallback?.Invoke(sidePoint);
                    return notFinished = false;
                }

                _nextCheckpointIndexList[carIndex] = nextCheckpointSingleIndex;
            }
        }

        return notFinished;
    }

    public Transform GetNextTargetFor(Transform carTransform)
    {
        int carIndex = _carsTransformList.IndexOf(carTransform);
        int nextCheckpointSingleIndex = _nextCheckpointIndexList[carIndex];
        var checkpoint = _checkpointSingleList[nextCheckpointSingleIndex];
        return checkpoint.transform;
    }

    private List<CheckpointSingle> SetCheckpointSingleLists()
    {
        var list = new List<CheckpointSingle>();

        foreach (Transform t in _checkpointsTransform)
        {
            var cps = t.GetComponent<CheckpointSingle>();
            cps.SetCheckpoinsTrack(this);
            list.Add(cps);
        }

        return list;
    }

    private List<int> SetCheckpointIndexList()
    { 
        var list = new List<int>();
        foreach (Transform t in _carsTransformList)
            list.Add(0);
        return list;
    }

    private List<Transform> SetSidePointsList()
    {
        var list = new List<Transform>();
        foreach (Transform t in _sideFinishPointsTransform)
            list.Add(t);
        return list;
    }

    [Button]
    private void DrawCheckpointMeshes()
    {
        foreach (Transform tr in _checkpointsTransform)
        { 
            tr.GetComponent<CheckpointSingle>().DrawMesh();
        }
    }
}
