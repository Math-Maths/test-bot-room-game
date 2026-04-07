using System.Threading.Tasks;
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

        private async void OnEnable()
        {
            EventManager.Instance.AddListener(EventNameSaver.GoToGameplay, LoadGamePlay);
            BindObjects();
            _loadingScreen.ShowLoadScreen();
            //Get Data from GameManager
            await InitializeObjects();
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

        private async Task InitializeObjects()
        {
            if(GameManager.Instance.CurrentGameState == GameState.Lobby)
            {
                _menuCanvas.GotoLobby(GetData());
                _mainCamera.transform.position = new Vector3(0, 2.45f, -10);
                _mainCamera.transform.rotation = Quaternion.Euler(Vector3.right * 9);
                return;
            }
            
            _menuCanvas.Initialize();
            _mainCamera.transform.position = new Vector3(0, 2.45f, -10);
            _mainCamera.transform.rotation = Quaternion.Euler(Vector3.right * 9);
        }

        private void LoadGamePlay()
        {
            _loadingScreen.ShowLoadScreen();
            GameManager.Instance.ChangeScene("Gameplay_Scene");
        }

        private GameStatus GetData()
        {
            return GameManager.Instance.GetPlayerData();
        }
    }
}