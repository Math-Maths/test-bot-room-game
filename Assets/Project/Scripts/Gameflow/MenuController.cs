using TestBotRoom.UI;
using UnityEngine;

namespace TestBotRoom
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private MenuCanvasControl _menuCanvas;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private GameObject _environment;
        [SerializeField] private GameObject _characterHolder;
        [SerializeField] private LoadingScreenControl _loadingScreen;

        private void OnEnable()
        {
            EventManager.Instance.AddListener(EventNameSaver.GoToGameplay, LoadGamePlay);
            BindObjects();
            //Show loading screen
            _loadingScreen.ShowLoadScreen();
            //Get Data from GameManager
            InitializeObjects();
            _loadingScreen.HideLoadingScreen();
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNameSaver.GoToGameplay, LoadGamePlay);
        }

        private void BindObjects()
        {
            _menuCanvas = Instantiate(_menuCanvas);
            _mainCamera = Instantiate(_mainCamera);
            _environment = Instantiate(_environment);
            _characterHolder = Instantiate(_characterHolder);
            _loadingScreen = Instantiate(_loadingScreen);
        }

        private void InitializeObjects()
        {
            _menuCanvas.Initialize(GetData());
        }

        private void LoadGamePlay()
        {
            //Show loading screen
            _loadingScreen.ShowLoadScreen();
            GameManager.Instance.ChangeScene("Gameplay_Scene");
        }

        private GameStatus GetData()
        {
            return GameManager.Instance.GetPlayerData();
        }
    }
}