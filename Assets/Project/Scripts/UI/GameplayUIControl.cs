using TMPro;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.UI;

namespace TestBotRoom.UI
{
    public class GameplayUIControl : MonoBehaviour, IInitiation
    {
        [Header("World UI Elements")]
        [SerializeField] private TMP_Text currentScoreText_World;

        [Space(10)]
        [Header("End Screen UI Elements")]
        [SerializeField] private GameOverPanelTweenControl endScreenPanel;
        [SerializeField] private ScoreCounter currentScoreText;
        [SerializeField] private TMP_Text bestScoreText;
        [SerializeField] private float endScreenDelay = 3f;
        [SerializeField] private Image continueButton;
        [SerializeField] private TMP_Text continueText;
        [SerializeField] private Button buttonToContinue;

        [Space(10)]
        [Header("UI Controls")]
        [SerializeField] private GameObject controlsPanel;

        [Space(10)]
        [Header("Initial Gameplay Screen")]
        [SerializeField] private TMP_Text startDelayText;

        private Color _continueButtonOriginalColor;
        private Color _continueTextOriginalColor;

        public void OnInitiate()
        {
            EventManager.Instance.AddListener<int>(EventNameSaver.OnScoreChanged, UpdateScore);
            EventManager.Instance.AddListener<int>(EventNameSaver.OnBestScoreChanged, UpdateBestScore);
            EventManager.Instance.AddListener(EventNameSaver.OnGameOver, DisableControls);

            _continueButtonOriginalColor = continueButton.color;
            _continueTextOriginalColor = continueText.color;
            buttonToContinue.enabled = true;
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener<int>(EventNameSaver.OnScoreChanged, UpdateScore);
            EventManager.Instance.RemoveListener<int>(EventNameSaver.OnBestScoreChanged, UpdateBestScore);
            EventManager.Instance.RemoveListener(EventNameSaver.OnGameOver, DisableControls);
        }

        public async Task ShowCountdown(float seconds)
        {
            startDelayText.gameObject.SetActive(true);

            int remaining = Mathf.CeilToInt(seconds);

            while (remaining > 0)
            {
                startDelayText.text = remaining.ToString();
                await Task.Delay(1000);
                remaining--;
            }

            startDelayText.text = "GO!";
            await Task.Delay(500);

            startDelayText.gameObject.SetActive(false);
            controlsPanel.SetActive(true);
        }

        private void UpdateScore(int value)
        {
            currentScoreText_World.text = value.ToString("000");
        }

        private void UpdateBestScore(int value)
        {
            bestScoreText.text = value.ToString("000");
        }

        public void CallEvent(string eventName)
        {
            EventManager.Instance.Invoke(eventName);
        }

        public void ShowEndScreen(int finalScore, bool canContinue = true)
        {
            StartCoroutine(showEndScreenAfterDelay(finalScore, canContinue));
        }

        public void DisableControls()
        {
            controlsPanel.SetActive(false);
        }

        public void ResetContinueButton()
        {
            continueButton.color = _continueButtonOriginalColor;
            continueText.color = _continueTextOriginalColor;
            buttonToContinue.enabled = true;
        }

        IEnumerator showEndScreenAfterDelay(int finalScore, bool canContinue)
        {
            yield return new WaitForSeconds(endScreenDelay);
            if(!canContinue)
            {
                continueButton.color = Color.gray5;
                continueText.color = Color.gray7;
                buttonToContinue.enabled = false;
            }
            else
            {
                continueButton.color = _continueButtonOriginalColor;
                continueText.color = _continueTextOriginalColor;
                buttonToContinue.enabled = true;
            }

            currentScoreText.SetScore(finalScore);
            endScreenPanel.gameObject.SetActive(true);
            endScreenPanel.PlayGameOverAnimation();
        }
    }
}
