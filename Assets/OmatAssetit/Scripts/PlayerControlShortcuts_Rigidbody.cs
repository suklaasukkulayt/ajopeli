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
    [Tooltip("Estää turhan herkästi laukeamisen; jos >0, vaaditaan myös tämä minimipyörimisnopeus.")]
    public float angularVelocityMin = 0.5f;

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

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
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

        // Target rotation: täsmälleen yaw = 90°
        Quaternion target90 = Quaternion.Euler(0f, 90f, 0f);

        // Päivitä kameraan yaw heti, jos se ei ole playerin child tai haluat sen päivittää
        if (mainCamera != null)
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
        if (angleFromUp <= angleThreshold) return false;

        if (angularVelocityMin > 0f)
        {
            if (rb.angularVelocity.magnitude < angularVelocityMin) return false;
        }

        return true;
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
