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

        private GameFlagsHandler _flagsHandler;
        private Vector3 _originalScale;

        public bool TimeToAct => _flagsHandler.HasFlag(_key);

        [Inject]
        private void Construct(GameFlagsHandler flagsHandler, Resolver resolver)
        {
            resolver.Add(this);
            _flagsHandler = flagsHandler;
            _originalScale = transform.localScale;
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
            if (TimeToAct)
                return;

            DeactivateSelf();

            _flagsHandler
                .Subscribe(_key, ActivateSelf)
                .AddTo(this);

            $"EnableOnFlag subscribed => Key: [{_key}] => Object: [{gameObject.name}]".Log(Logger.ColorYellow);
        }

        private void DisableOnFlag()
        {
            if (TimeToAct)
            { 
                DeactivateSelf();
                return;
            }

            _flagsHandler
                .Subscribe(_key, DeactivateSelf)
                .AddTo(this);

            $"DisableOnFlag subscribed => Key: [{_key}] => Object: [{gameObject.name}]".Log(Logger.ColorYellow);
        }

        private void DestroyOnFlag()
        {
            if (TimeToAct)
            {
                DestroySelf();
                return;
            }

            _flagsHandler
                .Subscribe(_key, DestroySelf)
                .AddTo(this);

            $"DestroyOnFlag subscribed => Key: [{_key}] => Object: [{gameObject.name}]".Log(Logger.ColorYellow);
        }

        private void DestroySelf()
        {
            gameObject.DoDestroy();
        }

        private void DeactivateSelf()
        {
            $"- Deactivate object [{gameObject.name}] with key => {_key}".Log();
            transform.localScale = Vector3.zero;
        }

        private void ActivateSelf()
        {
            $"+ Activating object [{gameObject.name}] with key => {_key}".Log();
            transform.localScale = _originalScale;
        }
    }
}
