using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ClientSignleton : MonoBehaviour
{
    private static ClientSignleton instance;
    public static ClientSignleton Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            else
            {
                instance = FindObjectOfType<ClientSignleton>();
                if (instance == null)
                {
                    Debug.LogError("No ClientSignleton found!");
                    return null;
                }
                return instance;
            }
        }
    }

    public ClientGameManager ClientGameManager { get; private set; }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    public async Task<bool> CreateClient()
    {
        ClientGameManager = new ClientGameManager();
        return await ClientGameManager.InitAsync();
    }

    private void OnDestroy()
    {
        ClientGameManager?.Dispose();
    }
}
