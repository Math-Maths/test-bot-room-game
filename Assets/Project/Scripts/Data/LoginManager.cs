using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
using TestBotRoom;
using Unity.Services.Core;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public class LoginManager : MonoBehaviour
{
    public const string METHOD_GOOGLE = "GooglePlayGames";
    public const string METHOD_ANONYMOUS = "Anonymous";
    public const string LOGIN_KEY = "LastLoginMethod";

    private string m_GooglePlayGamesToken;
    private GameManager _gameManager;

    private void Awake()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
#endif
    }

    public void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    private void OnEnable()
    {
        EventManager.Instance.AddListener(EventNameSaver.OnSignInWithGooglePlayGames, StartSignInWithGooglePlayGames_FromEvent);
        EventManager.Instance.AddListener(EventNameSaver.OnSignInAnonymously, StartAnonymousSignIn_FromEvent);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener(EventNameSaver.OnSignInWithGooglePlayGames, StartSignInWithGooglePlayGames_FromEvent);
        EventManager.Instance.RemoveListener(EventNameSaver.OnSignInAnonymously, StartAnonymousSignIn_FromEvent);
    }

#if UNITY_ANDROID
    private Task<SignInStatus> AuthenticateGooglePlayGamesAsync()
    {
        var tcs = new TaskCompletionSource<SignInStatus>();

        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("Google Play Games authentication successful.");
            }
            else
            {
                Debug.LogError("Google Play Games authentication failed: " + status);
            }

            tcs.TrySetResult(status);
        });

        return tcs.Task;
    }

    private Task<string> GetGoogleTokenAsync()
    {
        var tcs = new TaskCompletionSource<string>();

        PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
        {
            if (string.IsNullOrEmpty(code))
            {
                Debug.LogError("Google did not return a valid authorization code.");
                tcs.SetResult(null);
            }
            else
            {
                tcs.SetResult(code);
            }
        });

        return tcs.Task;
    }

    private async Task<bool> EnsureGooglePlayGamesTokenAsync()
    {
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            Debug.LogWarning("Not authenticated in Google Play Games. Starting login...");
            SignInStatus signInStatus = await AuthenticateGooglePlayGamesAsync();
            if (signInStatus != SignInStatus.Success)
            {
                return false;
            }
        }

        if (string.IsNullOrEmpty(m_GooglePlayGamesToken))
        {
            Debug.Log("Google token is empty. Requesting a new token...");
            m_GooglePlayGamesToken = await GetGoogleTokenAsync();
        }

        if (string.IsNullOrEmpty(m_GooglePlayGamesToken))
        {
            Debug.LogError("Critical failure: unable to get Google Play Games token.");
            return false;
        }

        return true;
    }
#endif

    private void StartSignInWithGooglePlayGames_FromEvent()
    {
        _ = StartSignInWithGooglePlayGames_FromEventAsync();
    }

    private async Task StartSignInWithGooglePlayGames_FromEventAsync()
    {
        AuthResult authResult = await StartSignInWithGooglePlayGames();
        await _gameManager.HandleAuthenticationResult(authResult);
    }

    public async Task<AuthResult> StartSignInWithGooglePlayGames()
    {
#if UNITY_ANDROID
        if (!await EnsureGooglePlayGamesTokenAsync())
        {
            return AuthResult.Failed(METHOD_GOOGLE, "Unable to authenticate with Google Play Games.");
        }

        return await SignInOrLinkWithGooglePlayGames();
#else
        return AuthResult.Failed(METHOD_GOOGLE, "Google Play Games is available only on Android.");
#endif
    }

    public async Task<AuthResult> SignInOrLinkWithGooglePlayGames()
    {
#if UNITY_ANDROID
        if (!await EnsureGooglePlayGamesTokenAsync())
        {
            return AuthResult.Failed(METHOD_GOOGLE, "Google Play Games token is not available.");
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            return await SignInWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
        }

        return await LinkWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
#else
        return AuthResult.Failed(METHOD_GOOGLE, "Google Play Games is available only on Android.");
#endif
    }

    public async Task<AuthResult> SignInWithSavedMethodAsync()
    {
        string lastMethod = PlayerPrefs.GetString(LOGIN_KEY, METHOD_ANONYMOUS);

        if (lastMethod == METHOD_GOOGLE)
        {
            return await SignInOrLinkWithGooglePlayGames();
        }

        return await StartAnonymousSignIn();
    }

#if UNITY_ANDROID
    private async Task<AuthResult> SignInWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);
            Debug.Log("Successfully signed in with Google Play Games.");

            PlayerPrefs.SetString(LOGIN_KEY, METHOD_GOOGLE);
            PlayerPrefs.Save();

            return AuthResult.Succeeded(METHOD_GOOGLE);
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError("Failed to sign in with Google Play Games: " + ex.Message);
            return AuthResult.Failed(METHOD_GOOGLE, ex.Message);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError("Request failed during Google Play Games sign-in: " + ex.Message);
            return AuthResult.Failed(METHOD_GOOGLE, ex.Message);
        }
    }

    private async Task<AuthResult> LinkWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(authCode);
            Debug.Log("Successfully linked Google Play Games account.");

            PlayerPrefs.SetString(LOGIN_KEY, METHOD_GOOGLE);
            PlayerPrefs.Save();

            return AuthResult.Succeeded(METHOD_GOOGLE);
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            Debug.LogWarning("This user is already linked to another account.");
            return AuthResult.Failed(METHOD_GOOGLE, ex.Message);
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError("Failed to link Google Play Games account: " + ex.Message);
            return AuthResult.Failed(METHOD_GOOGLE, ex.Message);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError("Request failed during Google Play Games linking: " + ex.Message);
            return AuthResult.Failed(METHOD_GOOGLE, ex.Message);
        }
    }
#endif

    private void StartAnonymousSignIn_FromEvent()
    {
        _ = StartAnonymousSignIn_FromEventAsync();
    }

    private async Task StartAnonymousSignIn_FromEventAsync()
    {
        AuthResult authResult = await StartAnonymousSignIn();
        await _gameManager.HandleAuthenticationResult(authResult);
    }

    public async Task<AuthResult> StartAnonymousSignIn()
    {
        return await SignUpAnonymouslyAsync();
    }

    private async Task<AuthResult> SignUpAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            PlayerPrefs.SetString(LOGIN_KEY, METHOD_ANONYMOUS);
            PlayerPrefs.Save();

            return AuthResult.Succeeded(METHOD_ANONYMOUS);
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
            return AuthResult.Failed(METHOD_ANONYMOUS, ex.Message);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            return AuthResult.Failed(METHOD_ANONYMOUS, ex.Message);
        }
    }
}
