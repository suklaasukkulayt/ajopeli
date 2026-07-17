using UnityEngine;
using UnityEngine.SceneManagement; // Tarvitaan skenojen vaihtamiseen

public class StartMenuManager : MonoBehaviour
{
    void Update()
    {
        // Input.anyKeyDown tunnistaa minkä tahansa näppäimistön tai hiiren napin painalluksen
        if (Input.anyKeyDown)
        {
            // Lataa skenen nimeltä SampleScene
            SceneManager.LoadScene("Game");
        }
    }
}