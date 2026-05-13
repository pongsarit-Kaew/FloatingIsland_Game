using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFinish : MonoBehaviour
{
    [Tooltip("Coins required per level number. Level 1 = 3, Level 2 = 6, Level 3 = 9, Level 4 = 12.")]
    public int coinsPerLevel = 3;

    [Tooltip("Use this only when a level needs a custom coin requirement. Keep 0 to auto-calculate.")]
    public int requiredCoinsOverride = 0;

    [Tooltip("Bonus coins after finishing the level. Level 1 = 1, Level 2 = 2, Level 3 = 3, Level 4 = 4.")]
    public int bonusCoinsPerLevel = 1;

    [Tooltip("Use this only when a level needs a custom bonus. Keep 0 to auto-calculate.")]
    public int bonusCoinsOverride = 0;

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
        GiveLevelCompleteBonus(player);
        UnlockNextLevel();

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowLevelComplete(player);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
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
    }

    int GetBonusCoins()
    {
        if (bonusCoinsOverride > 0)
        {
            return bonusCoinsOverride;
        }

        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        return currentLevelIndex * bonusCoinsPerLevel;
    }

    void GiveLevelCompleteBonus(PlayerController player)
    {
        int bonusCoins = GetBonusCoins();
        player.AddCoin(bonusCoins, true);

        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        totalCoins += bonusCoins;
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();

        Debug.Log($"Level complete bonus: +{bonusCoins} coins. Total bonus coins: {totalCoins}");
    }
}
