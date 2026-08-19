using UnityEngine;
using TMPro;
using System.Collections;

public class Countdown : MonoBehaviour
{
    public TMP_Text uiText;
    public int countdownFrom = 3;
    public float stepSeconds = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        Debug.Log("testi");
        for(int i = countdownFrom; i > 0; i--)
        {
            Debug.Log(i);
            uiText.text = i.ToString();
            yield return new WaitForSeconds(stepSeconds);
        }
        

        uiText.text = "GO!";
        yield return new WaitForSeconds(0.5f);
        uiText.text = "";
        GameManager.Instance.Phase = RacePhase.Racing;
    }


}
