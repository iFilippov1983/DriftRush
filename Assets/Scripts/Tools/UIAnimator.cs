using UnityEngine;
using DG.Tweening;

namespace RaceManager.Tools
{
    public static class UIAnimator
    {
        public static void DoShake(GameObject go)
        {
            DoShake(go.transform);
        }

        public static void DoShake(Transform transform)
        { 
            
        }

        public static void DoShake(RectTransform rect, float duration = 1)
        {
            rect.DOShakeAnchorPos(duration);
        }
    }
}
