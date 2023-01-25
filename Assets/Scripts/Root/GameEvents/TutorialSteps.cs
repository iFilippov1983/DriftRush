using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace RaceManager.Root
{
    [Serializable]
    public class TutorialSteps : SerializedMonoBehaviour, ISaveable
    {
        public enum TutorialEventType
        { 
            PressButton,
            GetReward,
            WaitForNotification,
            TapScreen,
            HoldScreenTap,
            ReleaseScreenTap,
            WinRace
        }

        [Serializable]
        public class TutorialStep
        {
            public int Number;
            public TutorialEventType Type;
            public string EventFilter;
            public int Count;

            public GameFlagType FlagType;
        }

        public int CurrentStep;

        [TableList]
        public List<TutorialStep> Steps;

        [Tooltip("Just for example, to use in EventFilter field. Isn't used in code.")]
        [ShowInInspector]
        private NotificationType _notificationExamples;

        private GameEvents _gameEvents;
        private GameFlagsHandler _flagsHandler;
        private SaveManager _saveManager;

        [Inject]
        private void Construct(GameEvents gameEvents, GameFlagsHandler flagsHandler, SaveManager saveManager)
        { 
            _gameEvents = gameEvents;
            _flagsHandler = flagsHandler;
            _saveManager = saveManager;
        }

        public void RunStep()
        {
            List<TutorialStep> steps = Steps.FindAll(s => s.Number == CurrentStep);
            Subscribe(steps.ToArray());

            string color = steps.Count == 0 ? Logger.ColorRed : Logger.ColorGreen;
            $"Step {CurrentStep} subscribers count => {steps.Count}".Log(color);
        }

        private void Subscribe(params TutorialStep[] steps)
        {
            foreach (var step in steps)
            {
                switch (step.Type)
                {
                    case TutorialEventType.PressButton:
                        _gameEvents.ButtonPressed
                            .Where(s => s.ToLower() == step.EventFilter.ToLower()) // Check button name
                            .Take(step.Count)
                            .Last()
                            .Subscribe(s => OnTrigger(step));
                        break;

                    case TutorialEventType.GetReward:
                        _gameEvents.GotReward
                            .Where(r => r.Type.ToString().ToLower() == step.EventFilter.ToLower()) // Check Reward Type
                            .Take(step.Count)
                            .Last()
                            .Subscribe(r => OnTrigger(step));
                        break;

                    case TutorialEventType.WaitForNotification:
                        _gameEvents.Notification
                            .Where(s => s.ToLower() == step.EventFilter.ToLower())
                            .Take(step.Count)
                            .Last()
                            .Subscribe(s => OnTrigger(step));
                        break;

                    case TutorialEventType.TapScreen:
                        _gameEvents.ScreenTaped
                            .Take(step.Count)
                            .Last()
                            .Subscribe(u => OnTrigger(step));
                        break;

                    case TutorialEventType.HoldScreenTap:
                        _gameEvents.ScreenTapHold
                            .Take(step.Count)
                            .Last()
                            .Subscribe(u => OnTrigger(step));
                        break;

                    case TutorialEventType.ReleaseScreenTap:
                        _gameEvents.ScreenTapReleased
                            .Take(step.Count)
                            .Last()
                            .Subscribe(u => OnTrigger(step));
                        break;

                    case TutorialEventType.WinRace:
                        _gameEvents.RaceWin
                            //.Where(v => v == step.Count)
                            .Take(step.Count)
                            .Last()
                            .Subscribe(c => OnTrigger(step));
                        break;
                }
            }
        }

        private void OnTrigger(TutorialStep step)
        { 
            SetFlag(step);
            NextStep();
            _saveManager.Save();
        }

        private void NextStep()
        {
            CurrentStep++;
            RunStep();
        }

        private void SetFlag(TutorialStep step)
        {
            $"Flag is set to => {step.FlagType}".Log();
            _flagsHandler.Add(step.FlagType);
        }

        public class SaveData { public int step; }
       
        public Type DataType() => typeof(SaveData);

        public void Load(object data)
        {
            CurrentStep = ((SaveData)data).step;
            $"Current step LOAD: {CurrentStep}".Log();
        }

        public object Save()
        {
            $"Current step SAVE: {CurrentStep}".Log();

            return new SaveData() { step = CurrentStep };
        }
    }
}
