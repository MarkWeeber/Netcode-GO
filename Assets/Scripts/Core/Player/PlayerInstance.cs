using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInstance : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer minimapSprite;
    [SerializeField] private Color ownColor;
    [SerializeField] private TMP_Text userOverheadName;
    [field: SerializeField] public Health Health { get; private set; }
    [field: SerializeField] public Inventory Inventory { get; private set; }
    public static UnityEvent<PlayerInstance> OnPlayerSpawned = new UnityEvent<PlayerInstance>();
    public static UnityEvent<PlayerInstance> OnPlayerDespawned = new UnityEvent<PlayerInstance>();
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
            OnPlayerSpawned?.Invoke(this);
        }
        if (IsOwner)
        {
            if (PlayerCameraRerouter.Instance != null)
            {
                PlayerCameraRerouter.Instance.BindCamera(this);
            }
            minimapSprite.color = ownColor;
        }
    }
    public override void OnNetworkDespawn()
    {
        PlayerName.OnValueChanged -= OnPlayerNameChanged;
        if (IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
        else
        {
        }
        if (IsOwner)
        {
            if (PlayerCameraRerouter.Instance != null)
            {
                PlayerCameraRerouter.Instance.UnBindCamera(this);
            }
        }
    }

    private void OnPlayerNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        userOverheadName.text = newValue.ToString();
    }
}
