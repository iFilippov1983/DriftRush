using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class RespawnCarButtonView : MonoBehaviour
    {
        [SerializeField] private Button _respawnButton;

        public void AddListener(UnityAction unityAction) => _respawnButton.onClick.AddListener(unityAction);
    }
}
