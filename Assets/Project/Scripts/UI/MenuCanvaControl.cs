using TMPro;
using UnityEngine;
using TestBotRoom.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestBotRoom.UI
{
    public class MenuCanvasControl : MonoBehaviour
    {
        [Header("Start References")]
        [SerializeField] private TMP_InputField playerInputField;
        [SerializeField] private GameObject startScreen;

        [Header("Lobby References")]
        [SerializeField] private GameObject lobbyScreen;
        [SerializeField] private TMP_Text coinCountText;
        [SerializeField] private TMP_Text bestScoreText;
        [SerializeField] private TMP_Text playerNameText;
        
        private List<OnEventReaction> onEventReaction;

        public void Initialize()
        {
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            onEventReaction = new List<OnEventReaction>(GetComponentsInChildren<OnEventReaction>(true));
            foreach(var reaction in onEventReaction)
            {
                reaction.SubscribeEvent();
            }
        }

        public void SavePlayerName_FromButton()
        {
            _ = SavePlayerName();
        }

        private async Task SavePlayerName()
        {
            if(IsPlayerNameValid(playerInputField.text))
            {
                playerNameText.text = playerInputField.text;
                await GameManager.Instance.SavePlayerName(playerInputField.text);
                startScreen.SetActive(false);
                lobbyScreen.SetActive(true);
            }
            else
            {
                Debug.Log("Invalid player name.");
                //Mostrar mensagem de erro.
            }
        }

        public void GotoLobby(GameStatus playerData)
        {
            bestScoreText.text = "Best Score: " + playerData.BestScore.ToString();
            playerNameText.text = playerData.PlayerName;
            startScreen.SetActive(false);
            lobbyScreen.SetActive(true);
        }

        public void CallEvent(string eventName)
        {
            EventManager.Instance.Invoke(eventName);
        }

        private bool IsPlayerNameValid(string name)
        {
            if(name.Length is < 4 or > 16)
            {
                return false;
            }

            return true;
        }
    }
}