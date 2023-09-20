using Unity;
using Unity.Netcode;
using UnityEngine;

public class TestCallNetwork : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log("On Network Spawn");
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log("On Network Despawn");
    }
}
