using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class LootboxProgressPanel : MonoBehaviour
    {
        [ShowInInspector, ReadOnly] private const float MarksFadeSpeed = 10f;
        [SerializeField] private TMP_Text _moreWinsText;
        [SerializeField] private GameObject _lootboxImage;
        [SerializeField] private Image[] _images = new Image[3];

        public TMP_Text MoreWinsText => _moreWinsText;
        public GameObject LootboxImage => _lootboxImage;
        public Image[] Images => _images;

        private IEnumerator DisableImages()
        {
            _lootboxImage.SetActive(false);

            for (int i = 0; i < _images.Length; i++)
            {
                var time = DateTime.Now;
                yield return new WaitForSeconds(1f);
                yield return FadeImage(_images[i]);
                var span = DateTime.Now - time;

                $"{_images[i].name} - Disabled => {span.TotalSeconds} sec.".Log();
            }
        }

        private IEnumerator FadeImage(Image image)
        {
            float alfa = image.color.a;
            Color color = image.color;

            while (image.color.a > 0)
            { 
                color = image.color;
                color.a -= MarksFadeSpeed;
                image.color = color;

                yield return null;
            }

            image.SetActive(false);
            color.a = alfa;
            image.color = color;
        }

        public void OnAnimationStart()
        {
            StartCoroutine(DisableImages());
        }

        public void OnAnimationFinish()
        {
            _lootboxImage.SetActive(true);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}

