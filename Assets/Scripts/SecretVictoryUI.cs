
using System.Collections;
using UnityEngine;

public class SecretVictoryUI : MonoBehaviour
{
    [Header("Victory Panel")]
    [SerializeField] private CanvasGroup victoryCanvasGroup;

    [Header("Animation")]
    [SerializeField] private float delayBeforeShow = 2f;
    [SerializeField] private float fadeDuration = 1.5f;

    private bool hasShown;

    public void ShowVictory()
    {
        if (hasShown)
            return;

        hasShown = true;

        StartCoroutine(ShowVictoryRoutine());
    }

    private IEnumerator ShowVictoryRoutine()
    {
        yield return new WaitForSeconds(delayBeforeShow);

        victoryCanvasGroup.gameObject.SetActive(true);

        victoryCanvasGroup.alpha = 0f;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / fadeDuration);

            // Плавное появление с замедлением.
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            victoryCanvasGroup.alpha = smoothProgress;

            yield return null;
        }

        victoryCanvasGroup.alpha = 1f;

        victoryCanvasGroup.interactable = true;
        victoryCanvasGroup.blocksRaycasts = true;
    }
}
