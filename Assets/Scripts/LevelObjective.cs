
using UnityEngine;
using System.Collections;

public class LevelObjective : MonoBehaviour
{
    public static LevelObjective Instance;

    [Header("Victory Settings")]
    [SerializeField] private float victoryDelay = 1f;

    private int remainingCameras;
    private bool victoryStarted;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CameraHealth[] cameras =
            FindObjectsByType<CameraHealth>(FindObjectsSortMode.None);

        remainingCameras = cameras.Length;

    }

    public void CameraDestroyed()
    {
        if (victoryStarted)
            return;

        remainingCameras--;

        if (remainingCameras < 0)
            remainingCameras = 0;

        if (remainingCameras == 0)
        {
            victoryStarted = true;
            StartCoroutine(VictoryRoutine());
        }
    }

    private IEnumerator VictoryRoutine()
    {

        yield return new WaitForSeconds(victoryDelay);

        if (LevelStatistics.Instance != null)
        {
            LevelStatistics.Instance.FinishLevel();
        }
    }
}
