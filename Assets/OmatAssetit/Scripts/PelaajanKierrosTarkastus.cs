using UnityEngine;

public class PelaajanKierrosTarkastus : MonoBehaviour
{
    public int checkpointCount = 11;
    private bool[] visited;
    private int visitedCount;
    void Awake()
    {
        ResetLap();
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
