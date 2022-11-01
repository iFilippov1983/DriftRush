using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RaceManager.Cars
{
    [Serializable]
    public class WheelPart : IPart
    {
        [SerializeField] private WheelColliderHandler _wheelHandler;
        [SerializeField] private GameObject _wheelGameObject;
        [SerializeField] private WheelProperty _wheelProperty;
        [ShowInInspector, ReadOnly]
        private bool _isActive;

        public PartType Type => PartType.Wheel;
        public WheelProperty Property => _wheelProperty;

        public bool IsActive
        {
            get => _isActive;
            set
            { 
                _isActive = value;

                if (_isActive)
                {
                    _wheelGameObject.transform.localScale = _wheelProperty.WheelScale;
                    var config = _wheelHandler.Config;
                    config.Radius = _wheelProperty.WheelRadius;
                    _wheelHandler.UpdateConfig(config);
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

    [Serializable]
    public class SuspentionPart : IPart
    {
        [SerializeField] private WheelColliderHandler _wheelHandler;
        [SerializeField] private SuspentionProperty _suspentionProperty;
        [ShowInInspector, ReadOnly]
        private bool _isActive;

        public PartType Type => PartType.Suspention;
        public SuspentionProperty SuspentionProperty => _suspentionProperty;

        public bool IsActive
        { 
            get => _isActive;
            set
            {
                _isActive = value;

                if (_isActive)
                {
                    var config = _wheelHandler.Config;
                    config.SuspensionDistance = _suspentionProperty.SuspentionHeight;
                    _wheelHandler.UpdateConfig(config);
                }
            }
        }
    }

    [Serializable]
    public class BumperPart : IPart
    {
        [SerializeField] private GameObject _bumperObject;
        [ShowInInspector, ReadOnly]
        private bool _isActive;

        public PartType Type => PartType.Bumper;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                if(_bumperObject != null)
                    _bumperObject.gameObject.SetActive(_isActive);
            }
        }
    }

    [Serializable]
    public class BodyKitPart : IPart
    {
        [SerializeField] private GameObject _bodyKitObject;
        [ShowInInspector, ReadOnly]
        private bool _isActive;

        public PartType Type => PartType.BodyKit;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                if(_bodyKitObject != null)
                    _bodyKitObject.gameObject.SetActive(_isActive);
            }
        }
    }
}
