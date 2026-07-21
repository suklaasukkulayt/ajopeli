using UnityEngine;
using TMPro; // Tarvitaan UI-tekstin päivittämiseen

[RequireComponent(typeof(Rigidbody))]
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
    private Rigidbody rb;

    // Viittaus suoristus/teleport-skriptiin, jotta tiedetään milloin SE ohjaa autoa
    // eikä pelaajan omaa ohjausta pidä ottaa huomioon samaan aikaan.
    private PlayerUprightAndShortcuts_AutoFallReset autoCorrector;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        autoCorrector = GetComponent<PlayerUprightAndShortcuts_AutoFallReset>();
        // Asetetaan pelin alussa nopeudeksi normaali perusnopeus
        currentSpeed = baseSpeed;
    }

    // Update: näppäinpainallukset, ajastimet ja UI -- ei fysiikkaa, joten pysyy täällä.
    void Update()
    {
        // Jos peli ei ole käynnissä, ei tehdä mitään (alkuperäinen logiikka)
        if (GameManager.Instance.Phase != RacePhase.Racing)
        {
            return;
        }

        // --- BOOST-LOGIIKKA ALKAA ---
        if (Input.GetKeyDown(KeyCode.B) && !isBoosted && cooldownTimer <= 0f)
        {
            isBoosted = true;
            currentSpeed = boostSpeed;
            boostTimer = boostDuration;
        }

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
            cooldownTimer -= Time.deltaTime;
        }

        if (uiText != null)
        {
            if (isBoosted)
            {
                uiText.text = "Boosted!";
            }
            else if (cooldownTimer > 0f)
            {
                uiText.text = $"{cooldownTimer:F1}s to the next boost!";
            }
            else
            {
                uiText.text = "Boost ready!";
            }
        }
        // --- BOOST-LOGIIKKA LOPPUU ---
    }

    // FixedUpdate: itse liikkuminen Rigidbodyn kautta (fysiikkamoottori näkee liikkeen,
    // Continuous Collision Detection toimii, ei enää seinien läpimenoa boostilla).
    void FixedUpdate()
    {
        if (GameManager.Instance.Phase != RacePhase.Racing)
        {
            return;
        }

        // TÄRKEÄ KORJAUS: jos PlayerControlShortcuts_Rigidbody parhaillaan suoristaa
        // autoa (kaatunut/ilmassa pyörinyt), ei anneta pelaajan oman ohjauksen kutsua
        // rb.MoveRotation:ia SAMASSA FixedUpdatessa -- kaksi skriptiä joka yrittää
        // asettaa saman Rigidbodyn rotaation joka framessa aiheuttaa juuri sen ongelman
        // missä ohjaus "jumittuu" kunnes irrottaa napit. Korjauksen ajaksi ohjaus
        // yksinkertaisesti pausetetaan, ja jatkuu heti kun auto on suoristunut.
        if (autoCorrector != null && autoCorrector.IsAutoCorrecting)
        {
            return;
        }

        float move = Input.GetAxis("Vertical") * currentSpeed * Time.fixedDeltaTime;
        float turn = Input.GetAxis("Horizontal") * turnSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + transform.forward * move);
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
    }
}