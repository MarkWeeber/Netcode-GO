using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LeaderBoardEntityDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Color myColor;
    public ulong ClientId;
    public FixedString32Bytes PlayerName;
    public int Coins;

    public void Initialize(ulong clientId, FixedString32Bytes playerName, int coins)
    {
        ClientId = clientId;
        PlayerName = playerName;
        Coins = coins;
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            displayText.color = myColor;
        }
        UpdateCoins(coins);
    }

    public void UpdateText()
    {
        displayText.text = $"#{transform.GetSiblingIndex() + 1}. {PlayerName} ({Coins})";
    }

    public void UpdateCoins(int coins)
    {
        Coins = coins;
        UpdateText();
    }
}
