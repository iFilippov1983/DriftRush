using System.Threading.Tasks;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    [SerializeField] private Animation _animation;
    [SerializeField] private AnimationClip _clipAppear;
    [SerializeField] private AnimationClip _clipDisappear;
    [SerializeField] private AnimationClip _clipIdle;
    [SerializeField] private GameObject _pointer;


    private Transform _transform;

    private void Start()
    {
        _transform = transform;
        _pointer.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Appear();
        }

        if (Input.GetMouseButtonUp(0))
        {
            Disappear();
        }

        if (gameObject.activeSelf)
        {
            _transform.position = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            _animation.Play(_clipIdle.name);
        }
            
    }

    private void Appear()
    {
        _pointer.gameObject.SetActive(true);
        _animation.Play(_clipAppear.name);
    }

    private async void Disappear()
    {
        _animation.Play(_clipDisappear.name);
        while(_animation.isPlaying)
            await Task.Yield();
        _pointer.gameObject.SetActive(false);
    }
}
