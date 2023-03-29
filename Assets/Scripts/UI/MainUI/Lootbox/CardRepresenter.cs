using DG.Tweening;
using RaceManager.Progress;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace RaceManager.UI
{
    public class CardRepresenter
    {
        private readonly RepresentationCard _representationCard;
        private readonly UIAnimator _animator;

        public CardRepresenter(RepresentationCard representationCard, UIAnimator animator)
        {
            _representationCard = representationCard;
            _animator = animator;
        }

        public async Task<bool> Represent(CarCardView cardView, CancellationTokenSource tokenSource = null)
        {
            _representationCard.SetActive(true);
            _representationCard.ReplaceableRect.SetActive(true);
            _representationCard.CardRect.SetActive(true);
            _representationCard.IsVisible = true;
            _representationCard.IsAppearing = true;

            SetRepresentationCard(cardView);

            Tween appearTween = _representationCard.CardRect.DOScale(0f, _representationCard.Settings.appearAnimDuration).From();
            appearTween.OnComplete(() => 
            { 
                _representationCard.IsAppearing = false;
            });

            while (_representationCard.IsAppearing)
            { 
                tokenSource?.Token.ThrowIfCancellationRequested();
                await Task.Yield();

                if (Input.GetMouseButtonDown(0))
                {
                    appearTween.Complete(true);
                    appearTween = null;
                    return true;
                }
            }

            Sequence replaceSequence = DOTween.Sequence();
            Sequence extraSequence = DOTween.Sequence();

            Vector3 initialPos;
            Vector3 targetPos;

            if (_representationCard.IsReplaceable)
            {
                _representationCard.IsReplacing = true;
                Vector3 initialScale = _representationCard.ReplaceableRect.localScale;
                initialPos = _representationCard.MaxCardsText.transform.position;
                targetPos = _representationCard.DisappearTargetRectUp.transform.position;

                _representationCard.MaxCardsText.SetActive(true);
                replaceSequence.Append(_representationCard.MaxCardsText.rectTransform.DOScale(0f, _representationCard.Settings.appearAnimDuration).From());
                replaceSequence.Append(_representationCard.MaxCardsText.rectTransform.DOPunchScale
                    (_representationCard.MaxCardsText.rectTransform.localScale *_representationCard.Settings.maxScale, 
                    _representationCard.Settings.stateAnimDuration, 5));
                replaceSequence.AppendInterval(_representationCard.Settings.animPauseDuration);
                replaceSequence.Append(_representationCard.ReplaceableRect.DOScale(0f, _representationCard.Settings.disappearAnimDuration));
                replaceSequence.Join(_representationCard.MaxCardsText.transform.DOMove(targetPos, _representationCard.Settings.disappearAnimDuration));
                replaceSequence.AppendCallback(() => 
                {
                    _representationCard.MaxCardsText.rectTransform.localScale = initialScale;
                    _representationCard.MaxCardsText.transform.position = initialPos;

                    _representationCard.ReplaceableRect.localScale = initialScale;
                    _representationCard.ReplaceableRect.SetActive(false);

                    ActivateAlternativeView(_representationCard.ReplacementInfo);
                    extraSequence = DOTween.Sequence();
                    extraSequence.Append(_representationCard.AlternativeAmountText.transform.DOScale(0f, _representationCard.Settings.appearAnimDuration).From());
                    extraSequence.AppendCallback(() => _representationCard.AlternativeAmountText.transform.localScale = initialScale);

                    _representationCard.IsReplacing = false;
                });

                replaceSequence.Play();
            }

            while (_representationCard.IsReplacing)
            {
                tokenSource?.Token.ThrowIfCancellationRequested();
                await Task.Yield();

                if (Input.GetMouseButtonDown(0))
                {
                    replaceSequence?.Complete(true);
                    replaceSequence = null;

                    extraSequence?.Complete(true);
                    extraSequence = null;

                    return true;
                }
            }

            while (!Input.GetMouseButtonDown(0))
            {
                tokenSource?.Token.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            Sequence disappearSequence = DOTween.Sequence();

            initialPos = _representationCard.CardRect.transform.position;
            targetPos = _representationCard.DisappearTargetRectLeft.transform.position;

            disappearSequence.Append(_representationCard.CardRect.DOMove(targetPos, _representationCard.Settings.disappearAnimDuration));
            disappearSequence.AppendCallback(() =>
            {
                _representationCard.CardRect.transform.position = initialPos;
                _representationCard.IsVisible = false;
            });

            while (_representationCard.IsVisible)
            {
                tokenSource?.Token.ThrowIfCancellationRequested();
                await Task.Yield();

                if (Input.GetMouseButtonDown(0))
                {
                    disappearSequence.Complete(true);
                    disappearSequence = null;
                    return true;
                }
            }

            return true;
        }

        private void SetRepresentationCard(CarCardView cardView)
        {
            string name = cardView.CarName.ToString().SplitByUppercaseWith(" ");
            name = name.Replace('_', ' ');
            _representationCard.CarNameText.SetActive(true);
            _representationCard.CarNameText.text = name.ToUpper();

            _representationCard.FrameImage.SetActive(true);
            _representationCard.FrameImage.color = cardView.FrameImage.color;

            _representationCard.CardAmountText.SetActive(true);
            _representationCard.CardAmountText.text = cardView.CardsAmountText.text;

            _representationCard.CarImage.SetActive(true);
            _representationCard.CarImage.sprite = cardView.CardCarImage.sprite;

            _representationCard.AlternativeAmountText.SetActive(false);

            _representationCard.IsReplaceable = false;
            _representationCard.ReplacementInfo = null;

            if (cardView.ReplacementInfo is null)
            {
                return;
            }
            else if (cardView.CardsAmount > 0)
            { 
                _representationCard.IsReplaceable = true;
                _representationCard.ReplacementInfo = cardView.ReplacementInfo;
            }
            else
            {
                ActivateAlternativeView(cardView.ReplacementInfo);
            }
        }

        private void ActivateAlternativeView(UnitReplacementInfo? info)
        {
            _representationCard.ReplacementInfo = info;

            _representationCard.CarNameText.SetActive(false);

            _representationCard.FrameImage.SetActive(false);

            _representationCard.CardAmountText.SetActive(false);

            _representationCard.CarImage.SetActive(false);

            _representationCard.MaxCardsText.SetActive(false);

            _representationCard.AlternativeAmountText.SetActive(true);
            _representationCard.AlternativeAmountText.text = info.Value.Amount.ToString();

            _animator.SpawnGroupOn(_representationCard.ReplacementInfo.Value.Type, _representationCard.AlternativeAmountText.transform, _representationCard.transform);
        }
    }
}

