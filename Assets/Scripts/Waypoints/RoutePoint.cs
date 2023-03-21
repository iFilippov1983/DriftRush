using UnityEngine;

namespace RaceManager.Waypoints
{
    public struct RoutePoint 
    {
        public Vector3 position;
        public Vector3 direction;
        public float recomendedSpeed;

        public RoutePoint(Vector3 position, Vector3 direction, float recomendedSpeed)
        {
            this.position = position;
            this.direction = direction;
            this.recomendedSpeed = recomendedSpeed;
        }
    }
}
    
