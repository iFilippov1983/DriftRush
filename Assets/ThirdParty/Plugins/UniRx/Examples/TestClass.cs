// for uGUI(from 4.6)
#if !(UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5)

using UnityEngine;
using System.Collections.Generic;
using UniRx.Toolkit;
using UniRx.InternalUtil;
using UniRx.Operators;
using UniRx.Triggers;
using System;

namespace UniRx.Examples
{
    public class TestClass : MonoBehaviour
    {

        public List<int> keyList = new List<int>();
        public List<int> valueList = new List<int>();

        private Dictionary<int, int> _dictionary;
        private float _period = 0.25f;

        private void Start()
        {
            CreateDistionary();

            var str = this.UpdateAsObservable()
                .Where(_ => Input.GetMouseButtonDown(0))
                .Select(_ => Input.mousePosition);

            str.Where(pos => pos.x >= 100 && pos.y >= 100)
                .Subscribe(pos => Check(pos.x, pos.y));

            var click = this.UpdateAsObservable()
                .Where(_ => Input.GetMouseButtonDown(0));

            click.Buffer(
                click.Throttle(TimeSpan.FromSeconds(_period)))
                .Where(x => x.Count >= 2)
                .Subscribe(_ => Debug.Log("Double click"));

        }

        private void Check(float x, float y)
        {
            foreach (var pair in _dictionary)
            {
                if(pair.Value > x && pair.Key > y)
                    Debug.Log($"Point fits: {pair.Key}, {pair.Value}");
            }
        }

        private void CreateDistionary()
        {
            _dictionary = new Dictionary<int, int>(20);
            for (int i = 0; i < keyList.Count; i++)
            {
                _dictionary.Add(keyList[i], valueList[i]);
            }
        }
    }
}

#endif