using UnityEngine;
using TMPro;
using System.Collections;

public class PelaajanKierrosTarkastus : MonoBehaviour
{
    public int checkpointCount = 11;
    public TMP_Text lapsText;

    [Tooltip("Kuinka pitkään 'You didn't hit all checkpoints!' -viesti näkyy ennen kuin kierrosteksti palautuu.")]
    public float missedCheckpointMessageDuration = 2f;

    [Tooltip("Kuinka pitkään ONNISTUNEEN kierroksen jälkeen varoitusviestiä ei näytetä, vaikka maaliviiva ylitettäisiin vahingossa uudestaan (esim. pakittamalla) -- ettei se aiheuta turhaa ihmetystä.")]
    public float missedCheckpointSuppressionAfterLap = 5f;

    private bool[] visited;
    private int visitedCount;

    // Muistetaan viimeksi näytetyt kierrosluvut, jotta väliaikaisen varoitusviestin
    // jälkeen osataan palauttaa oikea "LAPS: x/y" -teksti sen sijaan että se unohtuisi.
    private int lastDisplayedLap = 0;
    private int lastDisplayedMaxLap = 3;
    private Coroutine missedCheckpointRoutine;

    // Ajanhetki jolloin viimeisin ONNISTUNUT kierros rekisteröitiin (Time.time).
    // Alustetaan kauas menneisyyteen, ettei se estä ihan ensimmäistä varoitusta.
    private float lastSuccessfulLapTime = -999f;

    void Awake()
    {
        ResetLap();
        UpdateLapsTextDisplay();
    }

    public void UpdateLapsText(int currentLap, int maxLap)
    {
        lastDisplayedLap = currentLap;
        lastDisplayedMaxLap = maxLap;

        // Jos varoitusviesti on parhaillaan näkyvissä, ei kirjoiteta sen päälle --
        // se palauttaa oikean tekstin itse kun sen aika loppuu.
        if (missedCheckpointRoutine == null)
        {
            UpdateLapsTextDisplay();
        }
    }

    private void UpdateLapsTextDisplay()
    {
        if (lapsText == null) return;
        lapsText.text = $"LAPS: {lastDisplayedLap}/{lastDisplayedMaxLap}";
    }

    // Kutsutaan Tuomari.cs:stä kun pelaaja ylitti maalin muttei osunut kaikkiin
    // checkpointeihin sillä kierroksella.
    public void ShowMissedCheckpointsMessage()
    {
        if (lapsText == null) return;

        if (missedCheckpointRoutine != null)
        {
            StopCoroutine(missedCheckpointRoutine);
        }
        missedCheckpointRoutine = StartCoroutine(MissedCheckpointsRoutine());
    }

    private IEnumerator MissedCheckpointsRoutine()
    {
        lapsText.text = "You didn't hit all checkpoints!";
        yield return new WaitForSeconds(missedCheckpointMessageDuration);
        UpdateLapsTextDisplay();
        missedCheckpointRoutine = null;
    }

    public void ResetLap()
    {
        visited = new bool[checkpointCount];
        visitedCount = 0;
    }

    public void MarkVisited(int index)
    {
        if (!visited[index])
        {
            visited[index] = true;
            visitedCount++;
        }
    }

    public bool AllVisitedThisLap => visitedCount == checkpointCount;
}