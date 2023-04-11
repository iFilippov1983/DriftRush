using TMPro;
using UnityEngine;

public class FpsPanelView : MonoBehaviour
{
    [SerializeField] private TMP_Text _averageText;
    [SerializeField] private TMP_Text _highestText;
    [SerializeField] private TMP_Text _lowestText;

    public TMP_Text AverageText => _averageText;
    public TMP_Text HighestText => _highestText;
    public TMP_Text LowestText => _lowestText;

}
