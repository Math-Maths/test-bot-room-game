using System;
using UnityEngine;

static public class EventNameSaver
{
    [Serializable]
    public enum EventNameRef
    {
        OnGameOver,
        OnCoinColleted,
        OnGameStarts,
        OnScoreChanged,
        OnBestScoreChanged,
        OnGameReset,
        ProvisionalPlay,
        GoToMenu,
        OnContinueGameplay,
        GoToGameplay,
        OnSignInWithGooglePlayGames,
        OnSignInAnonymously,
        OnLoginCheck,
        OnLoginCheckedEnd,
        ShowSignInOptions,
        HideSignInOptions,
        ShowLobby,
        OnPlayerNameChanged,
    }

    #region Gameplay Events
    public static string OnGameOver = "OnGameOver";
    public static string OnCoinColleted = "OnCoinColleted";
    public static string OnGameStarts = "OnGameStarts";
    public static string OnScoreChanged = "OnScoreChanged";
    public static string OnBestScoreChanged = "OnBestScoreChanged";
    public static string OnGameReset = "OnGameReset";
    public static string ProvisionalPlay = "ProvisionalPlayGame";
    public static string GoToMenu = "GoToMenu";
    public static string OnContinueGameplay = "OnContinueGameplay";
    #endregion

    #region Menu Events
    public static string GoToGameplay = "GoToGameplay";
    public static string OnSignInWithGooglePlayGames = "OnSignInWithGooglePlayGames";
    public static string OnSignInAnonymously = "OnSignInAnonymously";
    public static string OnLoginCheck = "OnLoginCheck";
    public static string OnLoginCheckedEnd = "OnLoginCheckedEnd";
    public static string ShowSignInOptions = "ShowSignInOptions";
    public static string HideSignInOptions = "HideSignInOptions";
    public static string ShowLobby = "ShowLobby";
    public static string OnPlayerNameChanged = "OnPlayerNameChanged";
    #endregion

    public static string GetEventName(EventNameRef _ref)
    {
        switch (_ref)
        {
            case EventNameRef.OnGameOver:
                return OnGameOver;
            case EventNameRef.OnCoinColleted:
                return OnCoinColleted;
            case EventNameRef.OnGameStarts:
                return OnGameStarts;
            case EventNameRef.OnScoreChanged:
                return OnScoreChanged;
            case EventNameRef.OnBestScoreChanged:
                return OnBestScoreChanged;
            case EventNameRef.OnGameReset:
                return OnGameReset;
            case EventNameRef.ProvisionalPlay:
                return ProvisionalPlay;
            case EventNameRef.GoToMenu:
                return GoToMenu;
            case EventNameRef.OnContinueGameplay:
                return OnContinueGameplay;
            case EventNameRef.GoToGameplay:
                return GoToGameplay;
            case EventNameRef.OnSignInWithGooglePlayGames:
                return OnSignInWithGooglePlayGames;
            case EventNameRef.OnSignInAnonymously:
                return OnSignInAnonymously;
            case EventNameRef.OnLoginCheck:
                return OnLoginCheck;
            case EventNameRef.OnLoginCheckedEnd:
                return OnLoginCheckedEnd;
            case EventNameRef.ShowSignInOptions:
                return ShowSignInOptions;
            case EventNameRef.HideSignInOptions:
                return HideSignInOptions;
            case EventNameRef.ShowLobby:
                return ShowLobby;
            case EventNameRef.OnPlayerNameChanged:
                return OnPlayerNameChanged;
            default:
                Debug.LogError($"EventNameRef '{_ref}' não mapeado para um nome de evento.");
                return null;
        }
    }
}