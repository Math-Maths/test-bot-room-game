using System.Collections;
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
            StartCoroutine(EnableStartScreenCoroutine());
        }

        IEnumerator EnableStartScreenCoroutine()
        {
            yield return new WaitForSeconds(3f);
            startScreen.SetActive(true);
        }


    }
}