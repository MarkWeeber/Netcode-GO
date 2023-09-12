using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestCall : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private void Start()
    {
        if (HostSingleton.Instance != null)
        {
            text.text = HostSingleton.Instance.HostGameManager.JoinCode;
        }
    }
}
