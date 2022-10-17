using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))] 
public class CinemachineFollowCamView : MonoBehaviour
{
    private CinemachineVirtualCamera _camera;

    public CinemachineVirtualCamera Camera => GetComponent<CinemachineVirtualCamera>();

    private void Awake()
    {
        _camera = GetComponent<CinemachineVirtualCamera>();
    }
}
