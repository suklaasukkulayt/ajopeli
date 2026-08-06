using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class Tuomari : MonoBehaviour
{
    public TMP_Text resultText;

    public int kierrostenMaara = 3;
    private bool winnerDeclared = false;

    [Header("Voittokamera (kun PELAAJA voittaa)")]
    [Tooltip("Playerin sisällä oleva kamera. Jätä tyhjäksi niin skripti löytää sen automaattisesti 'Player'-tagatusta objektista.")]
    public Transform victoryCamera;

    [Tooltip("Kuinka korkealla AI-auton yläpuolella kamera leijuu voittokuvassa.")]
    public float victoryCameraHeight = 15f;

    [Header("Paluu päävalikkoon voiton jälkeen")]
    [Tooltip("Kuinka monta sekuntia voiton jälkeen ennen kuin palataan päävalikkoon.")]
    public float backToMenuDelay = 10f;
    [Tooltip("Scene johon palataan voiton jälkeen.")]
    public string mainMenuSceneName = "StartScreen";

    private void Start()
    {
        resultText.text = "";
    }

    private void OnTriggerEnter(Collider car)
    {
        CarIdentify id = car.GetComponent<CarIdentify>();

        if (id == null)
        {
            return;
        }

        LapCounter lap = car.GetComponent<LapCounter>();

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

            // Vain kun PELAAJA voittaa: näytetään kamera AI-auton yläpuolelta,
            // jotta näkyy kuinka kauas jälkeen AI jäi.
            if (id.kind == CarKind.Player)
            {
                ShowAiGapCamera();
            }

            // Molemmissa tapauksissa (pelaaja TAI AI voittaa): näytetään BoostTextissä
            // ja LapsTextissä lasku takaisin päävalikkoon.
            StartCoroutine(BackToMainMenuCountdown());
        }
    }

    private IEnumerator BackToMainMenuCountdown()
    {
        TMP_Text boostText = FindBoostText();
        TMP_Text lapsText = FindLapsText();

        int secondsLeft = Mathf.CeilToInt(backToMenuDelay);
        while (secondsLeft > 0)
        {
            string message = $"Returning to main menu in {secondsLeft}s";
            if (boostText != null) boostText.text = message;
            if (lapsText != null) lapsText.text = message;

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
            Player p = player.GetComponent<Player>();
            if (p != null)
            {
                return p.uiText;
            }
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
            {
                return validator.lapsText;
            }
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

        // Irrotetaan kamera Playerista (jos se on sen lapsi) jotta se voidaan vapaasti
        // sijoittaa maailmakoordinaateissa AI-auton yläpuolelle sen sijaan että se
        // seuraisi edelleen Playerin liikettä/rotaatiota.
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
            {
                return c.transform;
            }
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
            {
                return cam.transform;
            }
        }
        return null;
    }
}