
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenerationPerSecond = 0f;

    private float currentStamina;

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;

    private void Start()
    {
        currentStamina = maxStamina;
        UpdateUI();
    }

    private void Update()
    {
        Regenerate();
    }
    public void Restore(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        UpdateUI();

        Debug.Log("Выносливость восстановлена: +" + amount);
    }
    public bool TrySpend(float amount)
    {
        if (currentStamina < amount)
        {
            Debug.Log("Недостаточно выносливости!");
            return false;
        }

        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        UpdateUI();
        return true;
    }

    private void Regenerate()
    {
        if (currentStamina >= maxStamina)
            return;

        currentStamina += regenerationPerSecond * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateStamina(
                currentStamina,
                maxStamina
            );
        }
    }
}
