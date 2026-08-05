using UnityEngine;

// Lisää tämä skripti mihin tahansa scenen objektiin (esim. suoraan Playeriin).
// Kun V-näppäintä pidetään pohjassa, Playerin sisällä oleva kamera siirtyy
// auton keulan puolelle ja kääntyy katsomaan taaksepäin (auton yli), jolloin
// auton keula näkyy yhä kuvassa. Kun V vapautetaan, kamera palaa takaisin
// alkuperäiseen paikkaan ja suuntaan.
public class LookBackCamera : MonoBehaviour
{
    [Tooltip("Playerin sisällä oleva 'Camera'-objekti. Jos jätät tyhjäksi, skripti yrittää löytää sen automaattisesti 'Player'-tagatusta objektista.")]
    public Transform carCamera;

    [Tooltip("Näppäin, jota pitämällä pohjassa katsotaan taakse.")]
    public KeyCode lookBackKey = KeyCode.V;

    [Header("Taaksepäin-kameran asetukset")]
    [Tooltip("Peilaa kameran nykyinen paikka Z-akselilla (auton takaa auton eteen), jotta keula pysyy kuvassa. Suositellaan päällä.")]
    public bool autoMirrorPosition = true;

    [Tooltip("Käytetään vain jos Auto Mirror Position on pois päältä: kameran oma paikallinen sijainti taaksepäin katsottaessa.")]
    public Vector3 rearViewLocalPosition;

    [Tooltip("Lisäkallistus alaspäin (asteina) taaksepäin katsottaessa, jotta auton keula näkyy paremmin kuvan alareunassa. Kokeile esim. 5-20.")]
    public float rearViewPitchOffset = 10f;

    private Vector3 forwardPosition;
    private Quaternion forwardRotation;
    private Vector3 computedRearPosition;
    private Quaternion computedRearRotation;
    private bool lookingBack = false;

    void Start()
    {
        if (carCamera == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Camera cam = player.GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    carCamera = cam.transform;
                }
            }
        }

        if (carCamera == null)
        {
            Debug.LogWarning("[LookBackCamera] Car Camera -viittaus puuttuu. Aseta se Inspectorissa.");
            return;
        }

        // Sovelletaan vaikeustasovalitsimessa asetettu FOV, jos DifficultyManager on olemassa
        // (esim. jos Game-sceneä testataan suoraan ilman valitsinta, tätä ei tehdä).
        if (DifficultyManager.Instance != null)
        {
            Camera cam = carCamera.GetComponent<Camera>();
            if (cam != null)
            {
                cam.fieldOfView = DifficultyManager.Instance.SelectedFov;
            }
        }

        forwardPosition = carCamera.localPosition;
        forwardRotation = carCamera.localRotation;

        // Peilataan kameran Z-sijainti: jos kamera on normaalisti esim. 2.5 yksikköä
        // auton TAKANA, taaksepäin-kamera siirtyy 2.5 yksikköä auton ETEEN, jolloin
        // se katsoo auton yli taaksepäin ja keula jää näkyviin kuvan alareunaan.
        Vector3 basePos = autoMirrorPosition
            ? new Vector3(forwardPosition.x, forwardPosition.y, -forwardPosition.z)
            : rearViewLocalPosition;

        computedRearPosition = basePos;
        computedRearRotation = forwardRotation * Quaternion.Euler(rearViewPitchOffset, 180f, 0f);
    }

    void Update()
    {
        if (carCamera == null) return;

        if (Input.GetKeyDown(lookBackKey)) LookBack();
        if (Input.GetKeyUp(lookBackKey)) LookForward();
    }

    private void LookBack()
    {
        if (lookingBack) return;
        lookingBack = true;
        carCamera.localPosition = computedRearPosition;
        carCamera.localRotation = computedRearRotation;
    }

    private void LookForward()
    {
        if (!lookingBack) return;
        lookingBack = false;
        carCamera.localPosition = forwardPosition;
        carCamera.localRotation = forwardRotation;
    }
}