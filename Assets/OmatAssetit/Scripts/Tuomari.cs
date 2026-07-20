using UnityEngine;
using TMPro;
public class Tuomari : MonoBehaviour
{
    public TMP_Text resultText;

    public int kierrostenMaara = 3;
    private bool winnerDeclared = false;


    private void Start()
    {
        resultText.text = "";
    }
    private void OnTriggerEnter(Collider car)
    {
        CarIdentify id = car.GetComponent<CarIdentify>();

        if(id == null)
        {
            return;
        }


        LapCounter lap = car.GetComponent<LapCounter>();


        //string winnerName = id.displayName;


        if (id.kind == CarKind.Player)
        {
            var validator = car.GetComponent<PelaajanKierrosTarkastus>();
            if (validator == null)
            {
                Debug.LogError("Puuttuu PelaajanKierrosTarkastus-skripti");
                return;
            }

            if (!validator.AllVisitedThisLap)
            {
                Debug.Log("Player crossed the finish line, but hasn't hit all the checkpoints!");
                return;
            }
            int tmpLap = lap.lapsCompleted;
            validator.UpdateLapsText(tmpLap + 1, kierrostenMaara);
            validator.ResetLap();
        }
        lap.lapsCompleted++;


    if (winnerDeclared == false && lap.lapsCompleted >= kierrostenMaara)
     {
        string winnerName = id.displayName;
        winnerDeclared = true;
        resultText.text = $"<mark>WINNER: {winnerName}</mark>";
        GameManager.Instance.Phase = RacePhase.Finished;
        Debug.Log($"WINNER: {winnerName}");
        
    }
    }
}
