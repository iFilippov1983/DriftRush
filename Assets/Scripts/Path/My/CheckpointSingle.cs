using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointSingle : MonoBehaviour
{
    [SerializeField]
    private GameObject _meshObject;
    private CheckpointsTrack _checkpointsTrack;
    private List<CarDriverAI> _pasedDrivers;
    [ShowInInspector, ReadOnly]
    private bool _drawMesh = true;

    private void Awake()
    {
        _pasedDrivers = new List<CarDriverAI>();
        _drawMesh = false;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.TryGetComponent<CarMotor>(out CarMotor carMotor))
    //    {
    //        CarDriverAI carDriverAI = carMotor.CarDriverAI;
    //        if (_pasedDrivers.Contains(carDriverAI))
    //            return;
    //        _pasedDrivers.Add(carDriverAI);
    //        bool continueTrack = _checkpointsTrack.CarThoughCheckpoint(this, carDriverAI.transform, (t) => carDriverAI.Finish(t));
    //        if (continueTrack)
    //        {
    //            Transform nextTraget = _checkpointsTrack.GetNextTargetFor(carDriverAI.transform);
    //            carDriverAI.SetTarget(nextTraget);
    //        }
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CarDriverAI>(out CarDriverAI carDriverAI))
        {
            if (_pasedDrivers.Contains(carDriverAI))
                return;
            _pasedDrivers.Add(carDriverAI);
            bool continueTrack = _checkpointsTrack.CarThoughCheckpoint(this, carDriverAI.transform, (t) => carDriverAI.Finish(t));
            if (continueTrack)
            {
                Transform nextTraget = _checkpointsTrack.GetNextTargetFor(carDriverAI.transform);
                carDriverAI.SetTarget(nextTraget);
            }
        }
    }

    public void SetCheckpoinsTrack(CheckpointsTrack checkpointsTrack) => _checkpointsTrack = checkpointsTrack;

    public bool Passed(Transform transformToCheck)
    {
        if (transform.TryGetComponent<CarDriverAI>(out CarDriverAI carDriverAI))
        { 
            if(_pasedDrivers.Contains(carDriverAI)) 
                return true;
            else 
                return false;
        }

        return false;
    }

    [Button]
    public void DrawMesh()
    {
        _drawMesh = !_drawMesh;
        _meshObject.SetActive(_drawMesh);
    }
}
