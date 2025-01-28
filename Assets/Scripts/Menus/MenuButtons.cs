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
        //#if UNITY_WEBGL
        //GameManager.Instance.RestartGame();
        //#else
        //Application.Quit();
    }
}
