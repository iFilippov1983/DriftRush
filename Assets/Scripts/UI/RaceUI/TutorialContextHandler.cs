using DG.Tweening;
using RaceManager.Root;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;
using IInitializable = RaceManager.Root.IInitializable;

namespace RaceManager.UI
{
    public class TutorialContextHandler : MonoBehaviour, IInitializable
    {
        public enum ContextAction
        { 
            ShowDoneImage,
            TimeScaleDown,
            TimeScaleUp
        }

        [Serializable]
        public class TutorialContext
        { 
            public GameFlagType gameFlag;
            public List<ContextAction> actions;
            [ShowIf("ShowDoneImage")]
            public RectTransform showPlaceRect;

            private bool ShowDoneImage => actions.Contains(ContextAction.ShowDoneImage);
        }

        [SerializeField] private float _defaultTimeScale = 1f;
        [SerializeField] private float _minTimeScale = 0.2f;
        [SerializeField] private float _fixedTimeFactor = 0.01f;
        [SerializeField] private float _animDuration = 0.7f;
        [Space]
        [SerializeField] private RectTransform _doneSignRect;
        [SerializeField] private List<TutorialContext> _contexts;

        private GameFlagsHandler _flagsHandler;
        private Tween _currentDoneTween;

        [Inject]
        private void Construct(GameFlagsHandler flagsHandler, Resolver resolver)
        {
            resolver.Add(this);
            _flagsHandler = flagsHandler;
        }

        private bool Passed(GameFlagType gameFlag) => _flagsHandler.HasFlag(gameFlag);

        public void Initialize()
        {
            foreach (var context in _contexts) 
            {
                foreach (var action in context.actions)
                {
                    switch (action)
                    {
                        case ContextAction.ShowDoneImage:
                            ShowDoneImageOnFlag(context);
                            break;
                        case ContextAction.TimeScaleDown:
                            TimeScaleDownOnFlag(context.gameFlag);
                            break;
                        case ContextAction.TimeScaleUp:
                            TimeScaleUpOnFlag(context.gameFlag);
                            break;
                    }
                }
            }

            //Debug.Log($"[Initial] TS: {Time.timeScale}; FDT: {Time.fixedDeltaTime}");
        }

        private void ShowDoneImageOnFlag(TutorialContext context)
        {
            if (Passed(context.gameFlag))
                return;

            _flagsHandler
                .Subscribe(context.gameFlag, () => ShowDoneImage(context.showPlaceRect))
                .AddTo(this);

            //$"[Tutor Context] ShowDoneImage => OnFlag: [{context.gameFlag}]".Log(Logger.ColorYellow);
        }

        private void TimeScaleDownOnFlag(GameFlagType gameFlag)
        { 
            if(Passed(gameFlag))
                return;

            _flagsHandler
                .Subscribe(gameFlag, TimeScaleDown)
                .AddTo(this);
        }

        private void TimeScaleUpOnFlag(GameFlagType gameFlag)
        { 
            if(Passed(gameFlag))
                return;

            _flagsHandler 
                .Subscribe(gameFlag, TimeScaleUp)
                .AddTo(this);
        }

        private void ShowDoneImage(RectTransform rect)
        {
            if (_currentDoneTween != null || _currentDoneTween.IsActive())
            { 
                _currentDoneTween?.Complete(true);
                _currentDoneTween = null;
            }

            _doneSignRect.transform.position = rect.transform.position;
            _doneSignRect.SetActive(true);
            _currentDoneTween = _doneSignRect.DOPunchScale(new Vector3(1.01f, 1.01f, 1f), _animDuration * 2, 7, 0)
                .OnComplete(() =>
                {
                    _doneSignRect.SetActive(false);
                    _currentDoneTween = null;
                });

            //Debug.Log($"[ShowDoneImage] Pos: {rect.position}");
        }

        private void TimeScaleDown()
        {
            Time.timeScale = _minTimeScale;
            Time.fixedDeltaTime = Time.timeScale * _fixedTimeFactor;

            //Debug.Log($"[Down] TS: {Time.timeScale}; FDT: {Time.fixedDeltaTime}");
        }

        private void TimeScaleUp()
        {
            Time.timeScale = _defaultTimeScale;
            Time.fixedDeltaTime = Time.timeScale * _fixedTimeFactor;

            //Debug.Log($"[Up] TS: {Time.timeScale}; FDT: {Time.fixedDeltaTime}");
        }

        private void OnDestroy()
        {
            _currentDoneTween?.Complete(true);
        }
    }
}