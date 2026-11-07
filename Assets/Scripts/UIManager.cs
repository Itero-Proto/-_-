
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private ParryFeedback parryFeedback;

    [Header("Player UI")]
    [SerializeField] private Slider playerHealthBar;
    [SerializeField] private TMP_Text playerHealthText;

    [Header("Stamina UI")]
    [SerializeField] private Slider staminaBar;

    [Header("Result Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool gameEnded;

    private void Awake()
    {
        Instance = this;

        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    public void UpdateStamina(float current, float max)
    {
        if (staminaBar != null)
        {
            staminaBar.maxValue = max;
            staminaBar.value = current;
        }
    }

    public void UpdatePlayerHealth(int current, int max)
    {
        if (playerHealthBar != null)
        {
            playerHealthBar.maxValue = max;
            playerHealthBar.value = current;
        }

        if (playerHealthText != null)
            playerHealthText.text = current + "/" + max;
    }

    public void ShowGameOver()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;

        Debug.Log("GAME OVER!");
    }

    public void ShowVictory()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ShowNormalParry()
    {
        if (parryFeedback != null)
            parryFeedback.ShowNormalParry();
    }

    public void ShowPerfectParry()
    {
        if (parryFeedback != null)
            parryFeedback.ShowPerfectParry();
    }
}
