using System.Collections;
using TestBotRoom.Utils;
using Unity.VisualScripting;
using UnityEngine;

namespace TestBotRoom.Gameplay
{
    public class LaserSpawner : MonoBehaviour, IInitiation
    {
        [Header("Laser Spawner Settings")]
        [SerializeField] private LaserBehavior laserPrefab;
        [SerializeField] private Transform[] spawnPositions;
        [SerializeField] private float timeBetweenSpanw;
        [SerializeField] private float minSpawnTime;
        [SerializeField] private float maxLaserSpeed;
        [SerializeField] private float minLaserSpeed;
        [SerializeField] private int laserPoolSize;

        [Space(10)]
        [Header("Antecipation Laser Animation Settings")]
        [SerializeField] private float maxLaserAnimationTime;
        [SerializeField] private float minLaserAnimationTime;

        private float _difficultMultiplier;
        
        private const string LASER_POOL_ID = "LaserPool";

        public void OnInitiate()
        {
            EventManager.Instance.AddListener(EventNameSaver.OnCoinColleted, AdjustDifficulty);
            EventManager.Instance.AddListener(EventNameSaver.OnGameOver, StopAllCoroutines);
            EventManager.Instance.AddListener(EventNameSaver.OnGameOver, DisableAllLasers);

            _difficultMultiplier = 0.1f;
            DifficultyMultiplier.ResetDifficulty();

            PoolService.Instance.CreatePool(LASER_POOL_ID, laserPrefab.gameObject, laserPoolSize);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNameSaver.OnCoinColleted, AdjustDifficulty);
            EventManager.Instance.RemoveListener(EventNameSaver.OnGameOver, StopAllCoroutines);
            EventManager.Instance.RemoveListener(EventNameSaver.OnGameOver, DisableAllLasers);
        }

        public void StartLasers(bool reset = false)
        {
            if(!reset)
            {
                DifficultyMultiplier.ResetDifficulty();
                _difficultMultiplier = 0.1f;
            }
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
                GameObject laserGO = PoolService.Instance.Spawn(LASER_POOL_ID, spawnPositions[randomPosition].position, spawnPositions[randomPosition].rotation);
                LaserBehavior laser = laserGO.GetComponent<LaserBehavior>();
                float currentSpeed = Mathf.Lerp(minLaserSpeed, maxLaserSpeed, _difficultMultiplier);
                //Debug.Log("Current Speed: " + currentSpeed);
                laser.InitializeLaser(spawnPositions[randomPosition].forward, currentSpeed);
            }
        }

        private void DisableAllLasers()
        {
            PoolService.Instance.DespawnAll(LASER_POOL_ID); 
        }

        private void AdjustDifficulty()
        {  
            _difficultMultiplier = DifficultyMultiplier.GetDifficulty();
            //Debug.Log("Difficulty: " + _difficultMultiplier);
        }

    }
}