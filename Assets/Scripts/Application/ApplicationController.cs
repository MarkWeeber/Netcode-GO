using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSignleton clientSingletonPrefab;
    [SerializeField] private HostSingleton hostSignletonPrefab;
    private ClientSignleton clientSingleton;
    private HostSingleton hostSignleton;
    private async void Start()
    {
        DontDestroyOnLoad(gameObject);

        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private async Task LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            
        }
        else
        {
            hostSignleton = Instantiate(hostSignletonPrefab);
            hostSignleton.CreateHost();

            clientSingleton = Instantiate(clientSingletonPrefab);
            bool authenticated = await clientSingleton.CreateClient();

            if (authenticated)
            {
                clientSingleton.ClientGameManager.GoToMenu();
            }
        }
    }
}
