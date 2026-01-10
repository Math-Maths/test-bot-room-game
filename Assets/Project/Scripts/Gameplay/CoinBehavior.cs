using System;
using UnityEngine;

namespace TestBotRoom
{
    public class CoinBehavior : MonoBehaviour
    {
        public event Action OnCoinColleted;

        private void OnTriggerEnter(Collider other)
        {
            OnCoinColleted?.Invoke();
            EventManager.Instance.Invoke(EventNameSaver.OnCoinColleted);
        }
    }
}