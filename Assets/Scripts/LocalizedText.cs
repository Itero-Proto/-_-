
using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string russianText;
    [SerializeField] private string englishText;

    private TMP_Text textComponent;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        LanguageSettings.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    private void OnDisable()
    {
        LanguageSettings.OnLanguageChanged -= UpdateText;
    }

    public void UpdateText()
    {
        if (textComponent == null)
            return;

        textComponent.text =
            LanguageSettings.CurrentLanguage == GameLanguage.Russian
            ? russianText
            : englishText;
    }
}
