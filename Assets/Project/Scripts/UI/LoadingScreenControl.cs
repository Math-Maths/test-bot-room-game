using UnityEngine;

namespace TestBotRoom.UI
{
    public class LoadingScreenControl : MonoBehaviour
    {
        [SerializeField] private GameObject loadingScreen;

        public void ShowLoadScreen()
        {
            loadingScreen.SetActive(true);
        }

        public void HideLoadingScreen()
        {   
            loadingScreen.SetActive(false);
        }
    }
}