using System.Collections;
using TestBotRoom.Utils;
using UnityEngine;

namespace TestBotRoom
{
    public class LaserSpanwer : MonoBehaviour
    {
        [Header("Laser Spawner Settings")]
        [SerializeField] private LaserBehavior laserPrefab;
        [SerializeField] private Transform[] spawnPositions;
        [SerializeField] private float timeBetweenSpanw;
        [SerializeField] private float minSpawnTime;
        [SerializeField] private float maxLaserSpeed;
        [SerializeField] private float minLaserSpeed;

        [Space(10)]
        [Header("Antecipation Laser Animation Settings")]
        [SerializeField] private float maxLaserAnimationTime;
        [SerializeField] private float minLaserAnimationTime;

        private float _difficultMultiplier;

        private void OnEnable()
        {
            EventManager.Instance.AddListener(EventNameSaver.OnGameStarts, StartLasers);
            EventManager.Instance.AddListener(EventNameSaver.OnCoinColleted, AdjustDifficulty);
            EventManager.Instance.AddListener(EventNameSaver.OnPlayerDeath, StopAllCoroutines);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNameSaver.OnGameStarts, StartLasers);
            EventManager.Instance.RemoveListener(EventNameSaver.OnCoinColleted, AdjustDifficulty);
            EventManager.Instance.RemoveListener(EventNameSaver.OnPlayerDeath, StopAllCoroutines);
        }

        private void StartLasers()
        {
            _difficultMultiplier = 0.1f;
            DifficultyMultiplier.ResetDifficulty();
            StartCoroutine(SpawnSequence());
        }

        IEnumerator SpawnSequence()
        {
            while(GameManager.Instance.CurrentGameState == GameState.Gameplay)
            {
                float waitTime = Mathf.Lerp(timeBetweenSpanw, minSpawnTime, _difficultMultiplier);
                //Debug.Log("Wait Time: " + waitTime);
                yield return new WaitForSeconds(waitTime);

                int randomPosition = Random.Range(0, spawnPositions.Length);

                // Wait before spawning laser
                ElasticScale laserScale = spawnPositions[randomPosition].gameObject.GetComponent<ElasticScale>();
                float laserScaleTime = Mathf.Lerp(maxLaserAnimationTime, minLaserAnimationTime, _difficultMultiplier);
                laserScale.Play(laserScaleTime);
                yield return new WaitForSeconds(laserScaleTime);

                // Instantiate laser
                LaserBehavior laser = Instantiate(laserPrefab, spawnPositions[randomPosition].position, spawnPositions[randomPosition].rotation);
                float currentSpeed = Mathf.Lerp(minLaserSpeed, maxLaserSpeed, _difficultMultiplier);
                //Debug.Log("Current Speed: " + currentSpeed);
                laser.InitializeLaser(spawnPositions[randomPosition].forward, currentSpeed);
            }
        }

        private void AdjustDifficulty()
        {  
            _difficultMultiplier = DifficultyMultiplier.GetDifficulty();
            Debug.Log("Difficulty: " + _difficultMultiplier);
        }

    }
}