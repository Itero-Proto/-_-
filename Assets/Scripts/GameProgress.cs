
using UnityEngine;

public static class GameProgress
{
    private const int TotalLevels = 5;

    // Возвращает номер самого дальнего открытого уровня.
    public static int GetMaxUnlockedLevel()
    {
        return PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
    }

    // Проверяет, открыт ли обычный уровень.
    public static bool IsLevelUnlocked(int levelNumber)
    {
        return levelNumber <= GetMaxUnlockedLevel();
    }

    // Возвращает лучший ранг уровня.
    public static string GetBestRank(int levelNumber)
    {
        int rank = PlayerPrefs.GetInt("BestRank_Level_" + levelNumber, 0);

        switch (rank)
        {
            case 4: return "S";
            case 3: return "A";
            case 2: return "B";
            case 1: return "C";
            default: return "—";
        }
    }

    // Сохраняет результат и открывает следующий уровень.
    public static void CompleteLevel(int levelNumber, string rank)
    {
        if (levelNumber < 1 || levelNumber > TotalLevels)
            return;

        int newRankValue = RankToValue(rank);
        int oldRankValue = PlayerPrefs.GetInt(
            "BestRank_Level_" + levelNumber, 0);

        // Перезаписываем результат только если новый ранг лучше.
        if (newRankValue > oldRankValue)
        {
            PlayerPrefs.SetInt(
                "BestRank_Level_" + levelNumber, newRankValue);
        }

        // Открываем следующий уровень после победы.
        int maxUnlocked = GetMaxUnlockedLevel();

        if (levelNumber < TotalLevels && levelNumber >= maxUnlocked)
        {
            PlayerPrefs.SetInt("MaxUnlockedLevel", levelNumber + 1);
        }

        PlayerPrefs.Save();
    }

    // Секретный уровень доступен только при пяти рангах S.
    public static bool IsSecretLevelUnlocked()
    {
        for (int i = 1; i <= TotalLevels; i++)
        {
            if (GetBestRank(i) != "S")
                return false;
        }

        return true;
    }

    private static int RankToValue(string rank)
    {
        switch (rank)
        {
            case "S": return 4;
            case "A": return 3;
            case "B": return 2;
            case "C": return 1;
            default: return 0;
        }
    }

    // Сброс прогресса для тестирования.
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey("MaxUnlockedLevel");

        for (int i = 1; i <= TotalLevels; i++)
        {
            PlayerPrefs.DeleteKey("BestRank_Level_" + i);
        }

        PlayerPrefs.Save();
    }
}
