using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    public TMP_Text text;              // Raahaa Inspectorista TMP-tekstiobjekti
    public string message = "Press any button to start";
    public float interval = 1f;        // sekunteina

    private bool showing = false;

    void OnEnable()
    {
        InvokeRepeating(nameof(ToggleText), 0f, interval);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(ToggleText));
    }

    void ToggleText()
    {
        showing = !showing;
        text.text = showing ? message : "";
    }
}