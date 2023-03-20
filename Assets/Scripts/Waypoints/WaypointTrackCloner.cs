using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Waypoints
{
    [RequireComponent(typeof(WaypointTrack))]
    public class WaypointTrackCloner : MonoBehaviour
    {
        [Space]
        [ShowInInspector, ReadOnly]
        private WaypointTrack _track;
        [ShowInInspector, ReadOnly]
        private WaypointTrack _clone;

        [HorizontalGroup("Split")]
        [Button(ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f)]
        private void ReverseOriginTrack()
        {
            _track ??= GetComponent<WaypointTrack>();

            Stack<Vector3> posStack = new Stack<Vector3>();

            Transform[] nodesArray = _track.waypointList.items;

            for (int i = 0; i < nodesArray.Length; i++)
            {
                posStack.Push(nodesArray[i].position);
            }

            for (int i = 0; i < nodesArray.Length; i++)
            {
                nodesArray[i].position = posStack.Pop();
            }
        }

        [HorizontalGroup("Split", 0.5f)]
        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton), GUIColor(0.5f, 1f, 1f)]
        private void CloneOriginTrack(Color drawColor)
        { 
            _track ??= GetComponent<WaypointTrack>();

            _clone = Instantiate(_track, _track.transform.parent);

            _clone.SetDrawColor(drawColor);
        }

        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton), GUIColor(0f, 1f, 0f)]
        private void CloneAndReverse(Color drawColor)
        { 
            CloneOriginTrack(drawColor);

            Stack<Vector3> posStack = new Stack<Vector3>();

            Transform[] nodesArray = _clone.waypointList.items;

            for (int i = 0; i < nodesArray.Length; i++)
            {
                posStack.Push(nodesArray[i].position);
            }

            for (int i = 0; i < nodesArray.Length; i++)
            {
                nodesArray[i].position = posStack.Pop();
            }

            _clone.SetDrawColor(drawColor);
        }

        [Button(ButtonSizes.Small), GUIColor(1f, 0.2f, 0)]
        private void DeleteCurrentClone()
        { 
            if(_clone != null)
                DestroyImmediate(_clone.gameObject);
        }
    }
}
