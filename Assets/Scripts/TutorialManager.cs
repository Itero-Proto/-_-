
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField] private SecurityCamera tutorialCamera;
    [SerializeField] private PlayerParry playerParry;

    [Header("UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private Button nextButton;

    [Header("Tutorial Messages")]
    [TextArea(3, 5)]
    [SerializeField] private string[] russianMessages;

    [TextArea(3, 5)]
    [SerializeField] private string[] englishMessages;

    private int currentMessageIndex;
    private bool waitingForParry;
    private bool tutorialFinished;

    private bool IsEnglish =>
        LanguageSettings.CurrentLanguage == GameLanguage.English;

    private string[] CurrentMessages =>
        IsEnglish ? englishMessages : russianMessages;

    private void Start()
    {
        currentMessageIndex = 0;
        ShowMessage();
    }

    private void OnEnable()
    {
        if (playerParry != null)
            playerParry.OnSuccessfulParry += HandleSuccessfulParry;

        LanguageSettings.OnLanguageChanged += UpdateCurrentMessage;
    }

    private void OnDisable()
    {
        if (playerParry != null)
            playerParry.OnSuccessfulParry -= HandleSuccessfulParry;

        if (tutorialCamera != null)
            tutorialCamera.OnFirstAttackStarted -= HandleFirstAttackStarted;

        LanguageSettings.OnLanguageChanged -= UpdateCurrentMessage;
    }

    private void ShowMessage()
    {
        string[] messages = CurrentMessages;

        if (messages == null || messages.Length == 0)
        {
            EndTutorial();
            return;
        }

        currentMessageIndex = Mathf.Clamp(
            currentMessageIndex, 0, messages.Length - 1);

        tutorialPanel.SetActive(true);
        nextButton.gameObject.SetActive(true);

        tutorialText.text = messages[currentMessageIndex];

        if (currentMessageIndex == messages.Length - 1)
        {
            SetButtonAction(EndTutorial, IsEnglish ? "Start!" : "Начать!");
        }
        else
        {
            SetButtonAction(NextMessage, IsEnglish ? "Next" : "Далее");
        }
    }

    private void SetButtonAction(
        UnityEngine.Events.UnityAction action,
        string buttonText)
    {
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(action);

        nextButton.GetComponentInChildren<TMP_Text>().text = buttonText;
    }

    private void NextMessage()
    {
        currentMessageIndex++;

        if (currentMessageIndex >= CurrentMessages.Length)
        {
            EndTutorial();
            return;
        }

        ShowMessage();
    }

    private void EndTutorial()
    {
        if (tutorialFinished)
            return;

        tutorialFinished = true;
        tutorialPanel.SetActive(false);

        if (tutorialCamera != null)
        {
            tutorialCamera.OnFirstAttackStarted += HandleFirstAttackStarted;
            tutorialCamera.ActivateCamera();
        }
    }

    private void HandleFirstAttackStarted()
    {
        if (waitingForParry || !tutorialFinished)
            return;

        tutorialCamera.OnFirstAttackStarted -= HandleFirstAttackStarted;

        StartParryLesson();
    }

    private void StartParryLesson()
    {
        waitingForParry = true;

        tutorialPanel.SetActive(true);

        tutorialText.text = IsEnglish
            ? "The camera is preparing to shoot!\n\nPress SPACE to parry the laser."
            : "Камера готовится стрелять!\n\nНажми SPACE, чтобы парировать лазер.";

        nextButton.gameObject.SetActive(false);
    }

    private void HandleSuccessfulParry(bool isPerfect)
    {
        if (!waitingForParry)
            return;

        waitingForParry = false;

        tutorialPanel.SetActive(true);

        tutorialText.text = IsEnglish
            ? "Great! You reflected the laser!\n\nNow the reflected laser can destroy the camera."
            : "Отлично! Ты отразил лазер!\n\nТеперь отражённый лазер может уничтожить камеру.";

        SetButtonAction(ContinueAfterParry, IsEnglish ? "Next" : "Далее");

        Debug.Log(IsEnglish
            ? "Tutorial: successful parry!"
            : "Туториал: успешное парирование!");
    }

    private void ContinueAfterParry()
    {
        tutorialPanel.SetActive(false);

        Debug.Log(IsEnglish
            ? "Tutorial: moving to the next lesson."
            : "Туториал: переходим к следующему уроку.");
    }

    private void UpdateCurrentMessage()
    {
        if (!tutorialPanel.activeSelf)
            return;

        if (!tutorialFinished)
        {
            ShowMessage();
            return;
        }

        if (waitingForParry)
        {
            StartParryLesson();
            return;
        }

        tutorialText.text = IsEnglish
            ? "Great! You reflected the laser!\n\nNow the reflected laser can destroy the camera."
            : "Отлично! Ты отразил лазер!\n\nТеперь отражённый лазер может уничтожить камеру.";

        SetButtonAction(ContinueAfterParry, IsEnglish ? "Next" : "Далее");
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveAllListeners();
    }
}
