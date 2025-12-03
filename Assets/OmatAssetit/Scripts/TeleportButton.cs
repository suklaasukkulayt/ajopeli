using UnityEngine;

public class TeleportButton : MonoBehaviour
{
    [Tooltip("Voit liittää Playerin Inspectorissa. Jos jätät tyhjäksi, skripti yrittää etsiä Playeria automaattisesti.")]
    public GameObject player;

    public Vector3 targetPosition = new Vector3(-224f, 0f, 93f);

    void Awake()
    {
        // Jos inspectorissa ei ole asetettu, yritetään etsiä tagin tai nimen perusteella
        if (player == null)
        {
            // Ensiksi etsi tagilla "Player" (suositeltu)
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null)
            {
                player = found;
                Debug.Log("[TeleportButton] Player löydetty tagilla 'Player'.");
            }
            else
            {
                // Fallback: etsi nimen perusteella
                found = GameObject.Find("Player");
                if (found != null)
                {
                    player = found;
                    Debug.Log("[TeleportButton] Player löydetty nimellä 'Player'.");
                }
            }
        }

        // Lopuksi varoitus, jos ei löydetty
        if (player == null)
        {
            Debug.LogWarning("[TeleportButton] Player reference is null. Aseta Player Inspectorissa tai varmista että objektissa on tag 'Player' tai nimi 'Player'.");
        }
    }

    // Tämä metodi liitetään Buttonin OnClickiin
    public void TeleportPlayer()
    {
        if (player == null)
        {
            Debug.LogError("[TeleportButton] Teleport failed: player reference is null.");
            return;
        }

        // Jos pelaajalla on Rigidbody, käytä sitä telepor- tauttiin (nollaa nopeus)
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = targetPosition;
        }
        else
        {
            player.transform.position = targetPosition;
        }

        Debug.Log("[TeleportButton] Player teleported to " + targetPosition);
    }
}
