using RaceManager.Progress;
using RaceManager.Root;
using RaceManager.Tools;
using Sirenix.Utilities;
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
    public class GameProgressPanel : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [Space]
        [SerializeField] private TMP_Text _moneyAmountText;
        [SerializeField] private TMP_Text _gemsAmountText;
        [Space]
        [SerializeField] private GridLayoutGroup _progressStepsContent;
        [SerializeField] private RectTransform _progressStepsContentRect;

        private GameObject _progressStepPrefab;
        private SpritesContainerCarCards _spritesCards;
        private SpritesContainerCarCollection _spritesCars;
        private SpritesContainerRewards _spritesRewards;

        private List<ProgressStepView> _progressSteps = new List<ProgressStepView>();

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
        private void Construct(SpritesContainerCarCards spritesContainerCards, SpritesContainerCarCollection spritesContainerCars, SpritesContainerRewards spritesContainerRewards)
        { 
            _spritesCards = spritesContainerCards;
            _spritesCars = spritesContainerCars;
            _spritesRewards = spritesContainerRewards;
        }

        public void SetCupsAmountSlider(int cupsAmount)
        {
            int previouseGoal = 0;

            foreach (var stepView in _progressSteps)
            {
                if (cupsAmount > stepView.GoalCupsAmount)
                {
                    stepView.CupsAmountSlider.SliderLevelImage.SetActive(false);
                    stepView.CupsAmountSlider.SliderImage.fillAmount = 1f;
                    previouseGoal = stepView.GoalCupsAmount;
                    continue;
                }
                
                if (stepView.GoalCupsAmount >= cupsAmount && cupsAmount > previouseGoal)
                {
                    stepView.CupsAmountSlider.CupsAmountText.text = cupsAmount.ToString();
                    stepView.CupsAmountSlider.SliderLevelImage.SetActive(true);

                    int stepSize = stepView.GoalCupsAmount - previouseGoal;
                    int localLevel = cupsAmount - previouseGoal;
                    float fillAmount = (float)localLevel / (float)stepSize;
                    stepView.CupsAmountSlider.SliderImage.fillAmount = fillAmount;

                    Vector3 localPos = stepView.CupsAmountSlider.SliderLevelImage.rectTransform.localPosition;
                    localPos.y = stepView.CupsAmountSlider.SliderImage.rectTransform.rect.height * fillAmount;
                    stepView.CupsAmountSlider.SliderLevelImage.rectTransform.localPosition = localPos;

                    previouseGoal = stepView.GoalCupsAmount;
                    continue;
                }

                stepView.CupsAmountSlider.SliderLevelImage.SetActive(false);
                stepView.CupsAmountSlider.SliderImage.fillAmount = 0f;

                int indexCur = _progressSteps.IndexOf(stepView);
                if (indexCur - 1 >= 0)
                {
                    var stepPrev = _progressSteps[indexCur - 1];
                    if (stepPrev.GoalCupsAmount == cupsAmount)
                    {
                        ActivateLevelImageAndPlaceToZero(stepView, cupsAmount);
                    }
                }

                if (cupsAmount == 0 && indexCur == 0)
                {
                    ActivateLevelImageAndPlaceToZero(stepView, cupsAmount);
                }
            }
        }

        private void ActivateLevelImageAndPlaceToZero(ProgressStepView stepView, int cupsAmount)
        {
            stepView.CupsAmountSlider.SliderLevelImage.SetActive(true);
            stepView.CupsAmountSlider.CupsAmountText.text = cupsAmount.ToString();

            Vector3 localPos = stepView.CupsAmountSlider.SliderLevelImage.rectTransform.localPosition;
            localPos.y = 0;
            stepView.CupsAmountSlider.SliderLevelImage.rectTransform.localPosition = localPos;
        }

        private void UpdateStepStatus(ProgressStep step, ProgressStepView stepView)
        {
            if (step.IsReached)
            {
                stepView.ClaimButton.SetActive(true);
                stepView.ClaimButton.SetActive(!step.RewardsReceived);
            }
            else
            {
                stepView.ClaimButton.SetActive(false);
            }

            if (step.BigPrefab)
            {
                stepView.StepWindowBig.ClaimedImage.SetActive(step.RewardsReceived);
            }
            else
            {
                stepView.StepWindow.ClaimedImage.SetActive(step.RewardsReceived);
            }
        }

        public void AddProgressStep(int goalCupsAmount, ProgressStep step, UnityAction claimButtonAction)
        {
            GameObject stepGo = Instantiate(ProgressStepPrefab, _progressStepsContent.transform, false);
            ProgressStepView stepView = stepGo.GetComponent<ProgressStepView>();
            _progressSteps.Add(stepView);

            stepView.GoalCupsAmount = goalCupsAmount;

            stepView.ClaimButton.SetActive(step.IsReached);
            stepView.ClaimButton.onClick.AddListener(claimButtonAction);
            stepView.ClaimButton.onClick.AddListener(() => UpdateStepStatus(step, stepView));
            

            if (step.BigPrefab)
                SetBigPrefab(goalCupsAmount, stepView, step);
            else
                SetPrefab(goalCupsAmount, stepView, step);

            UpdateStepStatus(step, stepView);
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
                    case RewardType.Money:
                        Money money = (Money)reward;
                        window.RewardCards.SetActive(false);
                        window.RewardLootbox.SetActive(false);
                        window.RewardSimple.SetActive(true);
                        window.RewardSimple.RewardAmountText.text = money.MoneyAmount.ToString();
                        window.RewardSimple.RewardImage.sprite = _spritesRewards.GetSimpleRewardSprite(money.Type);
                        break;
                    case RewardType.Gems:
                        Gems gems = (Gems)reward;
                        window.RewardCards.SetActive(false);
                        window.RewardLootbox.SetActive(false);
                        window.RewardSimple.SetActive(true);
                        window.RewardSimple.RewardAmountText.text = gems.GemsAmount.ToString();
                        window.RewardSimple.RewardImage.sprite = _spritesRewards.GetSimpleRewardSprite(gems.Type);
                        break;
                    case RewardType.Lootbox:
                        LootboxReward lootbox = (LootboxReward)reward;
                        window.RewardCards.SetActive(false);
                        window.RewardSimple.SetActive(false);
                        window.RewardLootbox.SetActive(true);
                        window.RewardLootbox.LootboxImage.sprite = _spritesRewards.GetLootboxSprite(lootbox.Rarity);
                        break;
                    case RewardType.CarCard:
                        CarCard card = (CarCard)reward;
                        window.RewardSimple.SetActive(false);
                        window.RewardLootbox.SetActive(false);
                        window.RewardCards.SetActive(true);
                        window.RewardCards.CarImage.sprite = _spritesCars.GetCarSprite(card.CarName);
                        window.RewardCards.CardsRarityImage.sprite = _spritesCards.GetCardSprite(card.CarName);
                        window.RewardCards.CarName.text = card.CarName.ToString();
                        window.RewardCards.CardAmountText.text = card.CardsAmount.ToString();
                        break;
                    case RewardType.RaceReward:
                    case RewardType.Cups:
                    case RewardType.RaceMap:
                    case RewardType.IncomeBonus:
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

            foreach (var reward in step.Rewards)
            {
                switch (reward.Type)
                {
                    case RewardType.Money:
                        Money money = (Money)reward;
                        bigWindow.RewardSimple.RewardAmountText.text = money.MoneyAmount.ToString();
                        bigWindow.RewardSimple.RewardImage.sprite = _spritesRewards.GetSimpleRewardSprite(money.Type);
                        break;
                    case RewardType.RaceMap:
                        RaceMap raceMap = (RaceMap)reward;
                        bigWindow.LevelName.text = raceMap.LevelName.ToString();
                        bigWindow.LevelRepresentationImage.sprite = _spritesRewards.GetLevelSprite(raceMap.LevelName);
                        break;
                    case RewardType.IncomeBonus:
                        IncomeBonus incomeBonus = (IncomeBonus)reward;
                        string t = incomeBonus.BonusValue.ToString();
                        bigWindow.IncomeBonusText.text = $"+ {t}%";
                        break;
                    case RewardType.Gems:
                        Gems gems = (Gems)reward;
                        bigWindow.RewardSimple.RewardAmountText.text = gems.GemsAmount.ToString();
                        bigWindow.RewardSimple.RewardImage.sprite = _spritesRewards.GetSimpleRewardSprite(gems.Type);
                        break;
                    case RewardType.RaceReward:
                    case RewardType.Cups:
                    case RewardType.Lootbox:
                    case RewardType.CarCard:
                        bigWindow.SetActive(false);
                        Debug.LogError($"Incorrect ProgressStep.Rewards settings in Scheme! Goal Cups Amount: {goalCupsAmount}");
                        break;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var stepView in _progressSteps)
            {
                stepView.ClaimButton.onClick.RemoveAllListeners();
            }
        }
    }
}