using UnityEngine;
using UnityEngine.SceneManagement; // Tarvitaan skenojen vaihtamiseen

public class StartMenuManager : MonoBehaviour
{
    void Update()
    {
        // Input.anyKeyDown tunnistaa minkä tahansa näppäimistön tai hiiren napin painalluksen
        if (Input.anyKeyDown)
        {
            // Ladataan vaikeustasovalitsin Gamen sijaan -- valitsin lataa itse Gamen
            // kun pelaaja on valinnut vaikeustason (A/D valitsee, W vahvistaa).
            SceneManager.LoadScene("DifficultySelect");
        }
    }
}