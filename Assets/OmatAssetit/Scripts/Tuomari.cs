using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class Tuomari : MonoBehaviour
{
    [Header("Winner Text")]
    public TMP_Text resultText;

    [Header("Lap Times")]
    public int kierrostenMaara = 3;
    public TMP_Text kierros1Text;
    public TMP_Text kierros2Text;
    public TMP_Text kierros3Text;

    [Header("Final Result")]
    [Tooltip("Current LapsText. After the race this shows the total race time.")]
    public TMP_Text lapsText;
    [Tooltip("Text that shows the best time ever achieved.")]
    public TMP_Text parasIkinAikaText;

    [Header("Victory Camera (when PLAYER wins)")]
    [Tooltip("Camera inside the Player. Leave empty to find it automatically on the Player-tagged object.")]
    public Transform victoryCamera;

    [Tooltip("How high above the AI car the camera should be during the victory view.")]
    public float victoryCameraHeight = 15f;

    [Header("Return To Main Menu")]
    [Tooltip("Seconds after the winner is declared before returning to the main menu.")]
    public float backToMenuDelay = 10f;
    [Tooltip("Scene to load after the countdown.")]
    public string mainMenuSceneName = "StartScreen";

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

        // Lap time texts stay hidden until their lap has actually been completed.
        SetLapTextVisible(kierros1Text, false);
        SetLapTextVisible(kierros2Text, false);
        SetLapTextVisible(kierros3Text, false);

        // Best time is hidden until somebody finishes the race.
        SetBestTimeVisible(false);

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
            Debug.LogWarning("[Tuomari] Car is missing LapCounter.");
            return;
        }

        if (id.kind == CarKind.Player)
        {
            var validator = car.GetComponent<PelaajanKierrosTarkastus>();
            if (validator == null)
            {
                Debug.LogError("PelaajanKierrosTarkastus component is missing.");
                return;
            }

            if (!validator.AllVisitedThisLap)
            {
                Debug.Log("Player crossed the finish line, but hasn't hit all the checkpoints!");
                validator.ShowMissedCheckpointsMessage();
                return;
            }

            int completedLapNumber = lap.lapsCompleted + 1;
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

            // Keep the total race time in LapsText.
            if (lapsText == null)
                lapsText = FindLapsText();

            if (lapsText != null)
                lapsText.text = $"Race time: {totalTimeString}";

            UpdateBestTime(totalTime);

            if (GameManager.Instance != null)
                GameManager.Instance.Phase = RacePhase.Finished;

            Debug.Log($"WINNER: {winnerName}, Race time: {totalTimeString}");

            if (id.kind == CarKind.Player)
                ShowAiGapCamera();

            // The return countdown is shown only in BoostText.
            StartCoroutine(BackToMainMenuCountdown());
        }
    }

    private void RecordPlayerLapTime(int lapNumber)
    {
        if (!raceTimerStarted)
        {
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
        {
            target.text = $"Lap {lapNumber}: {FormatTime(lapTime)}";
            SetLapTextVisible(target, true);
        }
    }

    private void SetLapTextVisible(TMP_Text target, bool visible)
    {
        if (target != null && target.gameObject != null)
            target.gameObject.SetActive(visible);
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
        {
            parasIkinAikaText.text = $"Best time: {FormatTime(currentBest)}";
            SetBestTimeVisible(true);
        }
    }

    private void SetBestTimeVisible(bool visible)
    {
        if (parasIkinAikaText != null && parasIkinAikaText.gameObject != null)
            parasIkinAikaText.gameObject.SetActive(visible);
    }

    private IEnumerator BackToMainMenuCountdown()
    {
        TMP_Text boostText = FindBoostText();
        int secondsLeft = Mathf.CeilToInt(backToMenuDelay);

        while (secondsLeft > 0)
        {
            if (boostText != null)
                boostText.text = $"Returning to main menu in {secondsLeft}s";

            yield return new WaitForSeconds(1f);
            secondsLeft--;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private TMP_Text FindBoostText()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
                return playerScript.uiText;
        }

        return null;
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
            Debug.LogWarning("[Tuomari] AI car not found for victory camera.");
            return;
        }

        Transform cam = victoryCamera != null ? victoryCamera : FindPlayerCameraTransform();
        if (cam == null)
        {
            Debug.LogWarning("[Tuomari] Camera not found for victory camera.");
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