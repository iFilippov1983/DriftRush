using System;
using UnityEngine;

namespace RaceManager.UI
{
    public class ImageAnimationHandler
    { 
        private ImageAnimator _imageAnimator;
        private GameObject _imageToAnimate;

        public event Action OnAnimationInitialize;
        public event Action OnAnimationFinish;

        public ImageAnimationHandler(ImageAnimator imageAnimator, GameObject imageToAnimate)
        {
            _imageAnimator = imageAnimator;
            _imageToAnimate = imageToAnimate;
        }

        public void InitializeAnimationWithTarget(GameObject target)
        {
            OnAnimationInitialize?.Invoke();
            var image = MakeCopyOf(_imageToAnimate);
            _imageAnimator.Play(image, target, () => OnAnimationFinish?.Invoke());
        }

        private GameObject MakeCopyOf(GameObject objectToCopy)
        {
            var imageObj = GameObject.Instantiate(objectToCopy, objectToCopy.transform.position, Quaternion.identity);
            Vector3 trailSpawnPos = objectToCopy.transform.position + _imageAnimator.TrailSpawnOffset;

            if (_imageAnimator.ImageTrailPrefab != null)
            {
                GameObject.Instantiate(_imageAnimator.ImageTrailPrefab, trailSpawnPos, Quaternion.identity, imageObj.transform);
            }
            
            imageObj.SetActive(true);
            return imageObj;
        }
    }
}

