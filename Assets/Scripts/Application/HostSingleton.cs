using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    private static HostSingleton instance;
    public static HostSingleton Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            else
            {
                instance = FindObjectOfType<HostSingleton>();
                if (instance == null)
                {
                    Debug.LogError("No HostSignleton found!");
                    return null;
                }
                return instance;
            }
        }
    }

    public HostGameManager HostGameManager { get; private set; }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    public void CreateHost()
    {
        HostGameManager = new HostGameManager();
    }
}
