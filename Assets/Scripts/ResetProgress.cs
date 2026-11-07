
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetProgress : MonoBehaviour
{
    [SerializeField] private GameObject resetConfirmPanel;

    public void ShowConfirmation()
    {
        resetConfirmPanel.SetActive(true);
    }

    public void ConfirmReset()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CancelReset()
    {
        resetConfirmPanel.SetActive(false);
    }
}
