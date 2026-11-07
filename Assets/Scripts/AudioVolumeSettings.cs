
using UnityEngine;

public static class AudioVolumeSettings
{
    private const string VolumeKey = "MasterVolume";

    // Загружаем громкость до загрузки первой сцены.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadSavedVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 0.8f);

        AudioListener.volume = savedVolume;
    }

    public static float GetVolume()
    {
        return PlayerPrefs.GetFloat(VolumeKey, 0.8f);
    }

    public static void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        AudioListener.volume = volume;

        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }
}
