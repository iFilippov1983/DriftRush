using UnityEngine;

namespace RaceManager.Waypoints
{
    public sealed class WaypointCircuit : WaypointTrack
    {
        protected override void CachePositionsAndDistances()
        {
            // transfer the position of each point and distances between points to arrays for
            // speed of lookup at runtime
            _points = new Vector3[Waypoints.Length + 1];
            _distances = new float[Waypoints.Length + 1];

            _accumulateDistance = 0;
            for (int i = 0; i < _points.Length; ++i)
            {
                var t1 = Waypoints[(i) % Waypoints.Length];
                var t2 = Waypoints[(i + 1)%Waypoints.Length];
                if (t1 != null && t2 != null)
                {
                    Vector3 p1 = t1.position;
                    Vector3 p2 = t2.position;
                    _points[i] = Waypoints[i % Waypoints.Length].position;
                    _distances[i] = _accumulateDistance;
                    _accumulateDistance += (p1 - p2).magnitude;
                }
            }
        }

        public override Vector3 GetRoutePosition(float dist)
        {
            int point = 0;

            if (Length == 0)
            {
                Length = _distances[_distances.Length - 1];
            }

            dist = Mathf.Repeat(dist, Length);

            while (_distances[point] < dist)
            {
                ++point;
            }


            // get nearest two points, ensuring points wrap-around start & end of circuit
            p1n = ((point - 1) + _numPoints) % _numPoints;
            p2n = point;

            // found point numbers, now find interpolation value between the two middle points

            i = Mathf.InverseLerp(_distances[p1n], _distances[p2n], dist);

            if (_smoothRoute)
            {
                // smooth catmull-rom calculation between the two relevant points


                // get indices for the surrounding 2 points, because
                // four points are required by the catmull-rom function
                p0n = ((point - 2) + _numPoints) % _numPoints;
                p3n = (point + 1) % _numPoints;

                // 2nd point may have been the 'last' point - a dupe of the first,
                // (to give a value of max track distance instead of zero)
                // but now it must be wrapped back to zero if that was the case.
                p2n = p2n % _numPoints;

                P0 = _points[p0n];
                P1 = _points[p1n];
                P2 = _points[p2n];
                P3 = _points[p3n];

                return CatmullRom(P0, P1, P2, P3, i);
            }
            else
            {
                // simple linear lerp between the two points:

                p1n = ((point - 1) + _numPoints) % _numPoints;
                p2n = point;

                return Vector3.Lerp(_points[p1n], _points[p2n], i);
            }
        }

        protected override void DrawGizmos(bool selected)
        {
            waypointList.circuit = this;
            if (Waypoints.Length > 1)
            {
                _numPoints = Waypoints.Length;

                CachePositionsAndDistances();
                Length = _distances[_distances.Length - 1];

                _altColor = new Color(_drawColor.r, _drawColor.g, _drawColor.b, _drawColor.a / 2);
                Gizmos.color = selected ? _drawColor : _altColor;
                Vector3 prev = Waypoints[0].position;
                if (_smoothRoute)
                {
                    for (float dist = 0; dist < Length; dist += Length/editorVisualisationSubsteps)
                    {
                        Vector3 next = GetRoutePosition(dist + 1);
                        Gizmos.DrawLine(prev, next);
                        prev = next;
                    }
                    Gizmos.DrawLine(prev, Waypoints[0].position);
                }
                else
                {
                    for (int n = 0; n < Waypoints.Length; ++n)
                    {
                        Vector3 next = Waypoints[(n + 1)%Waypoints.Length].position;
                        Gizmos.DrawLine(prev, next);
                        prev = next;
                    }
                }
            }
        }
    }
}
