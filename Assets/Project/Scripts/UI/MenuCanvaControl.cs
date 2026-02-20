using TMPro;
using UnityEngine;

namespace TestBotRoom.UI
{
    public class MenuCanvasControl : MonoBehaviour
    {
        //[SerializeField] private TMP_Text gearCountText;
        [SerializeField] private TMP_Text coinCountText;
        [SerializeField] private TMP_Text bestScoreText;
        [SerializeField] private TMP_Text playerNameText;

        public void Initialize(GameStatus playerData)
        {
            //gearCountText.text = playerData.Gears.ToString();
            coinCountText.text = playerData.Coins.ToString();
            bestScoreText.text = "Best Score: " + playerData.BestScore.ToString();
            playerNameText.text = playerData.PlayerName;
        }

        public void CallEvent(string eventName)
        {
            EventManager.Instance.Invoke(eventName);
        }
    }
}