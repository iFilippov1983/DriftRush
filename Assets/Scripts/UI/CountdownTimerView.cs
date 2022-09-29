using UnityEngine;
using TMPro;

public class CountdownTimerView : MonoBehaviour
{
    [SerializeField] private TMP_Text _countdownText;
    private Animation _animation;

    private void Awake()
    {
        _animation = GetComponent<Animation>();
    }

    public void Show(int seconds)
    { 
        _countdownText.text = seconds.ToString();
        if (_animation != null)
            _animation.Play();
    }
}
