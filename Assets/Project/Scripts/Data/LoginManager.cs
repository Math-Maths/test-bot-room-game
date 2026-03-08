using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public class LoginManager : MonoBehaviour
{
    private string m_GooglePlayGamesToken;

    private async void Awake()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        LoginGooglePlayGames();
#endif

        if(UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
        }
    }

    private void OnEnable()
    {
        EventManager.Instance.AddListener(EventNameSaver.OnSignInWithGooglePlayGames, StartSignInWithGooglePlayGames);
        EventManager.Instance.AddListener(EventNameSaver.OnSignInAnonymously, StartAnonymousSignIn);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener(EventNameSaver.OnSignInWithGooglePlayGames, StartSignInWithGooglePlayGames);
        EventManager.Instance.RemoveListener(EventNameSaver.OnSignInAnonymously, StartAnonymousSignIn);
    }

#if UNITY_ANDROID
    private void LoginGooglePlayGames()
    {
        PlayGamesPlatform.Instance.Authenticate((status) =>
        {
            if(status == SignInStatus.Success)
            {
                Debug.Log("Google Play Games authentication successful.");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    Debug.Log("Authorization code: " + code);
                    m_GooglePlayGamesToken = code;
                });
            }
            else
            {
                Debug.LogError("Google Play Games authentication failed: " + status);
            }
            
        });
    }

    public void StartSignInWithGooglePlayGames()
    {
        if(!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            Debug.LogWarning("Not yet authenticated with Google Play Games. Please wait...");
            LoginGooglePlayGames();
            return;
        }

        SignInOrLinkWithGooglePlayGames();
    }

    private async void SignInOrLinkWithGooglePlayGames()
    {
        if(string.IsNullOrEmpty(m_GooglePlayGamesToken))
        {
            Debug.LogError("Google Play Games token is not available. Please authenticate first.");
            return;
        }

        if(!AuthenticationService.Instance.IsSignedIn)
        {
            await SignInWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
        }
        else
        {
            await LinkWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
        }
    }

    private async Task SignInWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);
            Debug.Log("Successfully signed in with Google Play Games.");
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError("Failed to sign in with Google Play Games: " + ex.Message);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError("Request failed during Google Play Games sign-in: " + ex.Message);
        }
    }

    private async Task LinkWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(authCode);
            Debug.Log("Successfully linked Google Play Games account.");
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            Debug.LogWarning("This user is already linked to another account. Attempting to sign in instead.");
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError("Failed to link Google Play Games account: " + ex.Message);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError("Request failed during Google Play Games linking: " + ex.Message);
        }
    }
#endif

    public async void StartAnonymousSignIn()
    {
        await SignUpAnonymouslyAsync();
    }

    private async Task SignUpAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
         }
    }


}