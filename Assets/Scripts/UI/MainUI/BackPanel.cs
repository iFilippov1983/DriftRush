using RaceManager.Cars;
using RaceManager.Root;
using RaceManager.Shed;
using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class BackPanel : MonoBehaviour
    {
        public Action OnClick;

        private void OnMouseDown()
        {
            OnClick?.Invoke();
            Debug.Log("Back panel clicked 1");
        }

        private void OnMouseUpAsButton()
        {
            OnClick?.Invoke();
            Debug.Log("Back panel clicked 2");
        }

        
    }
}

