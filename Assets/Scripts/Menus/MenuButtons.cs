using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Resume()
    {
        GameManager.Instance.activeCanvas.SetActive(false);
        GameManager.Instance.Unpause();
    }

    public void Restart()
    {
        GameManager.Instance.RestartGame();
    }
    public void Options()
    {
        GameManager.Instance.OptionsMenu();
    }

    public void Quit()
    {
        // TODO: Return to the main menu for WebGL builds


        // For standalone builds, quit the application
        Application.Quit();
    }
}
