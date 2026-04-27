using UnityEngine;

namespace TestBotRoom.Utils
{
    public class OnEventReaction : MonoBehaviour
    {
        public enum ReactionType
        {
            DisableObject,
            EnableObject,
        }

        [SerializeField] private EventNameSaver.EventNameRef eventName;
        [SerializeField] private ReactionType reactionType;

        public void SubscribeEvent()
        {
            Debug.Log(gameObject.name + " subscribing to event: " + EventNameSaver.GetEventName(eventName));
            EventManager.Instance.AddListener(EventNameSaver.GetEventName(eventName), React);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNameSaver.GetEventName(eventName), React);
        }

        private void React()
        {
            switch (reactionType)
            {
                case ReactionType.DisableObject:
                    gameObject.SetActive(false);
                    Debug.Log($"Event '{eventName}' triggered! Reacting by disabling the object.");
                    break;
                case ReactionType.EnableObject:
                    gameObject.SetActive(true);
                    Debug.Log($"Event '{eventName}' triggered! Reacting by enabling the object.");
                    break;
            }
        }
    }
}