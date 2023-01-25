using RaceManager.Root;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class UIButtonHandler : MonoBehaviour, IAgentsHelper
    {
        public enum HandleActionType
        { 
            Click,
            InteractableTrue,
            InteractableFalse
        }

        [Serializable]
        public class ButtonToHandle
        { 
            public Button button;

            public bool onActivate;
            [ShowIf("onActivate")]
            public HandleActionType onActivateAction;

            public bool onDeactivate;
            [ShowIf("onDeactivate")]
            public HandleActionType onDeactivateAction;
        }

        [SerializeField] private List<ButtonToHandle> _buttonsToHandle = new List<ButtonToHandle>();

        [Button]
        public void Activate()
        {
            foreach (var button in _buttonsToHandle)
            {
                if (button.onActivate)
                {
                    $"Handling button [{button.button.name}] => ACTIVATE".Log();

                    HandleActionFor(ref button.button, button.onActivateAction);
                }
            }
        }

        [Button]
        public void Deactivate()
        {
            foreach (var button in _buttonsToHandle)
            {
                if (button.onDeactivate)
                {
                    $"Handling button [{button.button.name}] => DEACTIVATE".Log();

                    HandleActionFor(ref button.button, button.onDeactivateAction);
                } 
            }
        }

        private void HandleActionFor(ref Button button, HandleActionType actionType)
        {
            switch (actionType)
            {
                case HandleActionType.Click:
                    button.onClick?.Invoke();
                    break;
                case HandleActionType.InteractableTrue:
                    button.interactable = true;
                    break;
                case HandleActionType.InteractableFalse:
                    button.interactable = false;
                    break;
            }
        }
    }
}
