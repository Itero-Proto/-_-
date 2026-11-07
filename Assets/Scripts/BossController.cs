using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    [Header("Boss Health")]
    [SerializeField] private int maxHealth = 15;

    [Header("Boss Parts")]
    [SerializeField] private GameObject[] cameraParts;

    private int currentHealth;
    private bool isDefeated;

    private void Start()
    {
        currentHealth = maxHealth;

        Debug.Log("БОСС: бой начался! Здоровье: " + currentHealth);
    }

    public void TakeDamage()
    {
        if (isDefeated)
            return;

        currentHealth--;

        if (currentHealth < 0)
            currentHealth = 0;

        Debug.Log("БОСС получил урон! Осталось HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            DefeatBoss();
        }
    }


    private void DefeatBoss()
    {
        if (isDefeated)
            return;

        isDefeated = true;

        Debug.Log("БОСС УНИЧТОЖЕН!");

        // Отключаем все пять камер.
        foreach (GameObject cameraPart in cameraParts)
        {
            if (cameraPart != null)
                cameraPart.SetActive(false);
        }

        // Завершаем уровень и показываем результаты.
        if (LevelStatistics.Instance != null)
        {
            LevelStatistics.Instance.FinishLevel();
        }
        else
        {
            Debug.LogError("LevelStatistics не найден!");
        }
    }
}