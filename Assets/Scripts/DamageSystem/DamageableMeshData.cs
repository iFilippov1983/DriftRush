using UnityEngine;

namespace RaceManager.DamageSystem
{
    public struct DamageableMeshData
    {
        public Transform Transform;
        public MeshFilter MeshFilter;
        public MeshCollider MeshCollider;
        public bool Damaged;
        public Vector3[] Verts;
        public Mesh Mesh;
    }
}
