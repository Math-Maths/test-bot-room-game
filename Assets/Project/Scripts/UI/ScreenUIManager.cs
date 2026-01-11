using UnityEngine;

namespace TestBotRoom.UI
{
    public class ScreenUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject startScreen;

        private void Start()
        {
            EventManager.Instance.AddListener(EventNameSaver.OnGameOver, EnableStartScreen);
        }

        private void EnableStartScreen()
        {
            Debug.Log("Reset");
            startScreen.SetActive(true);
        }

    }
}