using UnityEngine;
using TMPro;

public class Tuomari : MonoBehaviour
{
    [Header("Voittoteksti")]
    public TMP_Text resultText;

    [Header("Kierrokset")]
    public int kierrostenMaara = 3;
    public TMP_Text kierros1Text;
    public TMP_Text kierros2Text;
    public TMP_Text kierros3Text;

    [Header("Lopputulos")]
    [Tooltip("Nykyinen LapsText. Voiton jälkeen tähän tulee yhteisaika.")]
    public TMP_Text lapsText;
    [Tooltip("Uusi teksti, joka näyttää parhaan koskaan saadun ajan.")]
    public TMP_Text parasIkinAikaText;

    [Header("Voittokamera (kun PELAAJA voittaa)")]
    [Tooltip("Playerin sisällä oleva kamera. Jätä tyhjäksi niin skripti löytää sen automaattisesti 'Player'-tagatusta objektista.")]
    public Transform victoryCamera;

    [Tooltip("Kuinka korkealla AI-auton yläpuolella kamera leijuu voittokuvassa.")]
    public float victoryCameraHeight = 15f;

    private bool winnerDeclared = false;
    private bool raceTimerStarted = false;
    private float raceStartTime;
    private float previousLapTime;
    private const string BestTimeKey = "Ajopeli_BestTime";

    private void Start()
    {
        winnerDeclared = false;
        raceTimerStarted = false;
        previousLapTime = 0f;

        if (resultText != null)
            resultText.text = "";

        SetLapText(kierros1Text, 1, null);
        SetLapText(kierros2Text, 2, null);
        SetLapText(kierros3Text, 3, null);
        UpdateBestTimeText();

        // Inspectorissa oleva LapsText voi olla sama teksti kuin
        // PelaajanKierrosTarkastus.lapsText. Jos ei ole asetettu,
        // haetaan se automaattisesti Player-objektista.
        if (lapsText == null)
            lapsText = FindLapsText();
    }

    private void Update()
    {
        if (!raceTimerStarted && GameManager.Instance != null && GameManager.Instance.Phase == RacePhase.Racing)
        {
            raceTimerStarted = true;
            raceStartTime = Time.time;
            previousLapTime = 0f;
        }
    }

    private void OnTriggerEnter(Collider car)
    {
        CarIdentify id = car.GetComponent<CarIdentify>();
        if (id == null)
            return;

        LapCounter lap = car.GetComponent<LapCounter>();
        if (lap == null)
        {
            Debug.LogWarning("[Tuomari] Autolta puuttuu LapCounter.");
            return;
        }

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
                validator.ShowMissedCheckpointsMessage();
                return;
            }

            int completedLapNumber = lap.lapsCompleted + 1;

            // Lasketaan hyväksytyn kierroksen aika pelaajalle.
            RecordPlayerLapTime(completedLapNumber);

            validator.UpdateLapsText(completedLapNumber, kierrostenMaara);
            validator.ResetLap();
        }

        lap.lapsCompleted++;

        if (!winnerDeclared && lap.lapsCompleted >= kierrostenMaara)
        {
            string winnerName = id.displayName;
            winnerDeclared = true;

            float totalTime = raceTimerStarted ? Time.time - raceStartTime : 0f;
            string totalTimeString = FormatTime(totalTime);

            if (resultText != null)
                resultText.text = $"<mark>WINNER: {winnerName}</mark>";

            // LapsText näyttää voiton jälkeen yhteisajan eikä enää
            // "Returning to main menu" -tekstiä.
            if (lapsText == null)
                lapsText = FindLapsText();

            if (lapsText != null)
                lapsText.text = $"Race time: {totalTimeString}";

            UpdateBestTime(totalTime);

            if (GameManager.Instance != null)
                GameManager.Instance.Phase = RacePhase.Finished;

            Debug.Log($"WINNER: {winnerName}, Race time: {totalTimeString}");

            if (id.kind == CarKind.Player)
            {
                ShowAiGapCamera();
            }
        }
    }

    private void RecordPlayerLapTime(int lapNumber)
    {
        if (!raceTimerStarted)
        {
            // Turvavarmistus, jos kierrokselle päästään ennen kuin Update ehti
            // käynnistää ajastimen.
            raceTimerStarted = true;
            raceStartTime = Time.time;
            previousLapTime = 0f;
        }

        float totalElapsed = Time.time - raceStartTime;
        float lapTime = totalElapsed - previousLapTime;
        previousLapTime = totalElapsed;

        SetLapTextForNumber(lapNumber, lapTime);
    }

    private void SetLapTextForNumber(int lapNumber, float lapTime)
    {
        TMP_Text target = null;

        switch (lapNumber)
        {
            case 1: target = kierros1Text; break;
            case 2: target = kierros2Text; break;
            case 3: target = kierros3Text; break;
        }

        if (target != null)
            target.text = $"Lap {lapNumber}: {FormatTime(lapTime)}";
    }

    private void SetLapText(TMP_Text target, int lapNumber, float? lapTime)
    {
        if (target == null)
            return;

        target.text = lapTime.HasValue
            ? $"KIERROS {lapNumber}: {FormatTime(lapTime.Value)}"
            : $"KIERROS {lapNumber}: --:--.---";
    }

    private string FormatTime(float seconds)
    {
        if (seconds < 0f)
            seconds = 0f;

        int minutes = Mathf.FloorToInt(seconds / 60f);
        float remainingSeconds = seconds - minutes * 60f;
        return $"{minutes:00}:{remainingSeconds:00.000}";
    }

    private void UpdateBestTime(float totalTime)
    {
        if (totalTime <= 0f)
            return;

        float currentBest = PlayerPrefs.GetFloat(BestTimeKey, -1f);

        if (currentBest < 0f || totalTime < currentBest)
        {
            PlayerPrefs.SetFloat(BestTimeKey, totalTime);
            PlayerPrefs.Save();
            currentBest = totalTime;
        }

        if (parasIkinAikaText != null)
            parasIkinAikaText.text = $"Best time: {FormatTime(currentBest)}";
    }

    private void UpdateBestTimeText()
    {
        if (parasIkinAikaText == null)
            return;

        float currentBest = PlayerPrefs.GetFloat(BestTimeKey, -1f);

        parasIkinAikaText.text = currentBest < 0f
            ? "Best time: --:--.---"
            : $"Best time: {FormatTime(currentBest)}";
    }

    private TMP_Text FindLapsText()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PelaajanKierrosTarkastus validator = player.GetComponent<PelaajanKierrosTarkastus>();
            if (validator != null)
                return validator.lapsText;
        }

        return null;
    }

    private void ShowAiGapCamera()
    {
        Transform aiTransform = FindAiCarTransform();
        if (aiTransform == null)
        {
            Debug.LogWarning("[Tuomari] AI-autoa ei löytynyt voittokameraa varten.");
            return;
        }

        Transform cam = victoryCamera != null ? victoryCamera : FindPlayerCameraTransform();
        if (cam == null)
        {
            Debug.LogWarning("[Tuomari] Kameraa ei löytynyt voittokameraa varten.");
            return;
        }

        cam.SetParent(null, true);
        cam.position = aiTransform.position + Vector3.up * victoryCameraHeight;
        cam.rotation = Quaternion.LookRotation(Vector3.down, aiTransform.forward);
    }

    private Transform FindAiCarTransform()
    {
        CarIdentify[] cars = Object.FindObjectsByType<CarIdentify>(FindObjectsSortMode.None);
        foreach (var c in cars)
        {
            if (c.kind == CarKind.AI)
                return c.transform;
        }

        return null;
    }

    private Transform FindPlayerCameraTransform()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Camera cam = player.GetComponentInChildren<Camera>();
            if (cam != null)
                return cam.transform;
        }

        return null;
    }
}