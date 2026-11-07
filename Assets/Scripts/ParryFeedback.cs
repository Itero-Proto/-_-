
using System.Collections;
using TMPro;
using UnityEngine;

public class ParryFeedback : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text feedbackText;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 0.8f;
    [SerializeField] private float perfectDisplayDuration = 1.1f;

    private Coroutine currentCoroutine;

    public void ShowNormalParry()
    {
        Show("PARRY!", Color.cyan, displayDuration);
    }

    public void ShowPerfectParry()
    {
        Show("PERFECT PARRY!  +1 HP", Color.yellow,
            perfectDisplayDuration);
    }

    private void Show(string message, Color color, float duration)
    {
        if (feedbackText == null)
            return;

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(
            ShowRoutine(message, color, duration)
        );
    }

    private IEnumerator ShowRoutine(
        string message,
        Color color,
        float duration)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = color;

        feedbackText.alpha = 1f;

        yield return new WaitForSecondsRealtime(duration);

        feedbackText.gameObject.SetActive(false);
        currentCoroutine = null;
    }
}
