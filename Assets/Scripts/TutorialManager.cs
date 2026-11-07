
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
    [SerializeField] private string[] messages;

    private int currentMessageIndex;
    private bool waitingForParry;
    private bool tutorialFinished;

    private void Start()
    {
        currentMessageIndex = 0;
        ShowMessage();
    }

    private void OnEnable()
    {
        if (playerParry != null)
            playerParry.OnSuccessfulParry += HandleSuccessfulParry;
    }

    private void OnDisable()
    {
        if (playerParry != null)
            playerParry.OnSuccessfulParry -= HandleSuccessfulParry;

        if (tutorialCamera != null)
            tutorialCamera.OnFirstAttackStarted -= HandleFirstAttackStarted;
    }

    private void ShowMessage()
    {
        if (messages == null || messages.Length == 0)
        {
            EndTutorial();
            return;
        }

        tutorialPanel.SetActive(true);
        nextButton.gameObject.SetActive(true);

        tutorialText.text = messages[currentMessageIndex];

        if (currentMessageIndex == messages.Length - 1)
        {
            SetButtonAction(EndTutorial, "Начать!");
        }
        else
        {
            SetButtonAction(NextMessage, "Далее");
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

        if (currentMessageIndex >= messages.Length)
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

        Debug.Log("Вводная часть туториала завершена!");
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

        tutorialText.text =
            "Камера готовится стрелять!\n\nНажми SPACE, чтобы парировать лазер.";

        nextButton.gameObject.SetActive(false);
    }

    private void HandleSuccessfulParry(bool isPerfect)
    {
        if (!waitingForParry)
            return;

        waitingForParry = false;

        tutorialPanel.SetActive(true);
        tutorialText.text =
            "Отлично! Ты отразил лазер!\n\nТеперь отражённый лазер может уничтожить камеру.";

        SetButtonAction(ContinueAfterParry, "Далее");

        Debug.Log("Туториал: успешное парирование!");
    }

    private void ContinueAfterParry()
    {
        tutorialPanel.SetActive(false);

        Debug.Log("Туториал: переходим к следующему уроку.");
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveAllListeners();
    }
}
