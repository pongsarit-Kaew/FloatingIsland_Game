using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoinUI : MonoBehaviour
{
    public TMP_Text coinText;
    public TMP_Text bonusCoinText;
    public bool showBonusOnCoinText = false;
    public PlayerController player;
    public int coinsPerLevel = 3;
    public int requiredCoinsOverride = 0;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.GetComponent<PlayerController>();
            }
        }

        UpdateCoinText();
    }

    void Update()
    {
        UpdateCoinText();
    }

    void UpdateCoinText()
    {
        if (coinText == null || player == null) return;

        int requiredCoins = GetRequiredCoins();
        int bonusCoins = player.bonusCoinCount;

        if (showBonusOnCoinText)
        {
            coinText.text = $"Coin: {player.coinCount}/{requiredCoins}\nBonus: {bonusCoins}";
        }
        else
        {
            coinText.text = $"Coin: {player.coinCount}/{requiredCoins}";
        }

        if (bonusCoinText != null)
        {
            bonusCoinText.text = $"Bonus: {bonusCoins}";
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
}
