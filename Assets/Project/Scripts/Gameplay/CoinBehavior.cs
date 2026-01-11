using System;
using TestBotRoom.Utils;
using UnityEngine;

namespace TestBotRoom
{
    public class CoinBehavior : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            DifficultyMultiplier.currentCoinCount++;
            EventManager.Instance.Invoke(EventNameSaver.OnCoinColleted);
        }
    }
}