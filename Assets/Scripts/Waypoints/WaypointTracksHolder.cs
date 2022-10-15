using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Waypoints
{
    public class WaypointTracksHolder : MonoBehaviour
    {
        [SerializeField]
        private WaypointTrack[] _tracks;
        private List<GameObject> _trackNodes;

        private void Start()
        {
            _trackNodes = MakeNodes();
        }

        [Button]
        private List<GameObject> MakeNodes()
        {
            _tracks = GetComponentsInChildren<WaypointTrack>();
            var list = new List<GameObject>();
            var mainTrack = Array.Find(_tracks, t => t.MainTrack == true);

            for (int i = 0; i < mainTrack.Length; i++)
            {
                for (int k = 0; k < _tracks.Length; k++)
                {
                    GameObject parent = new GameObject();
                    parent.transform.position = mainTrack.Waypoints[i].transform.position;

                    _tracks[k].Waypoints[i].transform.SetParent(parent.transform);
                }
            }

            foreach (var node in list)
                node.transform.SetParent(transform);

            return list;
        }
    }
}
