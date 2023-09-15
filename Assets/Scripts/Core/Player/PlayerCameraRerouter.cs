using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public class PlayerCameraRerouter : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private PlayerInstance playerInstance;
    private static PlayerCameraRerouter instance;
    public static PlayerCameraRerouter Instance { get { return instance; } }


    private void Start()
    {
        instance = this;
        PlayerInstance [] playerInstances = FindObjectsOfType<PlayerInstance>();
        foreach (PlayerInstance item in playerInstances)
        {
            if (item.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
            {
                if (networkObject.IsOwner)
                {
                    BindCamera(item);
                    return;
                }
            }
        }
    }

    public void BindCamera(PlayerInstance player)
    {
        playerInstance = player;
        virtualCamera.LookAt = player.transform;
        virtualCamera.Follow = player.transform;
    }

    public void UnBindCamera(PlayerInstance player)
    {
        if (playerInstance == player)
        {
            virtualCamera.LookAt = null;
            virtualCamera.Follow = null;
        }
    }
}
