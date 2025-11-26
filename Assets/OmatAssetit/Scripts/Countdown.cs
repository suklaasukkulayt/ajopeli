using UnityEngine;
using TMPro;
using System.Collections;

public class Countdown : MonoBehaviour
{
    public TMP_Text uiText;
    public int countdownFrom = 5;
    public float stepSeconds = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        for(int i = countdownFrom; i < 0; i--)
        {
            uiText.text = i.ToString();
            yield return new WaitForSeconds(stepSeconds);
        }
        

        uiText.text = "GO!";

        GameManager.Instance.Phase = RacePhase.Racing;
    }


}
