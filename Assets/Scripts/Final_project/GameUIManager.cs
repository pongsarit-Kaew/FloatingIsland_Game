using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }
    public bool StartConfirmed { get; private set; }

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject completePanel;

    [Header("HUD Text")]
    public TMP_Text levelText;
    public TMP_Text waveText;
    public TMP_Text completeText;

    private bool isPaused;
    private bool levelEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        StartConfirmed = startPanel == null;
    }

    private void Start()
    {
        ShowStartScreen();
    }

    private void Update()
    {
        if (!StartConfirmed || levelEnded) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ShowStartScreen()
    {
        SetPanel(startPanel, startPanel != null);
        SetPanel(pausePanel, false);
        SetPanel(gameOverPanel, false);
        SetPanel(completePanel, false);

        Time.timeScale = startPanel != null ? 0f : 1f;
    }

    public void StartLevel()
    {
        StartConfirmed = true;
        Time.timeScale = 1f;
        SetPanel(startPanel, false);
    }

    public void PauseGame()
    {
        if (levelEnded) return;

        isPaused = true;
        Time.timeScale = 0f;
        SetPanel(pausePanel, true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SetPanel(pausePanel, false);
    }

    public void ShowGameOver()
    {
        levelEnded = true;
        Time.timeScale = 0f;
        SetPanel(gameOverPanel, true);
    }

    public void ShowLevelComplete(PlayerController player)
    {
        levelEnded = true;
        Time.timeScale = 0f;

        if (completeText != null && player != null)
        {
            completeText.text = $"Level Complete\nCoin: {player.coinCount}\nBonus: {player.bonusCoinCount}";
        }

        SetPanel(completePanel, true);
    }

    public void SetLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }
    }

    public void SetWave(int currentWave, int totalWaves)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave {currentWave}/{totalWaves}";
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    private void SetPanel(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
    }
}
