
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelStatistics : MonoBehaviour
{
    [Header("Level Navigation")]
    [SerializeField] private string nextSceneName = "Level2";

    public static LevelStatistics Instance;

    [Header("Parry Statistics")]
    public int perfectParries;
    public int normalParries;
    public int failedParries;

    [Header("Combat Statistics")]
    public int damageTaken;

    [Header("Level Progress")]
    [SerializeField] private int levelNumber = 1;

    [Header("Results UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text perfectParriesText;
    [SerializeField] private TMP_Text normalParriesText;
    [SerializeField] private TMP_Text failedParriesText;
    [SerializeField] private TMP_Text damageTakenText;

    private bool levelFinished;

    private bool IsEnglish =>
        LanguageSettings.CurrentLanguage == GameLanguage.English;

    private void Awake()
    {
        Instance = this;

        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    public void RecordPerfectParry()
    {
        if (levelFinished) return;
        perfectParries++;
    }

    public void RecordNormalParry()
    {
        if (levelFinished) return;
        normalParries++;
    }

    public void RecordFailedParry()
    {
        if (levelFinished) return;
        failedParries++;
    }

    public void RecordDamage()
    {
        if (levelFinished) return;
        damageTaken++;
    }

    public void FinishLevel()
    {
        if (levelFinished)
            return;

        levelFinished = true;

        string finalRank = CalculateRank();

        GameProgress.CompleteLevel(levelNumber, finalRank);

        UpdateResultsUI();

        if (resultPanel != null)
            resultPanel.SetActive(true);

        Debug.Log("===== LEVEL STATISTICS =====");
        Debug.Log("Perfect parries: " + perfectParries);
        Debug.Log("Normal parries: " + normalParries);
        Debug.Log("Failed attempts: " + failedParries);
        Debug.Log("Damage taken: " + damageTaken);

        Time.timeScale = 0f;
    }

    private void UpdateResultsUI()
    {
        if (perfectParriesText != null)
            perfectParriesText.text = IsEnglish
                ? "Perfect parries: " + perfectParries
                : "Идеальные парирования: " + perfectParries;

        if (normalParriesText != null)
            normalParriesText.text = IsEnglish
                ? "Normal parries: " + normalParries
                : "Обычные парирования: " + normalParries;

        if (failedParriesText != null)
            failedParriesText.text = IsEnglish
                ? "Failed attempts: " + failedParries
                : "Неудачные попытки: " + failedParries;

        if (damageTakenText != null)
            damageTakenText.text = IsEnglish
                ? "Damage taken: " + damageTaken
                : "Получено урона: " + damageTaken;

        if (rankText != null)
            rankText.text = IsEnglish
                ? "RANK: " + CalculateRank()
                : "РАНГ: " + CalculateRank();
    }

    private string CalculateRank()
    {
        int totalAttempts =
            perfectParries + normalParries + failedParries;

        if (totalAttempts == 0)
            return "C";

        if (perfectParries == totalAttempts && damageTaken == 0)
            return "S";

        float successRate =
            (float)(perfectParries + normalParries) / totalAttempts * 100f;

        if (successRate >= 80f && damageTaken <= 1)
            return "A";

        if (successRate >= 60f && damageTaken <= 2)
            return "B";

        return "C";
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }
}
