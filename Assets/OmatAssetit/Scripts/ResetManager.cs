using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetManager : MonoBehaviour
{
    public void RestartCurrentScene()
    {
        SceneManager.LoadScene("StartScreen");
    }

    void Update()
    {
        // KeyCode.Return on tavallinen Enter ja KeyCode.KeypadEnter on numeronäppäimistön Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            RestartCurrentScene();
        }
    }
}