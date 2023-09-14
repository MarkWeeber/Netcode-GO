using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraRerouter : MonoBehaviour
{
    private NetworkObject networkObject;
    private PlayerMovement playerMovement;
    public void Start()
    {
        BindCamera();
    }

    private void OnDestroy()
    {
        if (playerMovement != null)
        {
            playerMovement.OnDespawn.RemoveListener(UnBindCamera);
        }
    }

    private void BindCamera()
    {
        var playerObjects = FindObjectsOfType<PlayerMovement>();
        foreach (var item in playerObjects)
        {
            if (item.TryGetComponent<NetworkObject>(out networkObject))
            {
                if (networkObject.IsOwner)
                {
                    transform.position = item.transform.position;
                    transform.parent = item.transform;
                    playerMovement = item;
                    item.OnDespawn.AddListener(UnBindCamera);
                    return;
                }
            }
        }
    }

    private void UnBindCamera()
    {
        transform.parent = null;
    }
}
