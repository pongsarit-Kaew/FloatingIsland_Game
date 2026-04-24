using UnityEngine;
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
}

public class WaveSpawnManager : MonoBehaviour
{
    public List<Wave> waves;
    public Transform[] spawnPoints;

    [Header("Prefabs")]
    public GameObject enemyPrefab;
    public GameObject powerUpPrefab;
    public GameObject stunPowerPrefab;

    [Header("Level Completion")]
    [Tooltip("ลากจุดเส้นชัย (ที่มีสคริปต์ LevelFinish) มาใส่ตรงนี้")]
    public GameObject finishPortal;

    [Header("Item Spawn Settings")]
    [Tooltip("ระยะการสุ่มแกน X (ซ้าย-ขวา)")]
    public float spawnRangeX = 9.0f;
    [Tooltip("ระยะการสุ่มแกน Z (หน้า-หลัง)")]
    public float spawnRangeZ = 9.0f;
    [Tooltip("ความสูงที่ไอเทมจะเกิด (แกน Y) ป้องกันการมุดดิน")]
    public float spawnPosY = 0.5f;

    void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        // 1. ซ่อนเส้นชัยไว้ตั้งแต่เริ่มเกม
        if (finishPortal != null)
        {
            finishPortal.SetActive(false);
        }

        int waveIndex = 1;
        foreach (Wave wave in waves)
        {
            Debug.Log($"เริ่ม Wave {waveIndex}");

            List<Transform> activePoints = GetRandomSpawnPoints(wave.numberOfRandomSpawnPoint);

            if (activePoints.Count == 0)
            {
                Debug.LogError("Error: ไม่พบจุดเกิดศัตรู (Spawn Points)");
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

            // รอจนกว่าจะเสกศัตรูครบตามจำนวนในเวฟนั้น
            yield return StartCoroutine(SpawnEnemyRoutine(wave, activePoints));

            // 2. [ส่วนที่เพิ่มเข้ามา] รอจนกว่าผู้เล่นจะฆ่าศัตรูตายเกลี้ยง ถึงจะผ่านเวฟนี้ได้
            // (เป็นการบังคับว่าต้องเคลียร์ศัตรูให้หมดก่อน เวฟต่อไปถึงจะมา)
            yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Enemy").Length == 0);

            Debug.Log($"เคลียร์ Wave {waveIndex} สำเร็จ!");
            waveIndex++;
        }

        // 3. เมื่อลูป foreach ทำงานเสร็จ แปลว่าเคลียร์ครบทุกเวฟแล้ว! ให้เปิดเส้นชัยได้เลย
        Debug.Log("เคลียร์ทุกเวฟแล้ว! ปรากฏเส้นชัย!");
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
        if (prefab == null) return;

        Vector3 randomPos = GenerateRandomItemPosition();
        Instantiate(prefab, randomPos, prefab.transform.rotation);
        Debug.Log($"{itemName} {randomPos}");
    }
}