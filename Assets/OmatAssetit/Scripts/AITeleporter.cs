using UnityEngine;

public class CarTeleporter : MonoBehaviour
{
    [Header("Teleportin asetukset:")]
    [Tooltip("Minkä Y-arvon alapuolelle auton pitää mennä, että se teleportataan?")]
    public float fallThreshold = -10f;
    
    [Tooltip("Mihin koordinaatteihin auto palautetaan?")]
    public Vector3 respawnPosition = new Vector3(18.8f, 0.101f, 43.359f);

    private Rigidbody rb;

    void Start()
    {
        // Yritetään hakea auton Rigidbody (fysiikat) muistiin
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Tarkistetaan jatkuvasti, onko auton Y-koordinaatti alle rajan
        if (transform.position.y < fallThreshold)
        {
            TeleportCar();
        }
    }

    private void TeleportCar()
    {
        // 1. Siirretään auto uuteen sijaintiin
        transform.position = respawnPosition;
        
        // 2. Nollataan vauhti ja pyörimisliike, jos autossa on Rigidbody
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}