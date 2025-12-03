using UnityEngine;
using TMPro;
public class Tuomari : MonoBehaviour
{
    public TMP_Text resultText;


    private bool winnerDeclared = false;


    private void Start()
    {
        resultText.text = "";
    }
    private void OnTriggerEnter(Collider car)
    {
        CarIdentify id = car.GetComponent<CarIdentify>();

        string winnerName = id.displayName;
        

        if(id.kind == CarKind.Player)
        {
            var validator = car.GetComponent<PelaajanKierrosTarkastus>();
            if(validator == null)
            {
                Debug.LogError("Puuttuu PelaajanKierrosTarkastus-skripti");
                return;
            }

            if(!validator.AllVisitedThisLap)
            {
                Debug.Log("Player crossed the finish line, but hasn't hit all the checkpoints!");
                return;
            }
        }
    if (winnerDeclared == false)
    {
        winnerDeclared = true;
        resultText.text = $"WINNER: {winnerName}";
        Debug.Log($"WINNER: {winnerName}");
    }
    }
}
