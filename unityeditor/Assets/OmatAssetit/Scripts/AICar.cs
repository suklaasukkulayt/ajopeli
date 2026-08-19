using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AICar : MonoBehaviour
{
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;

    public float speed = 10f;
    public float rotationSpeed = 5f;

    [Header("Vaikeustaso")]
    [Tooltip("Kuinka paljon AI:n nopeutta kerrotaan Easy-tasolla (alle 1 = hitaampi).")]
    public float easyModeSpeedMultiplier = 0.85f;
    [Tooltip("Kuinka paljon AI:n nopeutta kerrotaan Hard-tasolla (yli 1 = nopeampi).")]
    public float hardModeSpeedMultiplier = 1.15f;
    [Tooltip("Kuinka monta yksikköä taaksepäin AI aloittaa Easy-tasolla (omaa aloitussuuntaansa pitkin).")]
    public float easyModeStartOffsetBack = 6f;

    [Header("Törmäysreaktio (kun pelaaja osuu koviin)")]
    [Tooltip("Kuinka suuri törmäysnopeus (suhteellinen, m/s) vaaditaan jotta AI 'säikähtää' ja päästää fysiikan hetkeksi vapaaksi (spinnaa/lentää). Pieni tönäisy takaa ei riitä.")]
    public float stunImpactThreshold = 6f;

    [Tooltip("Kuinka pitkään (s) AI on säikähtäneenä eikä ohjaa itseään -- fysiikka saa vaikuttaa vapaasti tänä aikana.")]
    public float stunDuration = 1.5f;

    [Tooltip("Ylimääräinen satunnainen vääntövoima törmäyksen yhteydessä, jotta reaktio näyttää aina hauskalta/dramaattiselta riippumatta tarkasta osumakulmasta.")]
    public float extraSpinTorque = 8f;

    private Rigidbody rb;
    private bool isStunned = false;
    private float stunTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Vaikeustason vaikutus AI-autoon. Jos DifficultyManageria ei löydy
        // (esim. Game-sceneä testataan suoraan), käytetään Normalia eli ei muutosta.
        if (DifficultyManager.Instance != null)
        {
            switch (DifficultyManager.Instance.SelectedDifficulty)
            {
                case Difficulty.Easy:
                    speed *= easyModeSpeedMultiplier;
                    rb.position -= transform.forward * easyModeStartOffsetBack;
                    break;
                case Difficulty.Hard:
                    speed *= hardModeSpeedMultiplier;
                    break;
                case Difficulty.Normal:
                default:
                    break;
            }
        }
    }

    // Reagoi vain toiseen autoon (esim. Playeriin), ei seiniin/terrainiin.
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<CarIdentify>() == null)
        {
            return;
        }

        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed >= stunImpactThreshold)
        {
            isStunned = true;
            stunTimer = stunDuration;

            Vector3 randomAxis = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ).normalized;

            rb.AddTorque(randomAxis * extraSpinTorque * impactSpeed, ForceMode.Impulse);
        }
    }

    // Siirretty Updatesta FixedUpdateen ja transform.Translate/rotation -> rb.MovePosition/MoveRotation.
    // Sama syy kuin Player.cs:ssä aiemmin: suora Transform-asetus ohittaa fysiikkamoottorin,
    // jolloin se riitelee samalla GameObjectilla olevan Rigidbodyn (gravitaatio, törmäykset)
    // kanssa joka framessa -- tästä nykiminen ja osin myös seinien läpimeno tulivat.
    void FixedUpdate()
    {
        if (GameManager.Instance.Phase != RacePhase.Racing)
        {
            return;
        }

        if (isStunned)
        {
            stunTimer -= Time.fixedDeltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
            }
            return; // Fysiikka saa hetkeksi täyden vallan -- ei ohjata itse tänä aikana
        }

        Transform target = waypoints[currentWaypointIndex];
        Vector3 targetXZ = new Vector3(target.position.x, rb.position.y, target.position.z);
        Vector3 direction = (targetXZ - rb.position).normalized;

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.Slerp(rb.rotation, lookRotation, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRotation);
        }

        rb.MovePosition(rb.position + transform.forward * speed * Time.fixedDeltaTime);

        if (Vector3.Distance(rb.position, targetXZ) < 2f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }
}