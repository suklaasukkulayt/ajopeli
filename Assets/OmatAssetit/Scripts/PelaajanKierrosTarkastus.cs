using UnityEngine;
using TMPro;
public class PelaajanKierrosTarkastus : MonoBehaviour
{
    public int checkpointCount = 11;
    public TMP_Text lapsText;
    private bool[] visited;
    private int visitedCount;
    void Awake()
    {
        ResetLap();
        lapsText.text = $"LAPS: 0/3";
    }


    public void UpdateLapsText(int currentLap, int maxLap)
    {
        Debug.Log("testi1");
        lapsText.text = $"LAPS: {currentLap}/{maxLap}";
    }
    public void ResetLap()
    {
        visited = new bool[checkpointCount];
        visitedCount = 0;
    }

    public void MarkVisited(int index)
    {
        if(!visited[index])
        {
            visited[index] = true;
            visitedCount++;
        }
    }


    public bool AllVisitedThisLap => visitedCount == checkpointCount;
}
