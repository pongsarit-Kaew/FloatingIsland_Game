using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFinish : MonoBehaviour
{
    [Tooltip("Coins required per level number. Level 1 = 3, Level 2 = 6, Level 3 = 9, Level 4 = 12.")]
    public int coinsPerLevel = 3;

    [Tooltip("Use this only when a level needs a custom coin requirement. Keep 0 to auto-calculate.")]
    public int requiredCoinsOverride = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.LogWarning("PlayerController is missing on Player.");
            return;
        }

        int requiredCoins = GetRequiredCoins();
        if (player.coinCount < requiredCoins)
        {
            Debug.Log($"Need {requiredCoins} coins to finish this level. Current coins: {player.coinCount}");
            return;
        }

        Debug.Log("Level complete. Saving progress...");
        UnlockNextLevel();
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

    void UnlockNextLevel()
    {
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        int nextLevelIndex = currentLevelIndex + 1;
        int highestUnlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);

        if (nextLevelIndex > highestUnlocked)
        {
            PlayerPrefs.SetInt("UnlockedLevel", nextLevelIndex);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene(0);
    }
}