using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class ChestProgressPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _moreWinsText;
        [SerializeField] private GameObject _chestObject;
        [SerializeField] private Image[] _images = new Image[3];

        public TMP_Text MoreWinsTaxt => _moreWinsText;
        public GameObject ChestObject => _chestObject;
        public Image[] Images => _images;
    }
}

