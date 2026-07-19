using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerUprightAndShortcuts_AutoFallReset : MonoBehaviour
{
    [Header("Teleport & shortcuts")]
    public Vector3 teleportPosition = new Vector3(13f, 0f, 47f);

    [Header("Auto-fall reset")]
    [Tooltip("Jos pelaajan Y (korkeus) laskee alle tämän arvon, suoritetaan teleport samaan tapaan kuin T-näppäin.")]
    public float fallYThreshold = -5f;

    [Header("Tilt detection")]
    [Tooltip("Kulma (asteina) jonka ylityttyä automaattinen korjaus käynnistyy.")]
    public float angleThreshold = 45f;
    [Tooltip("Estää turhan herkästi laukeamisen; jos >0, vaaditaan myös tämä minimipyörimisnopeus NOPEALLE kaatumiselle.")]
    public float angularVelocityMin = 0.5f;
    [Tooltip("Jos auto on kallellaan yli angleThresholdin tämän monta sekuntia yhtäjaksoisesti (esim. levossa katolla/kyljellä, pyörimisnopeus jo nollassa), korjaus käynnistyy silti tämän ajan jälkeen.")]
    public float stuckTimeThreshold = 1.5f;

    [Header("Smooth auto-correction")]
    [Tooltip("Kuinka monta astetta per sekunti voidaan kääntyä automaattisessa korjauksessa. Suurempi = nopeampi korjaus.")]
    public float autoCorrectionDegreesPerSecond = 360f;
    [Tooltip("Kun lopetetaan korjaus, jos etäisyys tavoitteeseen alle tämän verran asteina, korjaus päättyy.")]
    public float finishAngleEpsilon = 0.5f;
    [Tooltip("Jos true, korjauksessa korjataan vain Y-aksele (yaw) — pelaajan pitch/roll nollataan.")]
    public bool correctOnlyYaw = true;

    [Header("Camera settings")]
    [Tooltip("Käytetäänkö pääkameran vaakasuuntaa (yaw) pelaajan orientaation määrittelyyn R-komennossa ja automaatiossa.")]
    public bool useCameraYaw = true;

    Rigidbody rb;
    Camera mainCamera;

    // Sisäiset tilat
    bool isAutoCorrecting = false;
    Quaternion autoTargetRotation = Quaternion.identity;
    float tiltTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Haetaan kamera ensisijaisesti omista lapsista (Playerin sisällä oleva "Camera"),
        // koska se ei riipu MainCamera-tagista. Camera.main toimii vain, jos jokin AKTIIVINEN
        // kamera on tagattu "MainCamera" -- tällä hetkellä se ei ole voimassa tässä scenessä.
        mainCamera = GetComponentInChildren<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            Debug.LogWarning("[PlayerUprightAndShortcuts_AutoFallReset] Kameraa ei löytynyt Playerin lapsista eikä Camera.main:sta. useCameraYaw ei toimi.");
        }
    }

    void FixedUpdate()
    {
        // Aloita automaattinen korjaus, jos kallistus on liian suuri
        if (!isAutoCorrecting && IsTiltedBeyondThreshold())
        {
            StartAutoCorrection();
        }

        // Jos käynnissä, suorita pehmeä kääntyminen kohti targettia
        if (isAutoCorrecting)
        {
            PerformAutoCorrectionStep();
        }
    }

    void Update()
    {
        // R painallus -> käynnistä korjaus kameran suuntaan (tai fallback)
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartAutoCorrection(GetLookDirectionRotation());
        }

        // T painallus -> teleporttaa JA asettaa katseen täsmälleen Y=90°
        if (Input.GetKeyDown(KeyCode.T))
        {
            DoTeleportAsT();
        }

        // Auto-fall reset: jos pelaajan y < threshold -> teleporttaa kuten T
        if (transform.position.y < fallYThreshold)
        {
            DoTeleportAsT();
        }
    }

    void DoTeleportAsT()
    {
        // Teleport position fysiikan kautta
        rb.position = teleportPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        tiltTimer = 0f;

        // Target rotation: täsmälleen yaw = 90°
        Quaternion target90 = Quaternion.Euler(0f, 90f, 0f);

        // Päivitä kameran yaw suoraan VAIN jos kamera EI ole Playerin lapsi.
        // Jos se ON lapsi (kuten nyt), kamera seuraa Playerin rotaatiota automaattisesti
        // alla olevan autokorjauksen kautta -- kameran world-rotaation pakottaminen tässä
        // rikkoisi kameran paikallisen rotaation ja sotkisi esim. LookBackCameran tallentaman
        // "eteenpäin"-asennon.
        if (mainCamera != null && !mainCamera.transform.IsChildOf(transform))
        {
            Vector3 camEuler = mainCamera.transform.eulerAngles;
            mainCamera.transform.rotation = Quaternion.Euler(camEuler.x, 90f, camEuler.z);
        }

        // Käynnistä sulava korjaus kohti 90° yaw:ta
        StartAutoCorrection(target90);
    }

    bool IsTiltedBeyondThreshold()
    {
        float angleFromUp = Vector3.Angle(transform.up, Vector3.up);
        if (angleFromUp <= angleThreshold)
        {
            tiltTimer = 0f;
            return false;
        }

        // Nopea kaatuminen/lennossa pyöriminen: reagoi heti kun pyörimisnopeus riittää
        if (angularVelocityMin <= 0f || rb.angularVelocity.magnitude >= angularVelocityMin)
        {
            tiltTimer = 0f;
            return true;
        }

        // Hidas/pysähtynyt tapaus: auto makaa kyljellään tai katolla paikallaan, eikä
        // pyörimisnopeus enää ylitä angularVelocityMin-rajaa. Lasketaan aikaa ja
        // käynnistetään korjaus joka tapauksessa stuckTimeThresholdin jälkeen.
        tiltTimer += Time.fixedDeltaTime;
        return tiltTimer >= stuckTimeThreshold;
    }

    // Aloittaa automaattisen korjauksen; jos target ei annettu, käytetään kameran yaw:ia / forwardia
    void StartAutoCorrection(Quaternion? optionalTarget = null)
    {
        Quaternion target = optionalTarget ?? GetLookDirectionRotation();

        // Jos correctOnlyYaw on true, tiivistetään target vain yaw:ksi
        if (correctOnlyYaw)
        {
            float yaw = target.eulerAngles.y;
            target = Quaternion.Euler(0f, yaw, 0f);
        }

        autoTargetRotation = target;
        isAutoCorrecting = true;
        tiltTimer = 0f;

        // Nollaa pyörimisnopeus niin ei rikkoudu slerp:illä
        rb.angularVelocity = Vector3.zero;
    }

    void PerformAutoCorrectionStep()
    {
        // Laske kulmaero
        float angleToTarget = Quaternion.Angle(rb.rotation, autoTargetRotation);

        // Jos jo lähellä, tee lopetus
        if (angleToTarget <= finishAngleEpsilon)
        {
            rb.MoveRotation(autoTargetRotation);
            rb.angularVelocity = Vector3.zero;
            isAutoCorrecting = false;
            return;
        }

        // Laske kuinka monta astetta voimme kääntyä tässä FixedUpdateissä
        float maxDelta = autoCorrectionDegreesPerSecond * Time.fixedDeltaTime;

        // Seuraava rotatiovaihe
        Quaternion next = Quaternion.RotateTowards(rb.rotation, autoTargetRotation, maxDelta);

        // Siirrä rigidbodyn rotatioon fysiikan kautta
        rb.MoveRotation(next);

        // Pidä angularVelocity nollassa korjauksen ajan
        rb.angularVelocity = Vector3.zero;
    }

    // Palauttaa target rotationin: joko kameran yaw tai pelaajan forward suunta vaakatasossa
    Quaternion GetLookDirectionRotation()
    {
        if (useCameraYaw && mainCamera != null)
        {
            float cameraYaw = mainCamera.transform.eulerAngles.y;
            return Quaternion.Euler(0f, cameraYaw, 0f);
        }
        else if (transform.forward != Vector3.zero)
        {
            Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z);
            if (flatForward.sqrMagnitude < 0.0001f)
                return rb.rotation;
            return Quaternion.LookRotation(flatForward.normalized, Vector3.up);
        }
        else
        {
            return rb.rotation;
        }
    }

    // Debug-visualisointi editorissa
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
        if (isAutoCorrecting)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + autoTargetRotation * Vector3.forward * 2f);
        }

        // Piirrä myös alaraja-linja (visuaalinen apu tasolle)
        Gizmos.color = Color.yellow;
        Vector3 from = new Vector3(transform.position.x - 2f, fallYThreshold, transform.position.z - 2f);
        Vector3 to   = new Vector3(transform.position.x + 2f, fallYThreshold, transform.position.z + 2f);
        Gizmos.DrawLine(from, to);
    }
}