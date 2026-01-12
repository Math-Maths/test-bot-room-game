using System.Collections;
using TestBotRoom.Utils;
using UnityEngine;

namespace TestBotRoom
{
    public class LaserSpanwer : MonoBehaviour
    {
        [SerializeField] private LaserBehavior laserPrefab;
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private Transform[] spawnPositions;
        [SerializeField] private float timeBetweenSpanw;

        [SerializeField] private float minSpawnTime;
        [SerializeField] private float maxLaserSpeed;
        [SerializeField] private float minLaserSpeed;
        [SerializeField] private float maxArrowTime;
        [SerializeField] private float minArrowTime;

        private float _difficultMultiplier;
        private GameObject spawnedArrow;

        private void Start()
        {
            EventManager.Instance.AddListener(EventNameSaver.OnGameStarts, StartLasers);
            EventManager.Instance.AddListener(EventNameSaver.OnCoinColleted, AdjustDifficulty);
            _difficultMultiplier = 0.1f;
        }

        private void StartLasers()
        {
            _difficultMultiplier = 0.1f;
            DifficultyMultiplier.ResetDifficulty();
            StartCoroutine(SpawnSequence());
        }

        IEnumerator SpawnSequence()
        {
            while(GameManager.Instance.IsGameRunning)
            {
                float waitTime = Mathf.Lerp(timeBetweenSpanw, minSpawnTime, _difficultMultiplier);
                //Debug.Log("Wait Time: " + waitTime);
                yield return new WaitForSeconds(waitTime);

                int randomPosition = Random.Range(0, spawnPositions.Length);
                //Vector3 arrowPosition = spawnPositions[randomPosition].position;

                // Show arrow
                //if(spawnedArrow == null)
                //    spawnedArrow = Instantiate(arrowPrefab, arrowPosition, Quaternion.identity);
                //else
                //    spawnedArrow.transform.position = arrowPosition;
                //spawnedArrow.transform.forward = spawnPositions[randomPosition].forward;
                //spawnedArrow.SetActive(true);

                // Wait before spawning laser
                ElasticScale laserScale = spawnPositions[randomPosition].gameObject.GetComponent<ElasticScale>();
                float laserScaleTime = Mathf.Lerp(maxArrowTime, minArrowTime, _difficultMultiplier);
                laserScale.Play(laserScaleTime);
                yield return new WaitForSeconds(laserScaleTime);

                // Instantiate laser
                LaserBehavior laser = Instantiate(laserPrefab, spawnPositions[randomPosition].position, spawnPositions[randomPosition].rotation);
                float currentSpeed = Mathf.Lerp(minLaserSpeed, maxLaserSpeed, _difficultMultiplier);
                //Debug.Log("Current Speed: " + currentSpeed);
                laser.InitializeLaser(spawnPositions[randomPosition].forward, currentSpeed);

                // Hide arrow
                //spawnedArrow.SetActive(false);
            }
        }

        private void AdjustDifficulty()
        {  
            _difficultMultiplier = DifficultyMultiplier.GetDifficulty();
            Debug.Log("Difficulty: " + _difficultMultiplier);
        }

    }
}