using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class SpeedBooster : MonoBehaviour
{
    [Header("Target")]
    public GameObject playerObject;
    public MonoBehaviour movementScript;

    [Header("Boost settings")]
    public float boostAmount = 5f;
    public float boostDuration = 2f;
    public bool stackable = true;

    [Header("Cooldown & input")]
    public float cooldown = 15f;
    public KeyCode boostKey = KeyCode.B;

    [Header("UI (optional)")]
    public Button boostButton;
    public Color availableColor = Color.white;
    public Color unavailableColor = Color.gray;

    [Tooltip("Image that displays cooldown fill (must be Image.Type = Filled). If left empty, script tries to use the Button's Image if it's Filled.")]
    public Image cooldownFillImage;

    // --- internal state ---
    private bool isOnCooldown = false;
    private Image buttonImage;

    // Non-stackable handling
    private Coroutine currentBoostCoroutine = null;
    private float currentAppliedBoost = 0f; // amount currently applied for non-stackable mode

    // Stacking handling
    private int activeBoostCount = 0;

    // Cooldown coroutine ref
    private Coroutine cooldownCoroutine = null;

    void Start()
    {
        if (boostButton != null)
        {
            buttonImage = boostButton.GetComponent<Image>();
            SetButtonAvailable(true);

            // If cooldownFillImage not assigned, but buttonImage exists and is Filled, use it as fill image.
            if (cooldownFillImage == null && buttonImage != null && buttonImage.type == Image.Type.Filled)
            {
                cooldownFillImage = buttonImage;
            }
        }
        else
        {
            // If no button provided but cooldownFillImage exists, ensure fill is zero initially
            if (cooldownFillImage != null)
                cooldownFillImage.fillAmount = 0f;
        }

        if (cooldownFillImage != null)
            cooldownFillImage.fillAmount = 0f;
    }

    void Update()
    {
        if (Input.GetKeyDown(boostKey))
        {
            OnBoostButtonPressed();
        }
    }

    public void OnBoostButtonPressed()
    {
        if (movementScript == null && playerObject == null)
        {
            Debug.LogWarning("SpeedBooster: playerObject tai movementScript pitää asettaa inspectorissa.");
            return;
        }

        if (isOnCooldown)
        {
            // ei tehdä mitään jos cooldown päällä
            return;
        }

        bool applied = false;

        if (!stackable)
        {
            // Non-stackable: poista mahdollinen aiempi boost hallitusti ennen uuden asettamista
            if (currentAppliedBoost != 0f)
            {
                // Poistetaan aiempi boost ja lopetetaan sen coroutine
                TryModifySpeed(-currentAppliedBoost);
                currentAppliedBoost = 0f;
                if (currentBoostCoroutine != null)
                {
                    StopCoroutine(currentBoostCoroutine);
                    currentBoostCoroutine = null;
                }
            }

            // Yritetään asettaa uusi boost
            applied = TryModifySpeed(boostAmount);
            if (applied)
            {
                currentAppliedBoost = boostAmount;
                currentBoostCoroutine = StartCoroutine(OneBoostRoutineNonStackable(boostDuration));
            }
            else
            {
                // jos ei onnistunut, ei tehdä mitään
                Debug.LogWarning("SpeedBooster: boostin asettaminen epäonnistui (non-stackable).");
            }
        }
        else
        {
            // Stackable: yritä lisätä välittömästi; jos onnistuu, aloita poistorutiini
            applied = TryModifySpeed(boostAmount);
            if (applied)
            {
                activeBoostCount++;
                StartCoroutine(StackingRemoveRoutine(boostAmount, boostDuration));
            }
            else
            {
                Debug.LogWarning("SpeedBooster: boostin asettaminen epäonnistui (stackable).");
            }
        }

        // Aloitetaan cooldown vain jos boost todella asetettiin
        if (applied && !isOnCooldown)
        {
            cooldownCoroutine = StartCoroutine(CooldownRoutine());
        }
    }

    private IEnumerator OneBoostRoutineNonStackable(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (currentAppliedBoost != 0f)
        {
            TryModifySpeed(-currentAppliedBoost);
            currentAppliedBoost = 0f;
        }

        currentBoostCoroutine = null;
    }

    private IEnumerator StackingRemoveRoutine(float amount, float duration)
    {
        yield return new WaitForSeconds(duration);
        TryModifySpeed(-amount);
        activeBoostCount = Mathf.Max(0, activeBoostCount - 1);
    }

    private IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        SetButtonAvailable(false);

        // Jos fill-image asetettu, animoidaan fillAmount ajoissa: 1 -> 0
        if (cooldownFillImage != null)
        {
            float elapsed = 0f;
            cooldownFillImage.fillAmount = 1f;
            while (elapsed < cooldown)
            {
                elapsed += Time.deltaTime;
                float remaining = Mathf.Max(0f, cooldown - elapsed);
                cooldownFillImage.fillAmount = remaining / cooldown;
                yield return null;
            }
            cooldownFillImage.fillAmount = 0f;
        }
        else
        {
            // Ei fill-imagea: odotetaan normaali cooldown
            yield return new WaitForSeconds(cooldown);
        }

        isOnCooldown = false;
        SetButtonAvailable(true);
        cooldownCoroutine = null;
    }

    private void SetButtonAvailable(bool available)
    {
        if (boostButton != null)
        {
            boostButton.interactable = available;
        }
        if (buttonImage != null)
        {
            buttonImage.color = available ? availableColor : unavailableColor;
        }

        // jos fill-image on olemassa ja cooldown ei päällä, varmista se on tyhjä
        if (cooldownFillImage != null && !isOnCooldown)
        {
            cooldownFillImage.fillAmount = 0f;
        }
    }

    // --- Reflection-based speed modifier (sama kuin ennen) ---
    private bool TryModifySpeed(float delta)
    {
        if (movementScript != null)
        {
            if (ModifyFieldOrProperty(movementScript, "speed", delta)) return true;
            if (ModifyFieldOrProperty(movementScript, "Speed", delta)) return true;
        }

        if (playerObject != null)
        {
            var components = playerObject.GetComponents<MonoBehaviour>();
            foreach (var comp in components)
            {
                if (comp == null) continue;
                if (ModifyFieldOrPropertyCaseInsensitive(comp, "speed", delta)) return true;
            }
        }

        Debug.LogWarning("SpeedBooster: ei löytänyt 'speed' kenttää tai propertya annetusta komponentista/pelaajasta.");
        return false;
    }

    private bool ModifyFieldOrProperty(object target, string name, float delta)
    {
        var t = target.GetType();
        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && (f.FieldType == typeof(float) || f.FieldType == typeof(double) || f.FieldType == typeof(int)))
        {
            if (f.FieldType == typeof(float))
            {
                float old = (float)f.GetValue(target);
                f.SetValue(target, old + delta);
                return true;
            }
            if (f.FieldType == typeof(double))
            {
                double old = (double)f.GetValue(target);
                f.SetValue(target, old + (double)delta);
                return true;
            }
            if (f.FieldType == typeof(int))
            {
                int old = (int)f.GetValue(target);
                f.SetValue(target, old + Mathf.RoundToInt(delta));
                return true;
            }
        }

        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.CanRead && p.CanWrite)
        {
            if (p.PropertyType == typeof(float))
            {
                float old = (float)p.GetValue(target, null);
                p.SetValue(target, old + delta, null);
                return true;
            }
            if (p.PropertyType == typeof(double))
            {
                double old = (double)p.GetValue(target, null);
                p.SetValue(target, old + (double)delta, null);
                return true;
            }
            if (p.PropertyType == typeof(int))
            {
                int old = (int)p.GetValue(target, null);
                p.SetValue(target, old + Mathf.RoundToInt(delta), null);
                return true;
            }
        }

        return false;
    }

    private bool ModifyFieldOrPropertyCaseInsensitive(object target, string name, float delta)
    {
        var t = target.GetType();
        foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (string.Equals(f.Name, name, System.StringComparison.OrdinalIgnoreCase))
            {
                if (f.FieldType == typeof(float))
                {
                    float old = (float)f.GetValue(target);
                    f.SetValue(target, old + delta);
                    return true;
                }
                if (f.FieldType == typeof(double))
                {
                    double old = (double)f.GetValue(target);
                    f.SetValue(target, old + (double)delta);
                    return true;
                }
                if (f.FieldType == typeof(int))
                {
                    int old = (int)f.GetValue(target);
                    f.SetValue(target, old + Mathf.RoundToInt(delta));
                    return true;
                }
            }
        }

        foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (!p.CanRead || !p.CanWrite) continue;
            if (string.Equals(p.Name, name, System.StringComparison.OrdinalIgnoreCase))
            {
                if (p.PropertyType == typeof(float))
                {
                    float old = (float)p.GetValue(target, null);
                    p.SetValue(target, old + delta, null);
                    return true;
                }
                if (p.PropertyType == typeof(double))
                {
                    double old = (double)p.GetValue(target, null);
                    p.SetValue(target, old + (double)delta, null);
                    return true;
                }
                if (p.PropertyType == typeof(int))
                {
                    int old = (int)p.GetValue(target, null);
                    p.SetValue(target, old + Mathf.RoundToInt(delta), null);
                    return true;
                }
            }
        }

        return false;
    }
}
