using RaceManager.Progress;
using RaceManager.Root;
using RaceManager.Tools;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace RaceManager.UI
{
    public class GameProgressPanel : AnimatableSubject
    {
        [SerializeField] private Button _backButton;
        [Space]
        [SerializeField] private TMP_Text _moneyAmountText;
        [SerializeField] private TMP_Text _gemsAmountText;
        [Space]
        [SerializeField] private GridLayoutGroup _progressStepsContent;
        [SerializeField] private RectTransform _progressStepsContentRect;
        [Space]
        [SerializeField] private MenuColorName _notReachedColor = MenuColorName.PurpleDark;
        [SerializeField] private MenuColorName _reachedColor = MenuColorName.PinkBright;

        private float _yOffset;
        private Vector3 _offsetPos;

        private GameObject _progressStepPrefab;
        private SpritesContainerCarCollection _spritesCars;
        private SpritesContainerRewards _spritesRewards;

        private List<ProgressStepView> _progressSteps = new List<ProgressStepView>();

        public Action<Button> OnButtonPressed;

        private UIAnimator Animator => Singleton<UIAnimator>.Instance;

        private GameObject ProgressStepPrefab
        {
            get 
            {
                if (_progressStepPrefab == null)
                    _progressStepPrefab = ResourcesLoader.LoadPrefab(ResourcePath.ProgressStepPrefab);
                return _progressStepPrefab;
            }
        }

        public Button BackButton => _backButton;
        public TMP_Text MoneyAmountText => _moneyAmountText;
        public TMP_Text gemsAmountText => _gemsAmountText; 

        [Inject]
        private void Construct(SpritesContainerCarCollection spritesContainerCars, SpritesContainerRewards spritesContainerRewards)
        { 
            _spritesCars = spritesContainerCars;
            _spritesRewards = spritesContainerRewards;
        }

        public void SetCupsAmountSlider(int cupsAmount)
        {
            _yOffset = 0f;
            int previouseGoal = 0;

            foreach (var stepView in _progressSteps)
            {
                if (cupsAmount > stepView.GoalCupsAmount)
                {
                    stepView.CupsAmountSlider.SliderFlag.SetActive(false);
                    stepView.CupsAmountSlider.SliderImage.fillAmount = 1f;
                    previouseGoal = stepView.GoalCupsAmount;

                    if (stepView.IsLast)
                    {
                        ActivateLevelImageAndPlaceToEdge(stepView, cupsAmount, false);
                    }

                    _yOffset += _progressStepsContent.cellSize.y;
                    _yOffset += _progressStepsContent.spacing.y;
                    continue;
                }
                
                if (stepView.GoalCupsAmount >= cupsAmount && cupsAmount > previouseGoal)
                {
                    stepView.CupsAmountSlider.CupsAmountText.text = cupsAmount.ToString();
                    stepView.CupsAmountSlider.SliderFlag.SetActive(true);

                    int stepSize = stepView.GoalCupsAmount - previouseGoal;
                    int localLevel = cupsAmount - previouseGoal;
                    float fillAmount = (float)localLevel / (float)stepSize;
                    stepView.CupsAmountSlider.SliderImage.fillAmount = fillAmount;

                    Vector3 localPos = stepView.CupsAmountSlider.SliderFlag.localPosition;
                    localPos.y = stepView.CupsAmountSlider.SliderImage.rectTransform.rect.height * fillAmount;
                    stepView.CupsAmountSlider.SliderFlag.localPosition = localPos;

                    previouseGoal = stepView.GoalCupsAmount;

                    if (stepView.GoalCupsAmount == cupsAmount)
                    {
                        _yOffset += _progressStepsContent.cellSize.y;
                        _yOffset += _progressStepsContent.spacing.y;
                    }
                    continue;
                }

                stepView.CupsAmountSlider.SliderFlag.SetActive(false);
                stepView.CupsAmountSlider.SliderImage.fillAmount = 0f;

                int indexCur = _progressSteps.IndexOf(stepView);
                if (indexCur - 1 >= 0)
                {
                    var stepPrev = _progressSteps[indexCur - 1];
                    if (stepPrev.GoalCupsAmount == cupsAmount)
                    {
                        ActivateLevelImageAndPlaceToEdge(stepView, cupsAmount, true);
                    }
                }

                if (cupsAmount == 0 && indexCur == 0)
                {
                    ActivateLevelImageAndPlaceToEdge(stepView, cupsAmount, true);
                }
            }

            _yOffset += _progressStepsContent.spacing.y;
            _yOffset -= _progressStepsContent.cellSize.y;
            _offsetPos = _progressStepsContent.transform.localPosition;
            _offsetPos.y -= _yOffset;
            OffsetContent();
        }

        public void OffsetContent() => _progressStepsContent.transform.localPosition = _offsetPos;
        private void OnButtonPressedMethod(Button button) => OnButtonPressed?.Invoke(button);

        private void ActivateLevelImageAndPlaceToEdge(ProgressStepView stepView, int cupsAmount, bool toZero)
        {
            stepView.CupsAmountSlider.CupsAmountText.text = cupsAmount.ToString();
            stepView.CupsAmountSlider.SliderFlag.SetActive(true);
            
            Vector3 localPos = stepView.CupsAmountSlider.SliderFlag.localPosition;
            localPos.y = toZero 
                ? 0f 
                : stepView.CupsAmountSlider.SliderImage.rectTransform.rect.height;
            stepView.CupsAmountSlider.SliderFlag.localPosition = localPos;
        }

        private void UpdateStepStatus(ProgressStep step, ProgressStepView stepView)
        {
            bool received = step.RewardsReceived;

            if (step.IsReached)
            {
                stepView.ClaimButton.SetActive(true);
                stepView.ClaimButton.SetActive(!received);
            }
            else
            {
                stepView.ClaimButton.SetActive(false);
            }

            if (step.BigPrefab)
            {
                stepView.StepWindowBig.ClaimedImage.SetActive(received);

                if (received)
                    Animator.AnimateRect(stepView.StepWindowBig.ClaimedImage.rectTransform, animationsSequence: new AnimationType[] 
                    { 
                        AnimationType.PunchScale,
                        AnimationType.ShakeScale
                    })?.AddTo(this);
            }
            else
            {
                stepView.StepWindow.ClaimedImage.SetActive(received);

                if (received)
                    Animator.AnimateRect(stepView.StepWindow.ClaimedImage.rectTransform, animationsSequence: new AnimationType[]
                    {
                        AnimationType.PunchScale,
                        AnimationType.ShakeScale
                    })?.AddTo(this);
            }

            MenuColorName colorName = step.IsReached ? _reachedColor : _notReachedColor;
            Color color = _spritesRewards.GetMenuColor(colorName);
            stepView.StepWindow.TitleImage.color = color;
            stepView.StepWindowBig.TitleImage.color = color;
        }

        public void AddProgressStep(int goalCupsAmount, ProgressStep step, UnityAction claimButtonAction)
        {
            GameObject stepGo = Instantiate(ProgressStepPrefab, _progressStepsContent.transform, false);
            ProgressStepView stepView = stepGo.GetComponent<ProgressStepView>();
            _progressSteps.Add(stepView);

            stepView.GoalCupsAmount = goalCupsAmount;
            stepView.IsLast = step.IsLast;

            stepView.ClaimButton.SetActive(step.IsReached);
            stepView.ClaimButton.onClick.RemoveAllListeners();
            stepView.ClaimButton.onClick.AddListener(claimButtonAction);
            stepView.ClaimButton.onClick.AddListener(() => UpdateStepStatus(step, stepView));
            stepView.ClaimButton.onClick.AddListener(() => OnButtonPressedMethod(stepView.ClaimButton));


            if (step.BigPrefab)
                SetBigPrefab(goalCupsAmount, stepView, step);
            else
                SetPrefab(goalCupsAmount, stepView, step);

            UpdateStepStatus(step, stepView);
        }

        public ProgressStepView GetProgressStepView(int goalCupsAmount) => _progressSteps.Find(s => s.GoalCupsAmount == goalCupsAmount);

        public void ClearProgressSteps()
        {
            foreach (var step in _progressSteps)
                if(step)
                    Destroy(step.gameObject);

            _progressSteps.Clear();
        }

        private void SetPrefab(int goalCupsAmount, ProgressStepView stepView, ProgressStep step)
        {
            stepView.StepWindow.SetActive(true);
            stepView.StepWindowBig.SetActive(false);
            stepView.ConnectionLineImage.SetActive(!step.IsLast);

            var window = stepView.StepWindow;
            window.GoalCupsAmount.text = goalCupsAmount.ToString();
            
            foreach (var reward in step.Rewards)
            {
                switch (reward.Type)
                {
                    case GameUnitType.Money:
                        Money money = (Money)reward;
                        window.RewardCards.SetActive(false);
                        window.RewardLootbox.SetActive(false);
                        window.RewardSimple.SetActive(true);
                        window.RewardSimple.RewardAmountText.text = money.MoneyAmount.ToString();
                        window.RewardSimple.RewardImage.sprite = _spritesRewards.GetSimpleRewardSprite(money.Type);
                        break;
                    case GameUnitType.Gems:
                        Gems gems = (Gems)reward;
                        window.RewardCards.SetActive(false);
                        window.RewardLootbox.SetActive(false);
                        window.RewardSimple.SetActive(true);
                        window.RewardSimple.RewardAmountText.text = gems.GemsAmount.ToString();
                        window.RewardSimple.RewardImage.sprite = _spritesRewards.GetSimpleRewardSprite(gems.Type);
                        break;
                    case GameUnitType.Lootbox:
                        LootboxReward lootbox = (LootboxReward)reward;
                        window.RewardCards.SetActive(false);
                        window.RewardSimple.SetActive(false);
                        window.RewardLootbox.SetActive(true);
                        window.RewardLootbox.LootboxImage.sprite = _spritesRewards.GetLootboxSprite(lootbox.Rarity);
                        break;
                    case GameUnitType.CarCard:
                        CarCardReward card = (CarCardReward)reward;
                        window.RewardSimple.SetActive(false);
                        window.RewardLootbox.SetActive(false);
                        window.RewardCards.SetActive(true);
                        window.RewardCards.CarImage.sprite = _spritesCars.GetCarSprite(card.CarName);
                        window.RewardCards.CardAmountText.text = card.CardsAmount.ToString();
                        break;
                    case GameUnitType.RaceReward:
                    case GameUnitType.Cups:
                    case GameUnitType.RaceMap:
                    case GameUnitType.IncomeBonus:
                        window.SetActive(false);
                        Debug.LogError($"Incorrect ProgressStep.Rewards settings in Scheme! Goal Cups Amount: {goalCupsAmount}");
                        break;
                }
            }
        }

        private void SetBigPrefab(int goalCupsAmount, ProgressStepView stepView, ProgressStep step)
        {
            stepView.StepWindow.SetActive(false);
            stepView.StepWindowBig.SetActive(true);
            stepView.ConnectionLineImage.SetActive(!step.IsLast);

            var bigWindow = stepView.StepWindowBig;
            bigWindow.GoalCupsAmount.text = goalCupsAmount.ToString();
            bigWindow.IncomeBonusWindow.SetActive(false);

            foreach (var reward in step.Rewards)
            {
                switch (reward.Type)
                {
                    case GameUnitType.Money:
                        Money money = (Money)reward;
                        bigWindow.RewardSimple.RewardAmountText.text = money.MoneyAmount.ToString();
                        bigWindow.RewardSimple.RewardImage.sprite = _spritesRewards.GetSimpleRewardSprite(money.Type);
                        break;
                    case GameUnitType.RaceMap:
                        RaceMap raceMap = (RaceMap)reward;
                        bigWindow.LevelName.text = raceMap.LevelName.ToString();
                        bigWindow.LevelRepresentationImage.sprite = _spritesRewards.GetLevelSprite(raceMap.LevelName);
                        break;
                    case GameUnitType.IncomeBonus:
                        IncomeBonus incomeBonus = (IncomeBonus)reward;
                        string t = incomeBonus.BonusValue.ToString();
                        bigWindow.IncomeBonusText.text = $"+ {t}%";
                        bigWindow.IncomeBonusWindow.SetActive(true);
                        break;
                    case GameUnitType.Gems:
                        Gems gems = (Gems)reward;
                        bigWindow.RewardSimple.RewardAmountText.text = gems.GemsAmount.ToString();
                        bigWindow.RewardSimple.RewardImage.sprite = _spritesRewards.GetSimpleRewardSprite(gems.Type);
                        break;
                    case GameUnitType.RaceReward:
                    case GameUnitType.Cups:
                    case GameUnitType.Lootbox:
                    case GameUnitType.CarCard:
                        bigWindow.SetActive(false);
                        Debug.LogError($"Incorrect ProgressStep.Rewards settings in Scheme! Goal Cups Amount: {goalCupsAmount}");
                        break;
                }
            }
        }
    }
}