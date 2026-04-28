using TMPro;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.UI;
using System;

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
        [SerializeField] private Button menuButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button buttonToContinue;

        [Space(10)]
        [Header("UI Controls")]
        [SerializeField] private GameObject controlsPanel;

        [Space(10)]
        [Header("Initial Gameplay Screen")]
        [SerializeField] private TMP_Text startDelayText;

        private Color _continueButtonOriginalColor;
        private Color _continueTextOriginalColor;
        private Action _resetGameAction;
        private Action _goToMenuAction;
        private Action _continueGameplayAction;
        private Coroutine _showEndScreenCoroutine;

        public void ConfigureActions(Action resetGameAction, Action goToMenuAction, Action continueGameplayAction)
        {
            _resetGameAction = resetGameAction;
            _goToMenuAction = goToMenuAction;
            _continueGameplayAction = continueGameplayAction;
        }

        public void OnInitiate()
        {
            EventManager.Instance.AddListener<int>(EventNameSaver.OnScoreChanged, UpdateScore);
            EventManager.Instance.AddListener<int>(EventNameSaver.OnBestScoreChanged, UpdateBestScore);
            EventManager.Instance.AddListener(EventNameSaver.OnGameOver, DisableControls);

            _continueButtonOriginalColor = continueButton.color;
            _continueTextOriginalColor = continueText.color;
            SetEndScreenButtonsInteractable(true, true);
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

        public void OnResetButtonClicked()
        {
            _resetGameAction?.Invoke();
        }

        public void OnGoToMenuButtonClicked()
        {
            _goToMenuAction?.Invoke();
        }

        public void OnContinueGameplayButtonClicked()
        {
            _continueGameplayAction?.Invoke();
        }

        public void ShowEndScreen(int finalScore, bool canContinue = true)
        {
            if (_showEndScreenCoroutine != null)
            {
                StopCoroutine(_showEndScreenCoroutine);
            }

            _showEndScreenCoroutine = StartCoroutine(showEndScreenAfterDelay(finalScore, canContinue));
        }

        public void DisableControls()
        {
            controlsPanel.SetActive(false);
        }

        public void ResetContinueButton()
        {
            continueButton.color = _continueButtonOriginalColor;
            continueText.color = _continueTextOriginalColor;
            buttonToContinue.interactable = true;
        }

        public void BeginEndScreenActionTransition()
        {
            if (_showEndScreenCoroutine != null)
            {
                StopCoroutine(_showEndScreenCoroutine);
                _showEndScreenCoroutine = null;
            }

            SetEndScreenButtonsInteractable(false, false);
            endScreenPanel.Hide();
        }

        public void ResetEndScreenState()
        {
            ResetContinueButton();
            SetEndScreenButtonsInteractable(true, true);
        }

        IEnumerator showEndScreenAfterDelay(int finalScore, bool canContinue)
        {
            yield return new WaitForSeconds(endScreenDelay);
            if(!canContinue)
            {
                continueButton.color = Color.gray5;
                continueText.color = Color.gray7;
                buttonToContinue.interactable = false;
            }
            else
            {
                continueButton.color = _continueButtonOriginalColor;
                continueText.color = _continueTextOriginalColor;
                buttonToContinue.interactable = true;
            }

            SetEndScreenButtonsInteractable(true, canContinue);
            currentScoreText.SetScore(finalScore);
            endScreenPanel.gameObject.SetActive(true);
            endScreenPanel.PlayGameOverAnimation();
            _showEndScreenCoroutine = null;
        }

        private void SetEndScreenButtonsInteractable(bool primaryButtonsInteractable, bool continueButtonInteractable)
        {
            if (menuButton != null)
            {
                menuButton.interactable = primaryButtonsInteractable;
            }

            if (resetButton != null)
            {
                resetButton.interactable = primaryButtonsInteractable;
            }

            if (buttonToContinue != null)
            {
                buttonToContinue.interactable = continueButtonInteractable;
            }
        }
    }
}
