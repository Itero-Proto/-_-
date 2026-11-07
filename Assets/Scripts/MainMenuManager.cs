
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject mainButtons;
    [SerializeField] private GameObject volumeSettings;

    [Header("Master Volume")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TMP_Text volumeValueText;

    [Header("Level Selection")]
    [SerializeField] private GameObject levelSelectPanel;

    [Header("Level Buttons")]
    [SerializeField] private Button[] levelButtons = new Button[5];
    [SerializeField] private TMP_Text[] levelButtonTexts = new TMP_Text[5];

    [Header("Secret Level")]
    [SerializeField] private Button secretLevelButton;
    [SerializeField] private TMP_Text secretLevelButtonText;

    [Header("Scene Names")]
    [SerializeField]
    private string[] levelSceneNames =
    {
        "CombatPrototype",
        "Level2",
        "Level3",
        "Level4",
        "Level5"
    };

    [SerializeField] private string secretLevelSceneName = "SecretLevel";

    private string[] russianLevelNames =
    {
        "1. Обучение",
        "2. Две камеры",
        "3. Три камеры",
        "4. Сложный уровень",
        "5. Финал"
    };

    private string[] englishLevelNames =
    {
        "1. Tutorial",
        "2. Two Cameras",
        "3. Three Cameras",
        "4. Hard Level",
        "5. Final Level"
    };

    private bool IsEnglish =>
        LanguageSettings.CurrentLanguage == GameLanguage.English;

    private void Start()
    {
        SetupLevelButtons();
        SetupVolumeSlider();
        ShowMainMenu();
    }

    private void OnDestroy()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(ChangeMasterVolume);
    }

    // ---------------- MAIN MENU ----------------

    public void ShowMainMenu()
    {
        mainButtons.SetActive(true);
        volumeSettings.SetActive(true);
        levelSelectPanel.SetActive(false);
    }

    public void OpenLevelSelect()
    {
        mainButtons.SetActive(false);
        volumeSettings.SetActive(false);
        levelSelectPanel.SetActive(true);

        RefreshLevelButtons();
    }

    public void BackToMainMenu()
    {
        ShowMainMenu();
    }

    public void StartNewGame()
    {
        LoadScene("CombatPrototype_tutor");
    }

    public void QuitGame()
    {
        Debug.Log(IsEnglish ? "Exit game" : "Выход из игры");
        Application.Quit();
    }

    // ---------------- MASTER VOLUME ----------------

    private void SetupVolumeSlider()
    {
        if (masterVolumeSlider == null)
        {
            Debug.LogError("Master Volume Slider не назначен!");
            return;
        }

        float savedVolume = AudioVolumeSettings.GetVolume();

        masterVolumeSlider.value = savedVolume;

        masterVolumeSlider.onValueChanged.RemoveListener(ChangeMasterVolume);
        masterVolumeSlider.onValueChanged.AddListener(ChangeMasterVolume);

        UpdateVolumeText(savedVolume);
    }

    private void ChangeMasterVolume(float value)
    {
        AudioVolumeSettings.SetVolume(value);
        UpdateVolumeText(value);
    }

    private void UpdateVolumeText(float value)
    {
        if (volumeValueText != null)
            volumeValueText.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    // ---------------- LEVEL BUTTONS ----------------

    private void SetupLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null)
                continue;

            int levelNumber = i + 1;

            levelButtons[i].onClick.RemoveAllListeners();

            levelButtons[i].onClick.AddListener(
                () => LoadSelectedLevel(levelNumber)
            );
        }

        if (secretLevelButton != null)
        {
            secretLevelButton.onClick.RemoveAllListeners();
            secretLevelButton.onClick.AddListener(LoadSecretLevel);
        }
    }

    private void RefreshLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;

            if (levelButtons[i] == null)
                continue;

            bool unlocked = GameProgress.IsLevelUnlocked(levelNumber);

            levelButtons[i].interactable = unlocked;

            if (levelButtonTexts[i] != null)
            {
                string rank = GameProgress.GetBestRank(levelNumber);

                string levelName = IsEnglish
                    ? englishLevelNames[i]
                    : russianLevelNames[i];

                string status = unlocked
                    ? ""
                    : IsEnglish ? " — LOCKED" : " — ЗАКРЫТ";

                string rankText = rank == "—"
                    ? IsEnglish ? " — NOT COMPLETED" : " — НЕ ПРОЙДЕН"
                    : IsEnglish ? " — RANK: " + rank : " — РАНГ: " + rank;

                levelButtonTexts[i].text =
                    levelName + (unlocked ? rankText : status);
            }
        }

        bool secretUnlocked = GameProgress.IsSecretLevelUnlocked();

        if (secretLevelButton != null)
            secretLevelButton.interactable = secretUnlocked;

        if (secretLevelButtonText != null)
        {
            secretLevelButtonText.text = secretUnlocked
                ? IsEnglish
                    ? "SECRET LEVEL — AVAILABLE"
                    : "СЕКРЕТНЫЙ УРОВЕНЬ — ДОСТУПЕН"
                : IsEnglish
                    ? "SECRET LEVEL — LOCKED"
                    : "СЕКРЕТНЫЙ УРОВЕНЬ — ЗАКРЫТ";
        }
    }

    private void LoadSelectedLevel(int levelNumber)
    {
        if (!GameProgress.IsLevelUnlocked(levelNumber))
            return;

        int index = levelNumber - 1;

        if (index < 0 || index >= levelSceneNames.Length)
            return;

        LoadScene(levelSceneNames[index]);
    }

    private void LoadSecretLevel()
    {
        if (!GameProgress.IsSecretLevelUnlocked())
            return;

        LoadScene(secretLevelSceneName);
    }

    private void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
