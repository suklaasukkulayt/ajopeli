using UnityEngine;

public class TriggerBlueCarRotation : MonoBehaviour
{
    [Header("Raahaa tähän Blue Carin skripti Inspectorista:")]
    // HUOM: Vaihda sana 'AutonSkriptinNimi' siihen nimeen, mikä skripti autossasi oikeasti on!
    public AICar blueCarScript;

    [Header("Törmäysasetukset:")]
    [Tooltip("Mikä tagi objektilla pitää olla, jotta skripti aktivoituu?")]
    public string requiredTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        // Tarkistetaan, osuiko oikean tagin omaava objekti (esim. pelaaja) triggeriin
        if (other.CompareTag(requiredTag))
        {
            if (blueCarScript != null)
            {
                // 1. Aktivoidaan skripti (jos se oli pois päältä)
                blueCarScript.enabled = true;
                
                // 2. Asetetaan rotationSpeed haluttuun float-arvoon
                blueCarScript.rotationSpeed = 0.1f;
            }
            else
            {
                Debug.LogWarning("Muista raahata Blue Carin skripti tähän komponenttiin Inspectorissa!");
            }
        }
    }
}