using System;
using System.Threading.Tasks;
using TMPro;
using TestBotRoom.Utils;
using UnityEngine;

namespace TestBotRoom.UI
{
    public class MenuCanvasControl : MonoBehaviour
    {
        [Header("Start References")]
        [SerializeField] private TMP_InputField playerInputField;
        [SerializeField] private GameObject startScreen;
        [SerializeField] private GameObject startContainer;
        [SerializeField] private GameObject loginOptionsContainer;
        [SerializeField] private GameObject profileSetupContainer;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private GameObject startButton;

        [Header("Lobby References")]
        [SerializeField] private GameObject lobbyScreen;
        [SerializeField] private TMP_Text coinCountText;
        [SerializeField] private TMP_Text bestScoreText;
        [SerializeField] private TMP_Text playerNameText;

        [Header("General Settings")]
        [SerializeField] private Color errorMessageColor = Color.red;

        private enum PendingMenuFlow
        {
            None,
            LoginChoice,
            ProfileSetup,
            Lobby
        }

        private Action _goToGameplayAction;
        private PendingMenuFlow _pendingMenuFlow;
        private Color _defaultMessageColor;
        private bool _defaultMessageColorCaptured;

        public void ConfigureActions(Action goToGameplayAction)
        {
            _goToGameplayAction = goToGameplayAction;
        }

        public void Initialize()
        {
            CacheMessageDefaults();
            RefreshCanvasForCurrentFlow();
        }

        private void OnEnable()
        {
            EventManager.Instance.AddListener(EventNameSaver.ShowSignInOptions, ShowLoginChoice);
            EventManager.Instance.AddListener(EventNameSaver.ShowProfileSetup, ShowProfileSetup);
            EventManager.Instance.AddListener(EventNameSaver.ShowLobby, ShowLobbyFromCurrentData);
        }

        private void OnDisable()
        {
            if (EventManager.Instance == null)
            {
                return;
            }

            EventManager.Instance.RemoveListener(EventNameSaver.ShowSignInOptions, ShowLoginChoice);
            EventManager.Instance.RemoveListener(EventNameSaver.ShowProfileSetup, ShowProfileSetup);
            EventManager.Instance.RemoveListener(EventNameSaver.ShowLobby, ShowLobbyFromCurrentData);
        }

        public void SavePlayerName_FromButton()
        {
            _ = SavePlayerName();
        }

        public void OnAnonymousSignInButtonClicked()
        {
            _ = GameManager.Instance.StartAnonymousSignInAsync();
        }

        public void OnGoogleSignInButtonClicked()
        {
            _ = GameManager.Instance.StartGooglePlayGamesSignInAsync();
        }

        public void OnPlayButtonClicked()
        {
            _goToGameplayAction?.Invoke();
        }

        public void OnStartButtonClicked()
        {
            GameManager.Instance.CompleteStartupGate();
            ApplyPendingFlow();
        }

        public void GotoLobby(GameStatus playerData)
        {
            coinCountText.text = playerData.Coins.ToString();
            bestScoreText.text = "Best Score: " + playerData.BestScore;
            playerNameText.text = playerData.PlayerName;
            SetStartButtonVisible(false);
            HideMessage();
            ApplyCanvasState(showStartContainer: false, showStartScreen: false, showLoginOptions: false, showProfileSetup: false, showLobbyScreen: true);
        }

        private async Task SavePlayerName()
        {
            if (IsPlayerNameValid(playerInputField.text))
            {
                await GameManager.Instance.SavePlayerName(playerInputField.text);
                GotoLobby(GameManager.Instance.GetPlayerData());
                return;
            }

            Debug.Log("Invalid player name.");
            ShowMessage("Name must have between 4 and 16 characters.", true);
        }

        private void RefreshCanvasForCurrentFlow()
        {
            switch (GameManager.Instance.CurrentAppFlowState)
            {
                case AppFlowState.Booting:
                case AppFlowState.LoadingProfile:
                    ShowLoadingMessage();
                    break;
                case AppFlowState.Lobby:
                    QueueOrApplyFlow(PendingMenuFlow.Lobby);
                    break;
                case AppFlowState.NeedsProfileSetup:
                    QueueOrApplyFlow(PendingMenuFlow.ProfileSetup);
                    break;
                case AppFlowState.NeedsLoginChoice:
                default:
                    QueueOrApplyFlow(PendingMenuFlow.LoginChoice);
                    break;
            }
        }

        private void ShowLoginChoice()
        {
            QueueOrApplyFlow(PendingMenuFlow.LoginChoice);
        }

        private void ShowProfileSetup()
        {
            QueueOrApplyFlow(PendingMenuFlow.ProfileSetup);
        }

        private void ShowLobbyFromCurrentData()
        {
            QueueOrApplyFlow(PendingMenuFlow.Lobby);
        }

        private void QueueOrApplyFlow(PendingMenuFlow pendingMenuFlow)
        {
            _pendingMenuFlow = pendingMenuFlow;

            if (!GameManager.Instance.HasCompletedStartupGate)
            {
                ShowStartPrompt();
                return;
            }

            ApplyPendingFlow();
        }

        private void ApplyPendingFlow()
        {
            switch (_pendingMenuFlow)
            {
                case PendingMenuFlow.LoginChoice:
                    DisplayLoginChoice();
                    break;
                case PendingMenuFlow.ProfileSetup:
                    DisplayProfileSetup();
                    break;
                case PendingMenuFlow.Lobby:
                    DisplayLobbyFromCurrentData();
                    break;
                case PendingMenuFlow.None:
                default:
                    RefreshCanvasForCurrentFlow();
                    break;
            }
        }

        private void DisplayLoginChoice()
        {
            Debug.Log("Menu canvas showing login choice screen.");
            SetStartButtonVisible(false);
            HideMessage();
            ApplyCanvasState(showStartContainer: true, showStartScreen: true, showLoginOptions: true, showProfileSetup: false, showLobbyScreen: false);
        }

        private void DisplayProfileSetup()
        {
            Debug.Log("Menu canvas showing profile setup screen.");
            SetStartButtonVisible(false);
            HideMessage();
            ApplyCanvasState(showStartContainer: false, showStartScreen: true, showLoginOptions: false, showProfileSetup: true, showLobbyScreen: false);
        }

        private void DisplayLobbyFromCurrentData()
        {
            GameStatus playerData = GameManager.Instance.GetPlayerData();

            if (playerData == null)
            {
                Debug.LogWarning("Menu canvas received lobby state, but player data is null.");
                ShowMessage("It was not possible to load player data.", true);
                return;
            }

            Debug.Log($"Menu canvas showing lobby for player '{playerData.PlayerName}'.");
            GotoLobby(playerData);
        }

        private void ShowLoadingMessage()
        {
            Debug.Log("Menu canvas showing loading message.");
            _pendingMenuFlow = PendingMenuFlow.None;
            SetStartButtonVisible(false);
            ApplyCanvasState(showStartContainer: true, showStartScreen: true, showLoginOptions: false, showProfileSetup: false, showLobbyScreen: false);
            ShowMessage("Loading data...", false);
        }

        private void ShowStartPrompt()
        {
            Debug.Log("Menu canvas waiting for Start button before revealing the next flow state.");
            HideMessage();
            SetStartButtonVisible(true);
            ApplyCanvasState(showStartContainer: true, showStartScreen: true, showLoginOptions: false, showProfileSetup: false, showLobbyScreen: false);
        }

        private void ShowMessage(string message, bool isError)
        {
            CacheMessageDefaults();

            if (messageText == null)
            {
                Debug.LogWarning($"Menu message could not be shown because the message references are missing. Message: {message}");
                return;
            }

            messageText.gameObject.SetActive(true);
            messageText.color = isError ? errorMessageColor : _defaultMessageColor;
            messageText.text = message;
        }

        private void HideMessage()
        {
            if (messageText == null)
            {
                return;
            }

            messageText.color = _defaultMessageColor;
            messageText.text = string.Empty;
            messageText.gameObject.SetActive(false);
        }

        private void ApplyCanvasState(bool showStartContainer, bool showStartScreen, bool showLoginOptions, bool showProfileSetup, bool showLobbyScreen)
        {
            startContainer.SetActive(showStartContainer);
            startScreen.SetActive(showStartScreen);
            loginOptionsContainer.SetActive(showLoginOptions);
            profileSetupContainer.SetActive(showProfileSetup);
            lobbyScreen.SetActive(showLobbyScreen);
        }

        private void SetStartButtonVisible(bool isVisible)
        {
            if (startButton != null)
            {
                startButton.SetActive(isVisible);
            }
        }

        private void CacheMessageDefaults()
        {
            if (_defaultMessageColorCaptured || messageText == null)
            {
                return;
            }

            _defaultMessageColor = messageText.color;
            _defaultMessageColorCaptured = true;
        }

        private bool IsPlayerNameValid(string name)
        {
            return name.Length is >= 4 and <= 16;
        }
    }
}
