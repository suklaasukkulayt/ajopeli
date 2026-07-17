using UnityEngine;
using TMPro; // Tarvitaan UI-tekstin päivittämiseen

public class Player : MonoBehaviour
{
    [Header("Liikkuminen")]
    public float baseSpeed = 10f;
    public float turnSpeed = 130f;

    [Header("Boost-asetukset")]
    public float boostSpeed = 15f;
    public float boostDuration = 1.5f;
    public float boostCooldown = 4.0f;
    public TextMeshProUGUI uiText; // Raahaa BoostText tähän Inspectorissa

    private float currentSpeed;
    private float boostTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isBoosted = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Asetetaan pelin alussa nopeudeksi normaali perusnopeus
        currentSpeed = baseSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        // Jos peli ei ole käynnissä, ei tehdä mitään (alkuperäinen logiikka)
        if(GameManager.Instance.Phase != RacePhase.Racing)
        {
            return;
        }

        // --- BOOST-LOGIIKKA ALKAA ---
        // Boostin aktivointi (B-näppäin)
        if (Input.GetKeyDown(KeyCode.B) && !isBoosted && cooldownTimer <= 0f)
        {
            isBoosted = true;
            currentSpeed = boostSpeed;
            boostTimer = boostDuration;
        }

        // Ajastimien päivitys
        if (isBoosted)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0f)
            {
                // Boost loppuu
                isBoosted = false;
                currentSpeed = baseSpeed;
                cooldownTimer = boostCooldown;
            }
        }
        else if (cooldownTimer > 0f)
        {
            // Jäähtymisaika kuluu
            cooldownTimer -= Time.deltaTime;
        }

        // UI-tekstin päivitys
        if (uiText != null)
        {
            if (isBoosted)
            {
                uiText.text = "Boosted!";
            }
            else if (cooldownTimer > 0f)
            {
                uiText.text = $"Seuraavaan boostiin: {cooldownTimer:F1}s";
            }
            else
            {
                uiText.text = "Boost valmis! (Paina B)";
            }
        }
        // --- BOOST-LOGIIKKA LOPPUU ---


        // --- ALKUPERÄINEN LIIKKUMISLOGIIKKA ---
        // Käytetään nyt currentSpeed-muuttujaa, joka muuttuu boostin mukaan
        float move = Input.GetAxis("Vertical") * currentSpeed * Time.deltaTime;
        float turn = Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime;
        //Debug.Log(move);

        transform.Translate(Vector3.forward * move);
        transform.Rotate(Vector3.up * turn);
    }
}