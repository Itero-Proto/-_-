
using TMPro;
using UnityEngine;

public class LanguageButton : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;

    private void Start()
    {
        UpdateButtonText();
    }

    public void ChangeLanguage()
    {
        LanguageSettings.ToggleLanguage();
        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        if (buttonText == null)
            return;

        buttonText.text =
            LanguageSettings.CurrentLanguage == GameLanguage.Russian
            ? "ЯЗЫК: RU"
            : "LANGUAGE: EN";
    }
}
