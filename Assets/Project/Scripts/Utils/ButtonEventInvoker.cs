using UnityEngine;
using UnityEngine.UI;

namespace TestBotRoom.Utils
{
    public class ButtonEventInvoker : MonoBehaviour
    {
        [SerializeField] private EventNameSaver.EventNameRef eventName;

        private Button _button;

        private void Start()
        {
            _button = GetComponent<Button>();
            if (_button == null)
            {
                Debug.LogError("ButtonEventInvoker requires a Button component on the same GameObject.");
                return;
            }

            _button.onClick.AddListener(ButtonEventInvoke);
        }

        public void ButtonEventInvoke()
        {
            Debug.Log($"Button clicked! Invoking event: {EventNameSaver.GetEventName(eventName)}");
            EventManager.Instance.Invoke(EventNameSaver.GetEventName(eventName));
        }
    }
}