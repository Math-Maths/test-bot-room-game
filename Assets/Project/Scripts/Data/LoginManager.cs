using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Unity.VisualScripting;
using TestBotRoom;



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
        LoginGooglePlayGames();
#endif

        //Debug.Log(PlayGamesPlatform.Instance.IsAuthenticated());
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

    private Task<string> GetGoogleTokenAsync()
    {
        var tcs = new TaskCompletionSource<string>();
    
        PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
        {
            if (string.IsNullOrEmpty(code))
            {
                Debug.LogError("Google não devolveu um código válido.");
                tcs.SetResult(null);
            }
            else
            {
                tcs.SetResult(code);
            }
        });
    
        return tcs.Task;
    }

    private async void StartSignInWithGooglePlayGames_FromEvent()
    {
        await StartSignInWithGooglePlayGames();
    }

    public async Task StartSignInWithGooglePlayGames()
    {
        if(!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            Debug.LogWarning("Não autenticado no GPG. Chamando login...");
            LoginGooglePlayGames();
            return; 
        }

        if(string.IsNullOrEmpty(m_GooglePlayGamesToken))
        {
            Debug.Log("Token nulo, solicitando novo token ao Google...");
            m_GooglePlayGamesToken = await GetGoogleTokenAsync();
        }

        if (!string.IsNullOrEmpty(m_GooglePlayGamesToken))
        {
            await SignInOrLinkWithGooglePlayGames();
        }
        else
        {
            Debug.LogError("Falha crítica: impossível obter token do Google Play Games.");
        }
    }

    public async Task SignInOrLinkWithGooglePlayGames()
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

            PlayerPrefs.SetString(LOGIN_KEY, METHOD_GOOGLE);
            PlayerPrefs.Save();

            await OnLoginSuccess(true);
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

            PlayerPrefs.SetString(LOGIN_KEY, METHOD_GOOGLE);
            PlayerPrefs.Save();

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

    private void StartAnonymousSignIn_FromEvent()
    {
        _ = StartAnonymousSignIn();
    }

    public async Task StartAnonymousSignIn()
    {
        await SignUpAnonymouslyAsync();
    }

    private async Task SignUpAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            PlayerPrefs.SetString(LOGIN_KEY, METHOD_ANONYMOUS);
            PlayerPrefs.Save();

            await OnLoginSuccess();
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
         }
    }

    /// <summary>
    /// Orchestrates data loading and UI transition once the identity is confirmed.
    /// </summary>
    private async Task OnLoginSuccess(bool goToLobby = false)
    {
        // Load existing data or create default if first time
        await _gameManager.LoadData();
        
        if(goToLobby)
        {
            EventManager.Instance.Invoke(EventNameSaver.ShowLobby);
        }
        //TODO else - mostrar o campo para preencher o nome do jogador, e só depois ir para a lobby.

        Debug.Log("Login flow completed. Data loaded and Lobby triggered.");
    }


}