using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    [Serializable]
    public class AnimationData
    {
        public AnimationType animationType;
        public float animationDuration;

        [ShowIf("CanUseRects")]
        public bool useRects;
        [ShowIf("Animatable")]
        public bool useImages;

        [Space]
        [ShowIf("CanSelectTransform")]
        public bool useMoveFromToTransform;
        [ShowIf("useMoveFromToTransform")]
        public Transform moveFromToTransform;

        [Space]
        [ShowIf("HaveToAnimateRects")]
        public List<RectTransform> rectsToAnimate = new List<RectTransform>();

        [Space]
        [ShowIf("useImages")]
        public List<Image> imagesToAnimate = new List<Image>();

        #region ShowIf Properties

        private bool Animatable => animationType != AnimationType.None && animationDuration > 0;
        private bool CanUseRects => Animatable && !NeedToFade;
        private bool HaveToAnimateRects => CanUseRects && useRects;
        private bool CanSelectTransform => Animatable && (useRects || useImages) && NeedToMove;
        private bool NeedToMove =>
            animationType == AnimationType.MoveOut
            || animationType == AnimationType.MoveIn
            || animationType == AnimationType.MoveFromTo
            || animationType == AnimationType.MoveInX
            || animationType == AnimationType.MoveOutX
            || animationType == AnimationType.MoveInY
            || animationType == AnimationType.MoveOutY;
        private bool NeedToFade => 
            animationType == AnimationType.FadeOut 
            || animationType == AnimationType.FadeIn 
            || animationType == AnimationType.FadeInOutLoop;

        #endregion
    }
}

