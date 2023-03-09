using RaceManager.Progress;
using RaceManager.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.UI
{
    public class CurrencyAnimator : MonoBehaviour
    {
        [SerializeField] private int _objToSpawnMin;
        [SerializeField] private int _objToSpawnMax;

        private GameObject _animCurrencyPrefab;

        private List<AnimatableImage> _currenciesList = new List<AnimatableImage>();
        private Stack<AnimatableImage> _currenciesStack = new Stack<AnimatableImage>();

        private GameObject AnimCurrencyPrefab
        {
            get
            {
                if (_animCurrencyPrefab == null)
                    _animCurrencyPrefab = ResourcesLoader.LoadPrefab(ResourcePath.AnimatableCurrencyPrefab);
                return _animCurrencyPrefab;
            }
        }

        public void SpawnRewardAndMove(RewardType type, Transform spawnTransform, Transform moveToTransform, float animationDuration = 0f)
        { 
            
        }

    }
}

