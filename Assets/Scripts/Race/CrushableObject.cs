using System;
using UnityEngine;
using UniRx;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;

namespace RaceManager.Race
{
    public class CrushableObject : MonoBehaviour, ICountableCollision
    {
        [ShowInInspector, ReadOnly]
        private string _id;
        [ShowInInspector, ReadOnly]
        private int _layer;

        public string ID => _id;
        public int Layer => _layer;

        private void Awake()
        {
            _id = MakeId();
            _layer = gameObject.layer;
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