using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RaceManager.Cars
{
    [Serializable]
    public class Part : IPart
    {
        [SerializeField] private PartType _partType;

        [SerializeField]
        [ShowIf("_partType", PartType.Wheel)]
        private float _wheelRadius;
        [SerializeField]
        [ShowIf("_partType", PartType.Wheel)]
        private Vector3 _wheelScale;

        [SerializeField]
        [ShowIf("_partType", PartType.Suspention)]
        private float _suspentionHeight;

        [SerializeField]
        [HideIf("_partType", PartType.Suspention)]
        private GameObject _partObject;

        public PartType Type => _partType;
        public GameObject Object => _partObject;
        public PartProperty Property
        {
            get
            {
                switch (_partType)
                {
                    case PartType.Wheel:
                        return new PartProperty()
                        {
                            Name = nameof(_wheelRadius),
                            Value = _wheelRadius,
                            Scale = _wheelScale                           
                        };
                    case PartType.Suspention:
                        return new PartProperty() 
                        { 
                            Name = nameof(_suspentionHeight), 
                            Value = _suspentionHeight,
                            Scale = Vector3.one
                        };
                    case PartType.Bumper:
                    case PartType.BodyKit:
                    default:
                        return new PartProperty()
                        {
                            Name = string.Empty,
                            Value = 0,
                            Scale = Vector3.one
                        };
                }
            }
        }

        //[Button]
        //private void MakeId()
        //{
        //    if (string.IsNullOrEmpty(_id) == false)
        //        return;

        //    StringBuilder builder = new StringBuilder();
        //    Enumerable
        //       .Range(65, 26)
        //        .Select(e => ((char)e).ToString())
        //        .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
        //        .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
        //        .OrderBy(e => Guid.NewGuid())
        //        .Take(12)
        //        .ToList().ForEach(e => builder.Append(e));

        //    _id = builder.ToString();
        //}
    }
}
