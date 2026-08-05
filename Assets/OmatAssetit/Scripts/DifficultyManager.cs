using UnityEngine;

public enum Difficulty { Easy, Normal, Hard }

// Säilyy DontDestroyOnLoad:n ansiosta scenevaihdon yli (vaikeustasovalitsin -> Game),
// samaan tapaan kuin GameManager. Tämä EI voi olla GameManagerin sisällä, koska
// GameManager elää vain Game-scenessä eikä ole vielä olemassa valitsinvaiheessa.
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    // Normal oletuksena, jos Game-sceneä testataan suoraan ilman valitsinta.
    public Difficulty SelectedDifficulty { get; set; } = Difficulty.Normal;

    // Oletus-FOV, jos Game-sceneä testataan suoraan ilman valitsinta.
    public float SelectedFov { get; set; } = 60f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}