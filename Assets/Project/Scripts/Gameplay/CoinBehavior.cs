using System;
using TestBotRoom.Utils;
using UnityEngine;

namespace TestBotRoom.Gameplay
{
    public class CoinBehavior : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private float floatAmplitude = 0.5f;
        [SerializeField] private float floatFrequency = 1f;
        [SerializeField] private float scaleTime = 0.5f;

        private void Update()
        {
            // Rotate the coin
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // Float the coin up and down
            Vector3 position = transform.position;
            position.y += Mathf.Sin(Time.time * floatFrequency) * floatAmplitude * Time.deltaTime;
            transform.position = position;
        }


        private void OnTriggerEnter(Collider other)
        {
            DifficultyMultiplier.currentCoinCount++;
            EventManager.Instance.Invoke(EventNameSaver.OnCoinColleted);
        }

        public void SpawnAnimation()
        {
            transform.localScale = Vector3.zero;
            LeanTween.scale(gameObject, Vector3.one * 0.4f, scaleTime).setEaseInOutBack();
        }
    }
}