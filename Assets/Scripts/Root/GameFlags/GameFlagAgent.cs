using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace RaceManager.Root
{
    public class GameFlagAgent : MonoBehaviour, IInitializable
    {
        public enum AgentAction
        { 
            EnableOnFlag,
            DisableOnFlag,
            DestroyOnFlag
        }

        [SerializeField] private GameFlagType _key;
        [SerializeField] private AgentAction _action;

        private List<IAgentsHelper> _helpers;
        private GameFlagsHandler _flagsHandler;
        private Vector3 _originalScale;

        public bool HasHelpers => _helpers.Count > 0;

        [Inject]
        private void Construct(GameFlagsHandler flagsHandler, Resolver resolver)
        {
            resolver.Add(this);
            _flagsHandler = flagsHandler;
            _originalScale = transform.localScale;
            _helpers = new List<IAgentsHelper>(GetComponentsInChildren<IAgentsHelper>());
        }

        public void Initialize()
        {
            switch (_action)
            {
                case AgentAction.EnableOnFlag:
                    EnableOnFlag();
                    break;

                case AgentAction.DisableOnFlag:
                    DisableOnFlag();
                    break;

                case AgentAction.DestroyOnFlag:
                    DestroyOnFlag();
                    break;
            }
        }

        private void EnableOnFlag()
        {
            if (_flagsHandler.HasFlag(_key))
                return;

            DeactivateSelf();
            _flagsHandler
                .Subscribe(_key, ActivateSelf)
                .AddTo(this);

            $"EnableOnFlag sub => Key: [{_key}] => Object: [{gameObject.name}] => Helpers count: [{_helpers.Count}]".Log(Logger.ColorYellow);
        }

        private void DisableOnFlag()
        {
            if (_flagsHandler.HasFlag(_key))
            { 
                DeactivateSelf();
                return;
            }

            _flagsHandler
                .Subscribe(_key, DeactivateSelf)
                .AddTo(this);

            $"DisableOnFlag sub => Key: [{_key}] => Object: [{gameObject.name}] => Helpers count: [{_helpers.Count}]".Log(Logger.ColorYellow);
        }

        private void DestroyOnFlag()
        {
            if (_flagsHandler.HasFlag(_key))
            {
                DestroySelf();
                return;
            }

            _flagsHandler
                .Subscribe(_key, DestroySelf)
                .AddTo(this);

            $"DestroyOnFlag sub => Key: [{_key}] => Object: [{gameObject.name}] => Helpers count: [{_helpers.Count}]".Log(Logger.ColorYellow);
        }

        private void DestroySelf()
        {
            if (HasHelpers)
            {
                foreach (var helper in _helpers)
                    helper.Destroy();
            }

            gameObject.DoDestroy();
        }

        private void DeactivateSelf()
        {
            if (HasHelpers)
            {
                foreach (var helper in _helpers)
                    helper.Deactivate();
            }

            $"- Deactivate object [{gameObject.name}] with key => {_key}".Log();
            transform.localScale = Vector3.zero;
        }

        private void ActivateSelf()
        {
            $"+ Activating object [{gameObject.name}] with key => {_key}".Log();
            transform.localScale = _originalScale;

            if (HasHelpers)
            {
                foreach (var helper in _helpers)
                    helper.Activate();
            }
        }
    }
}
