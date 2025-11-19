using UnityEngine;

public enum RacePhase{Countdown, Racing, Finished}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public RacePhase Phase{ get; set; } = RacePhase.Countdown;
    

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
