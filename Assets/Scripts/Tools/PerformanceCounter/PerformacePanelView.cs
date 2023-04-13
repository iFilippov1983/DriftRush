using TMPro;
using UnityEngine;

public class PerformacePanelView : MonoBehaviour
{
    [SerializeField] private TMP_Text _averageText;
    [SerializeField] private TMP_Text _highestText;
    [SerializeField] private TMP_Text _lowestText;
    [Space]
    [SerializeField] private TMP_Text _monoUsedText;
    [SerializeField] private TMP_Text _monoHeapText;

    public TMP_Text AverageText => _averageText;
    public TMP_Text HighestText => _highestText;
    public TMP_Text LowestText => _lowestText;
    public TMP_Text MonoUsedText => _monoUsedText;
    public TMP_Text MonoHeapText => _monoHeapText;
}
