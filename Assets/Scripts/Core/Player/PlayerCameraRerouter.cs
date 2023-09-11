using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraRerouter : NetworkBehaviour
{
    [SerializeField] private string cameraFollowerGameObjectName = "";
    private GameObject cameraFollowerObject;
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            cameraFollowerObject = GameObject.Find(cameraFollowerGameObjectName);
            cameraFollowerObject.transform.position = transform.position;
            cameraFollowerObject.transform.parent = transform;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            cameraFollowerObject.transform.parent = null;
        }
    }
}
