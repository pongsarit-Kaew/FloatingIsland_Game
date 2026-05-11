using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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
    public static bool GameHasStarted { get; private set; } = true;

    public List<Wave> waves;
    public Transform[] spawnPoints;

    [Header("Coin Rules")]
    public int coinsPerLevel = 3;
    public int bonusCoinsPerLevel = 1;

    [Header("Prefabs")]
    public GameObject enemyPrefab;
    public GameObject powerUpPrefab;
    public GameObject stunPowerPrefab;
    public GameObject coinPrefab;
    public GameObject bonusCoinPrefab;

    [Header("Level Completion")]
    [Tooltip("Drag the finish portal object that has LevelFinish here.")]
    public GameObject finishPortal;
    [Tooltip("Use this only when a level needs a custom coin requirement. Keep 0 to auto-calculate.")]
    public int requiredCoinsOverride = 0;

    [Header("Item Spawn Settings")]
    [Tooltip("Random spawn range on the X axis.")]
    public float spawnRangeX = 4f;
    [Tooltip("Random spawn range on the Z axis.")]
    public float spawnRangeZ = 4f;
    [Tooltip("Item spawn height on the Y axis.")]
    public float spawnPosY = 0.5f;
    [Tooltip("Minimum distance between spawned items. Larger values spread items farther apart.")]
    public float minimumItemSpawnDistance = 1.5f;

    [Header("Start Countdown")]
    public float startCountdownDuration = 3f;
    public TMP_Text startCountdownText;
    public bool lockPlayerDuringStartCountdown = true;

    private readonly List<Vector3> availableItemSpawnPositions = new List<Vector3>();

    void Start()
    {
        GameHasStarted = false;
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        if (finishPortal != null)
        {
            finishPortal.SetActive(false);
        }

        yield return StartCoroutine(RunStartCountdown());

        for (int waveListIndex = 0; waveListIndex < waves.Count; waveListIndex++)
        {
            Wave wave = waves[waveListIndex];
            int waveIndex = waveListIndex + 1;
            bool isLastWave = waveListIndex == waves.Count - 1;

            Debug.Log($"Start Wave {waveIndex}");

            List<Transform> activePoints = GetRandomSpawnPoints(wave.numberOfRandomSpawnPoint);
            PrepareItemSpawnPositions();

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

            yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Enemy").Length == 0);

            Debug.Log($"Clear Wave {waveIndex}");

            int coinsToDrop = GetBaseCoinsToDrop(wave);
            int bonusCoinsToDrop = isLastWave ? GetLevelBonusCoins() : 0;
            Debug.Log($"Wave {waveIndex} completed. Drop {coinsToDrop} coins and {bonusCoinsToDrop} bonus coins.");
            for (int coinIndex = 0; coinIndex < coinsToDrop; coinIndex++)
            {
                SpawnCoinAtRandomPosition(false);
            }

            for (int bonusCoinIndex = 0; bonusCoinIndex < bonusCoinsToDrop; bonusCoinIndex++)
            {
                SpawnCoinAtRandomPosition(true);
            }
        }

        int requiredCoins = GetRequiredCoins();
        Debug.Log($"All waves cleared. Waiting for {requiredCoins} coins before opening finish portal.");
        yield return new WaitUntil(() => HasPlayerCollectedRequiredCoins(requiredCoins));

        Debug.Log("Coin requirement met. Finish portal opened.");
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

    int GetBaseCoinsToDrop(Wave wave)
    {
        return Mathf.Max(0, wave.numberOfCoins);
    }

    int GetRequiredCoins()
    {
        if (requiredCoinsOverride > 0)
        {
            return requiredCoinsOverride;
        }

        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        return currentLevelIndex * coinsPerLevel;
    }

    int GetLevelBonusCoins()
    {
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        return currentLevelIndex * bonusCoinsPerLevel;
    }

    bool HasPlayerCollectedRequiredCoins(int requiredCoins)
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return false;

        PlayerController player = playerObject.GetComponent<PlayerController>();
        if (player == null) return false;

        return player.coinCount >= requiredCoins;
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

    void PrepareItemSpawnPositions()
    {
        availableItemSpawnPositions.Clear();

        float spacing = Mathf.Max(0.1f, minimumItemSpawnDistance);
        for (float x = -spawnRangeX; x <= spawnRangeX; x += spacing)
        {
            for (float z = -spawnRangeZ; z <= spawnRangeZ; z += spacing)
            {
                availableItemSpawnPositions.Add(new Vector3(x, spawnPosY, z));
            }
        }

        ShuffleItemSpawnPositions();
    }

    void ShuffleItemSpawnPositions()
    {
        for (int i = 0; i < availableItemSpawnPositions.Count; i++)
        {
            int randomIndex = Random.Range(i, availableItemSpawnPositions.Count);
            Vector3 currentPosition = availableItemSpawnPositions[i];
            availableItemSpawnPositions[i] = availableItemSpawnPositions[randomIndex];
            availableItemSpawnPositions[randomIndex] = currentPosition;
        }
    }

    bool TryGetRandomItemPosition(out Vector3 position)
    {
        if (availableItemSpawnPositions.Count == 0)
        {
            position = Vector3.zero;
            return false;
        }

        int lastIndex = availableItemSpawnPositions.Count - 1;
        position = availableItemSpawnPositions[lastIndex];
        availableItemSpawnPositions.RemoveAt(lastIndex);
        return true;
    }

    void SpawnCoinAtRandomPosition(bool isBonusCoin)
    {
        GameObject prefab = isBonusCoin && bonusCoinPrefab != null ? bonusCoinPrefab : coinPrefab;
        GameObject spawnedCoin = SpawnItemAtRandomPosition(prefab, isBonusCoin ? "Bonus Coin" : "Coin");
        if (spawnedCoin == null) return;

        Coin coin = spawnedCoin.GetComponent<Coin>();
        if (coin != null)
        {
            coin.isBonusCoin = isBonusCoin;
        }
    }

    GameObject SpawnItemAtRandomPosition(GameObject prefab, string itemName)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"{itemName} prefab is missing on WaveSpawnManager.");
            return null;
        }

        if (!TryGetRandomItemPosition(out Vector3 randomPos))
        {
            Debug.LogWarning($"No free item spawn position left for {itemName}. Try lowering Minimum Item Spawn Distance or increasing Spawn Range X/Z.");
            return null;
        }

        GameObject spawnedItem = Instantiate(prefab, randomPos, prefab.transform.rotation);
        Debug.Log($"{itemName} {randomPos}");
        return spawnedItem;
    }

    IEnumerator RunStartCountdown()
    {
        if (startCountdownDuration <= 0f) yield break;

        PlayerController player = FindPlayer();
        if (lockPlayerDuringStartCountdown && player != null)
        {
            player.canMove = false;
        }

        TMP_Text countdownText = GetOrCreateStartCountdownText();
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        for (int secondsLeft = Mathf.CeilToInt(startCountdownDuration); secondsLeft > 0; secondsLeft--)
        {
            if (countdownText != null)
            {
                countdownText.text = secondsLeft.ToString();
            }

            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
        {
            countdownText.text = "GO!";
            yield return new WaitForSeconds(0.5f);
            countdownText.gameObject.SetActive(false);
        }

        if (lockPlayerDuringStartCountdown && player != null)
        {
            player.canMove = true;
        }

        GameHasStarted = true;
    }

    PlayerController FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return null;

        return playerObject.GetComponent<PlayerController>();
    }

    TMP_Text GetOrCreateStartCountdownText()
    {
        if (startCountdownText != null) return startCountdownText;

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return null;

        GameObject textObject = new GameObject("StartCountdownText");
        textObject.transform.SetParent(canvas.transform, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 96;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.raycastTarget = false;

        RectTransform rectTransform = text.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(300f, 140f);

        startCountdownText = text;
        return startCountdownText;
    }
}
