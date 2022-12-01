using RaceManager.Tools;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RaceManager.UI
{
    public class ImageAnimator : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed;
        [SerializeField] private AnimationCurve _timeCurve;
        [SerializeField] private AnimationCurve[] _trajectoryCurves;
        [Space]
        [SerializeField] private float _endScaleSpeed;
        [SerializeField] private AnimationCurve _scaleCurve;
        [Space]
        [SerializeField] private float _resizeSpeed;
        [SerializeField] private AnimationCurve _resizeCurve;
        [SerializeField] private Vector3 _endImageSize;
        [Space]
        [SerializeField] private Vector3 _trailSpawnOffset = new Vector3(0f, 0f, 0.5f); 
        
        private GameObject _imageTrailPrefab;

        public Vector3 TrailSpawnOffset => _trailSpawnOffset;

        public GameObject ImageTrailPrefab
        {
            get 
            {
                if (_imageTrailPrefab == null)
                    ResourcesLoader.LoadPrefab(ResourcePath.ImageAnimationTrailPrefab);
                return _imageTrailPrefab;
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        public static event Action ImageReachedTarget;

        public void Play(GameObject objectToAnimate, GameObject target, Action animationFinishCallBack)
        {
            StartCoroutine(StartAnimation(objectToAnimate, target, animationFinishCallBack));
        }

        private IEnumerator StartAnimation(GameObject imageObject, GameObject target, Action animationFinishCallBack)
        {
            imageObject.SetActive(true);

            var endPosition = target.transform.position;

            ImageReachedTarget += OnLetterReachedTarget;

            void OnLetterReachedTarget()
            {
                StartCoroutine(ScaleLetterToZero(imageObject));
                animationFinishCallBack?.Invoke();
                ImageReachedTarget -= OnLetterReachedTarget;
            }

            StartCoroutine(Animate(imageObject.transform, endPosition));

            yield return null;
        }

        private IEnumerator ScaleLetterToZero(GameObject imageObject)
        {
            yield return ScaleToZero(imageObject.transform, _endScaleSpeed, _scaleCurve);

            Destroy(imageObject.gameObject);
        }

        private IEnumerator Animate(Transform imageObject, Vector3 endPosition)
        {
            StartCoroutine(Rescale(imageObject, _endImageSize, _resizeSpeed, _resizeCurve));

            yield return MoveToPosition
                (
                imageObject,
                endPosition,
                _moveSpeed, 
                _timeCurve,
                _trajectoryCurves[Random.Range(0, _trajectoryCurves.Length)]
                );

            ImageReachedTarget?.Invoke();
        }

        private static IEnumerator MoveToPosition
            (
            Transform transformToMove,
            Vector3 endPosition,
            float speed,
            AnimationCurve timeCurve,
            AnimationCurve trajectoryCurve = null
            )
        {
            var startPosition = transformToMove.position;
            var travelPercent = 0f;

            while (travelPercent <= 1f)
            {
                Vector3 posVector = Vector3.Lerp(startPosition,
                                               endPosition,
                                               timeCurve.Evaluate(travelPercent));
                if (trajectoryCurve != null)
                {
                    posVector.x += trajectoryCurve.Evaluate(travelPercent);
                }


                transformToMove.position = posVector;

                travelPercent += speed * Time.deltaTime;

                yield return null;
            }

            transformToMove.position = endPosition;
        }

        private static IEnumerator Rescale(Transform target, Vector3 targetScale, float speed, AnimationCurve curve)
        {
            var percent = 0f;
            var startScale = target.localScale;

            while (percent < 1f)
            {
                percent += speed * Time.deltaTime;
                target.localScale = Vector3.Lerp(startScale, targetScale, curve.Evaluate(percent));
                yield return null;
            }
        }

        private static IEnumerator ScaleToZero(Transform letter, float speed, AnimationCurve curve)
        {
            var percent = 0f;
            var startScale = letter.localScale;

            while (percent <= 1f)
            {
                percent += speed * Time.deltaTime;
                letter.localScale = startScale * curve.Evaluate(percent);
                yield return null;
            }
        }
    }
}
