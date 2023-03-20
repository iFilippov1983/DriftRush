using Sirenix.OdinInspector;
using System;
using System.Collections;
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
        [SerializeField] private TMP_Text _moreWinsText;
        [SerializeField] private GameObject _lootboxImage;
        [SerializeField] private Image[] _images = new Image[3];

        public TMP_Text MoreWinsText => _moreWinsText;
        public GameObject LootboxImage => _lootboxImage;
        public Image[] Images => _images;

        public Action OnImagesDisableComplete;

        private IEnumerator DisableImages()
        {
            for (int i = _images.Length - 1; i >= 0 ; i--)
            {
                yield return FadeImage(_images[i]);
            }

            OnImagesDisableComplete?.Invoke();
        }

        private IEnumerator FadeImage(Image image)
        {
            float alfa = image.color.a;
            Color color = image.color;

            while (image.color.a > 0)
            { 
                color = image.color;
                color.a -= Time.deltaTime * MarksFadeSpeed;
                image.color = color;

                yield return null;
            }

            image.SetActive(false);
            color.a = alfa;
            image.color = color;
        }

        public void OnAnimationFinish()
        {
            StartCoroutine(DisableImages());
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}

