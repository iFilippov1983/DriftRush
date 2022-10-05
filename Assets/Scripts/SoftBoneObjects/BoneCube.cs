using UnityEngine;

namespace RaceManager.SoftObjects
{
    public class BoneCube : MonoBehaviour
    {
        /*
            E --------- F
            |           |
            |   A --------- B
            |   |       |   |
            |   |       |   |
            H --|------ G   |
                |           |
                D --------- C
        */

        /*
            tD ---------tC
            |           |
            |   tA ---------tB
            |   |       |   |
            |   |       |   |
            bD--|------ bC  |
                |           |
                bA --------- bB
        */
        [Header("Bones")]
        public GameObject tA = null;
        public GameObject tB = null;
        public GameObject tC = null;
        public GameObject tD = null;

        public GameObject bA = null;
        public GameObject bB = null;
        public GameObject bC = null;
        public GameObject bD = null;

        [Header("Spring Joint Settings")]
        [Tooltip("Strength of spring")]
        public float Spring = 100f;
        [Tooltip("Higher the value the faster the spring oscillation stops")]
        public float Damper = 0.2f;
        [Header("Other Settings")]
        public Softbody.ColliderShape Shape = Softbody.ColliderShape.Box;
        public float ColliderSize = 0.002f;
        public float RigidbodyMass = 1f;
        public LineRenderer PrefabLine = null;
        public bool ViewLines = true;

        private void Start()
        {
            Softbody.Init(Shape, ColliderSize, RigidbodyMass, Spring, Damper, RigidbodyConstraints.None, PrefabLine, ViewLines);

            Softbody.AddCollider(ref tA);
            Softbody.AddCollider(ref tB);
            Softbody.AddCollider(ref tC);
            Softbody.AddCollider(ref tD);
            
            Softbody.AddCollider(ref bA);
            Softbody.AddCollider(ref bB);
            Softbody.AddCollider(ref bC);
            Softbody.AddCollider(ref bD);

            //down
            Softbody.AddSpring(ref tA, ref bA);
            Softbody.AddSpring(ref tB, ref bB);
            Softbody.AddSpring(ref tC, ref bC);
            Softbody.AddSpring(ref tD, ref bD);

            //across
            Softbody.AddSpring(ref tA, ref bC);
            Softbody.AddSpring(ref tB, ref bD);
            Softbody.AddSpring(ref tC, ref bA);
            Softbody.AddSpring(ref tD, ref bB);

            //top
            Softbody.AddSpring(ref tA, ref tB);
            Softbody.AddSpring(ref tB, ref tC);
            Softbody.AddSpring(ref tC, ref tD);
            Softbody.AddSpring(ref tD, ref tA);

            //bottom
            Softbody.AddSpring(ref bA, ref bB);
            Softbody.AddSpring(ref bB, ref bC);
            Softbody.AddSpring(ref bC, ref bD);
            Softbody.AddSpring(ref bD, ref bA);
        }
    }
}

