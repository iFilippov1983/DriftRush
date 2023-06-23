using DG.Tweening;
using RaceManager.Root;
using RaceManager.Services;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class AdsDependentObjectsHandler : MonoBehaviour
    {
        [SerializeField] private bool _toggleButtonsInteraction;
        [ShowIf("_toggleButtonsInteraction")]
        [SerializeField] private List<Button> _buttons = new List<Button>();

        [SerializeField] private List<GameObject> _adsAvaObjects = new List<GameObject>();
        [SerializeField] private List<GameObject> _adsNotAvaObjects = new List<GameObject>();

        private Dictionary<int, DOTweenAnimation> _avaAnimations = new Dictionary<int, DOTweenAnimation>();
        private Dictionary<int, DOTweenAnimation> _notAvaAnimations = new Dictionary<int, DOTweenAnimation>();

        private MaxSdkAdvertisement Advertisment => Singleton<MaxSdkAdvertisement>.Instance;

        private void Awake()
        {
            InitializeDictionaries();
        }

        private void Start()
        {
            StartHandling();
        }

        private void InitializeDictionaries()
        {
            foreach (GameObject obj in _adsAvaObjects)
            {
                if (obj.TryGetComponent(out DOTweenAnimation animation))
                {
                    _avaAnimations.Add(_adsAvaObjects.IndexOf(obj), animation);
                }
            }

            foreach (GameObject obj in _adsNotAvaObjects)
            {
                if (obj.TryGetComponent(out DOTweenAnimation animation))
                { 
                    _notAvaAnimations.Add(_adsNotAvaObjects.IndexOf(obj), animation);
                }
            }
        }

        private void StartHandling()
        {
            this.UpdateAsObservable()
                .Subscribe(_ => 
                {
                    if(gameObject.activeInHierarchy)
                        UpdateStatus(Advertisment.IsRewardedAdReady);
                }).AddTo(this);
        }

        private void UpdateStatus(bool adsAvailable)
        {
            ActivateDependents(adsAvailable, _adsAvaObjects, _avaAnimations);
            ActivateDependents(!adsAvailable, _adsNotAvaObjects, _notAvaAnimations);

            _buttons.ForEach(button => { button.interactable = adsAvailable; });
        }

        private void ActivateDependents(bool activate, List<GameObject> dependentObjects, Dictionary<int, DOTweenAnimation> animations)
        {
            dependentObjects.ForEach(o =>
            { 
                o.SetActive(activate);

                int key = dependentObjects.IndexOf(o);

                if (animations.ContainsKey(key))
                {
                    if (activate)
                    {
                        animations[key].tween.Play();
                    }
                    else if (animations[key].tween.IsPlaying())
                    {
                        animations[key].DOPause();
                    }
                }
            });
        }
    }
}
