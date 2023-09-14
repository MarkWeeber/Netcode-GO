using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInstance : NetworkBehaviour
{
    [SerializeField] private TMP_Text userOverheadName;
    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
    private UserData userData;

    public override void OnNetworkSpawn()
    {
        OnPlayerNameChanged(string.Empty, PlayerName.Value);
        if (IsServer)
        {
            userData = HostSingleton.Instance.HostGameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            PlayerName.Value = userData.userName;
            userOverheadName.text = PlayerName.Value.ToString();
            PlayerName.OnValueChanged += OnPlayerNameChanged;
        }
        if (IsOwner)
        {

        }
    }
    public override void OnNetworkDespawn()
    {
        PlayerName.OnValueChanged -= OnPlayerNameChanged;
    }

    private void OnPlayerNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        userOverheadName.text = newValue.ToString();
    }


}
