using System;
using UnityEngine;
using UniRx;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using static ToonyColorsPro.ShaderGenerator.Enums;

namespace RaceManager.Race
{
    public class CrushableObject : MonoBehaviour, ICountableCollision
    {
        [SerializeField]
        private float _lifeTimeAfterCrush = 10f;
        [ShowInInspector, ReadOnly]
        private string _id;
        [ShowInInspector, ReadOnly]
        private int _layer;
        [SerializeField]
        private MeshCollider[] _meshes = new MeshCollider[0];

        private BoxCollider _collider;

        public string ID => _id;
        public int Layer => _layer;

        private void Awake()
        {
            _id = MakeId();
            _layer = gameObject.layer;
            _collider = GetComponent<BoxCollider>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_collider == null)
                return;

            if (collision.gameObject.layer == LayerMask.NameToLayer(Tools.Layer.Car))
            {
                Destroy(_collider);
                _collider = null;

                for (int i = 0; i < _meshes.Length; i++)
                {
                    _meshes[i].enabled = true;
                    _meshes[i].gameObject.AddComponent<Rigidbody>();
                }

                Destroy(gameObject, _lifeTimeAfterCrush);
            }
        }

        private string MakeId()
        {
            StringBuilder builder = new StringBuilder();
            Enumerable
               .Range(65, 26)
                .Select(e => ((char)e).ToString())
                .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
                .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
                .OrderBy(e => Guid.NewGuid())
                .Take(11)
                .ToList().ForEach(e => builder.Append(e));
            return builder.ToString();
        }
    }
}