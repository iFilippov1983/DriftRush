using UnityEngine;
using DG.Tweening;
using System;
using System.Threading.Tasks;

namespace RaceManager.Tools
{
    public class UIAnimator
    {
        public void DoShake(GameObject go)
        {
            DoShake(go.transform);
        }

        public void DoShake(Transform transform)
        { 
            
        }

        public void DoShake(RectTransform rect, float duration = 1)
        {
            rect.DOShakeAnchorPos(duration);
        }

        public void DoScaleUpDown(RectTransform rect, float duration, float targetScaleFactor = 1.25f, int scaleCycles = 1, Action onCompleteAction = null)
        { 
            Vector3 originScale = rect.localScale;
            Vector2 targetScale = originScale * targetScaleFactor;

            if (scaleCycles < 1) scaleCycles = 1;
            int[] cycles = new int[scaleCycles];

            float d = duration / scaleCycles;

            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < cycles.Length; i++)
            {
                sequence.Append(rect.DOScale(targetScale, d / 2));
                sequence.Append(rect.DOScale(originScale, d / 2));
            }

            if (onCompleteAction is null) return;
                sequence.OnComplete(() => onCompleteAction?.Invoke());

            sequence.Play();
        }

        public void DoScale(RectTransform rect, Vector3 targetScale, float duration, Action onCompleteAction = null)
        {
            var tween = rect.DOScale(targetScale, duration);

            if (onCompleteAction is null) return;
            tween.OnComplete(() => onCompleteAction?.Invoke());
        }
    }
}
