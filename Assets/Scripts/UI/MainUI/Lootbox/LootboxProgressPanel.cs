using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class LootboxProgressPanel : AnimatableSubject
    {
        [Space]
        [Header("Main Fields")]
        [ShowInInspector, ReadOnly] private const float MarksFadeSpeed = 5f;
        [SerializeField] private float _imageFadeDuration = 0.3f;
        [SerializeField] private TMP_Text _moreWinsText;
        [SerializeField] private GameObject _lootboxImage;
        [SerializeField] private Image[] _images = new Image[3];

        private Color _initialColor;
        private Sequence _sequence;

        public TMP_Text MoreWinsText => _moreWinsText;
        public GameObject LootboxImage => _lootboxImage;
        public Image[] Images => _images;

        public Action OnImagesDisableComplete;

        private void DisableImages()
        { 
            _sequence = DOTween.Sequence();

            _initialColor = _images[_images.Length - 1].color;
            _sequence.Append(_images[_images.Length - 1].DOFade(0, _imageFadeDuration));

            float d = _imageFadeDuration / 3;

            for (int i = _images.Length - 2; i > 0; i--)
            {
                _sequence.Insert(d, _images[i].DOFade(0, _imageFadeDuration));
                d += _imageFadeDuration / 3;
            }

            _sequence.AppendCallback(() => 
            {
                OnImagesDisableComplete?.Invoke();

                for (int i = 0; i < _images.Length; i++)
                {
                    _images[i].color = _initialColor;
                    _images[i].SetActive(false);
                }
            });
        }

        public void OnAnimationFinish()
        {
            DisableImages();
        }

        private void OnDestroy()
        {
            _sequence.Complete();
        }
    }
}

