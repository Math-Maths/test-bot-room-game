using System.Collections;
using UnityEngine;

namespace TestBotRoom
{
    public class LaserSpanwer : MonoBehaviour
    {
        [SerializeField] private LaserBehavior laserPrefab;
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private Transform[] spawnPositions;
        [SerializeField] private float timeBetweenSpanw;

        private float _nextSpanw;
        private GameObject spawnedArrow;

        private void Start()
        {
            StartCoroutine(SpawnSequence());
        }

        // private void Update()
        // {
        //     if(Time.time > _nextSpanw)
        //     {
        //         _nextSpanw = Time.time + timeBetweenSpanw;

        //         int randomPosition = Random.Range(0, spawnPositions.Length);
        //         LaserBehavior laser = Instantiate(laserPrefab, spawnPositions[randomPosition]);
        //         laser.InitializeLaser(spawnPositions[randomPosition].forward);
        //     }
        // }

        IEnumerator SpawnSequence()
        {
            while(true)
            {
                yield return new WaitForSeconds(timeBetweenSpanw);

                int randomPosition = Random.Range(0, spawnPositions.Length);
                Vector3 arrowPosition = spawnPositions[randomPosition].position;

                // Show arrow
                if(spawnedArrow == null)
                    spawnedArrow = Instantiate(arrowPrefab, arrowPosition, Quaternion.identity);
                else
                    spawnedArrow.transform.position = arrowPosition;

                spawnedArrow.transform.forward = spawnPositions[randomPosition].forward;
                spawnedArrow.SetActive(true);

                // Wait before spawning laser
                yield return new WaitForSeconds(1.5f);

                // Instantiate laser
                LaserBehavior laser = Instantiate(laserPrefab, spawnPositions[randomPosition]);
                laser.InitializeLaser(spawnPositions[randomPosition].forward);

                // Hide arrow
                spawnedArrow.SetActive(false);
            }

        }

    }
}