using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Wave
{
    public int totalSpawnEnemies;
    public int numberOfRandomSpawnPoint;
    public float delayStart;
    public float spawnInterval;
    public int numberOfPowerUp;
    public int numberOfStunPower;
    public int numberOfCoins;
}

public class WaveSpawnManager : MonoBehaviour
{
    public List<Wave> waves;
    public Transform[] spawnPoints;
    public int defaultCoinsPerWave = 3;
    public bool useLevelNumberForCoinDrop = true;
    public int coinsPerLevel = 3;

    [Header("Prefabs")]
    public GameObject enemyPrefab;
    public GameObject powerUpPrefab;
    public GameObject stunPowerPrefab;
    public GameObject coinPrefab;

    [Header("Level Completion")]
    [Tooltip("Drag the finish portal object that has LevelFinish here.")]
    public GameObject finishPortal;

    [Header("Item Spawn Settings")]
    [Tooltip("Random spawn range on the X axis.")]
    public float spawnRangeX = 9.0f;
    [Tooltip("Random spawn range on the Z axis.")]
    public float spawnRangeZ = 9.0f;
    [Tooltip("Item spawn height on the Y axis.")]
    public float spawnPosY = 0.5f;

    void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        if (finishPortal != null)
        {
            finishPortal.SetActive(false);
        }

        int waveIndex = 1;
        foreach (Wave wave in waves)
        {
            Debug.Log($"Start Wave {waveIndex}");

            List<Transform> activePoints = GetRandomSpawnPoints(wave.numberOfRandomSpawnPoint);

            if (activePoints.Count == 0)
            {
                Debug.LogError("Error: no enemy spawn points found.");
                yield break;
            }

            for (int i = 0; i < wave.numberOfPowerUp; i++)
            {
                SpawnItemAtRandomPosition(powerUpPrefab, "PowerUp");
            }

            for (int i = 0; i < wave.numberOfStunPower; i++)
            {
                SpawnItemAtRandomPosition(stunPowerPrefab, "StunPower");
            }

            yield return new WaitForSeconds(wave.delayStart);

            yield return StartCoroutine(SpawnEnemyRoutine(wave, activePoints));

            Debug.Log($"Wave {waveIndex} finished spawning. Drop coins.");

            int coinsToDrop = GetCoinsToDrop(wave);
            for (int i = 0; i < coinsToDrop; i++)
            {
                SpawnItemAtRandomPosition(coinPrefab, "Coin");
            }

            yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Enemy").Length == 0);

            Debug.Log($"Clear Wave {waveIndex}");

            waveIndex++;
        }

        Debug.Log("All waves cleared. Finish portal opened.");
        if (finishPortal != null)
        {
            finishPortal.SetActive(true);
        }
    }

    IEnumerator SpawnEnemyRoutine(Wave wave, List<Transform> activePoints)
    {
        for (int i = 0; i < wave.totalSpawnEnemies; i++)
        {
            SpawnEnemyAtPoint(enemyPrefab, activePoints);
            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    int GetCoinsToDrop(Wave wave)
    {
        if (useLevelNumberForCoinDrop)
        {
            int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
            return currentLevelIndex * coinsPerLevel;
        }

        if (wave.numberOfCoins > 0)
        {
            return wave.numberOfCoins;
        }

        return defaultCoinsPerWave;
    }

    List<Transform> GetRandomSpawnPoints(int count)
    {
        List<Transform> selected = new List<Transform>();
        if (spawnPoints == null || spawnPoints.Length == 0) return selected;

        List<Transform> pool = new List<Transform>(spawnPoints);
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            selected.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return selected;
    }

    void SpawnEnemyAtPoint(GameObject prefab, List<Transform> points)
    {
        if (points == null || points.Count == 0 || prefab == null) return;

        int index = Random.Range(0, points.Count);
        Instantiate(prefab, points[index].position, prefab.transform.rotation);
    }

    Vector3 GenerateRandomItemPosition()
    {
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        float randomZ = Random.Range(-spawnRangeZ, spawnRangeZ);
        return new Vector3(randomX, spawnPosY, randomZ);
    }

    void SpawnItemAtRandomPosition(GameObject prefab, string itemName)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"{itemName} prefab is missing on WaveSpawnManager.");
            return;
        }

        Vector3 randomPos = GenerateRandomItemPosition();
        Instantiate(prefab, randomPos, prefab.transform.rotation);
        Debug.Log($"{itemName} {randomPos}");
    }
}
