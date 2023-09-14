using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JoinCoideExposer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    
    private void Awake()
    {
        if (HostSingleton.Instance != null)
        {
            text.text = HostSingleton.Instance.HostGameManager.JoinCode;
        }
    }
}
