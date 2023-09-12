using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class AuthenticationWrapper
{
    public static AuthenticationState AuthenticationState { get; private set; } = AuthenticationState.NotAuthenticated;
    public static async Task<AuthenticationState> DoAuthenticate(int maxTries = 5)
    {
        if (AuthenticationState == AuthenticationState.Authenticated)
        {
            return AuthenticationState;
        }
        if (AuthenticationState == AuthenticationState.Authenticating)
        {
            Debug.LogWarning("Already authenticating");
            await Authenticating();
            return AuthenticationState;
        }
        await SignInAnonymoislyAsync(maxTries);
        return AuthenticationState;
    }

    private static async Task<AuthenticationState> Authenticating()
    {
        while (AuthenticationState == AuthenticationState.Authenticating || AuthenticationState == AuthenticationState.NotAuthenticated)
        {
            await Task.Delay(200);
        }
        return AuthenticationState;
    }

    private static async Task SignInAnonymoislyAsync(int maxTries)
    {
        AuthenticationState = AuthenticationState.Authenticating;
        int tries = maxTries;
        while (tries > 0 && AuthenticationState == AuthenticationState.Authenticating)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                {
                    AuthenticationState = AuthenticationState.Authenticated;
                    break;
                }
            }
            catch (AuthenticationException authException)
            {
                Debug.LogError(authException);
                AuthenticationState = AuthenticationState.Error;
            }
            catch (RequestFailedException requestFailException)
            {
                Debug.LogError(requestFailException);
                AuthenticationState = AuthenticationState.Error;
            }
            tries--;
            await Task.Delay(1000);
        }
        if (AuthenticationState == AuthenticationState.Authenticating)
        {
            Debug.LogWarning($"Authentication time out after {maxTries} tries");
            AuthenticationState = AuthenticationState.Timeout;
        }
    }
}

public enum AuthenticationState
{
    NotAuthenticated = 0,
    Authenticating = 1,
    Authenticated = 2,
    Error = 3,
    Timeout = 4
}
