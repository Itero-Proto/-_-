
using UnityEngine;
using System;

public enum GameLanguage
{
    Russian,
    English
}

public static class LanguageSettings
{
    private const string LanguageKey = "GameLanguage";

    public static GameLanguage CurrentLanguage { get; private set; } =
        (GameLanguage)PlayerPrefs.GetInt(LanguageKey, 0);

    public static event Action OnLanguageChanged;

    public static void ToggleLanguage()
    {
        CurrentLanguage = CurrentLanguage == GameLanguage.Russian
            ? GameLanguage.English
            : GameLanguage.Russian;

        PlayerPrefs.SetInt(LanguageKey, (int)CurrentLanguage);
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke();
    }
}
