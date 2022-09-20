using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointSingle : MonoBehaviour
{
    private CheckpointsTrack _checkpointsTrack;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CarDriverAI>(out CarDriverAI carDriverAI))
        {
            _checkpointsTrack.CarThoughCheckpoint(this, other.transform, carDriverAI.Finish());
            Transform nextTraget = _checkpointsTrack.GetNextTargetFor(other.transform);
            carDriverAI.SetTarget(nextTraget);
        }
    }

    public void SetCheckpoinsTrack(CheckpointsTrack checkpointsTrack) => _checkpointsTrack = checkpointsTrack;

}
