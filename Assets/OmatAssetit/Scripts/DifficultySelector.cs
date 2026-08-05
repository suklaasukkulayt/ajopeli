using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Lisää tämä skripti tyhjään GameObjectiin "DifficultySelect"-scenessä.
// KAKSI VAIHETTA:
//  1) Vaikeustaso: A/D vaihtavat valintaa, W vahvistaa.
//  2) FOV: A/D muuttavat FOV-arvoa +-5 per painallus, W vahvistaa ja lataa Game-scenen.
// FOV-teksti on näkyvissä koko ajan, mutta A/D eivät vaikuta siihen ennen kuin
// vaikeustaso on vahvistettu (vaihe 2 alkaa).
public class DifficultySelector : MonoBehaviour
{
    private enum Stage { Difficulty, Fov }

    [Header("Vaikeustaso-tekstit (raahaa Inspectorissa)")]
    public TMP_Text easyText;
    public TMP_Text normalText;
    public TMP_Text hardText;

    [Header("FOV-teksti (näkyvissä koko ajan, aktivoituu vaikeustason vahvistuksen jälkeen)")]
    public TMP_Text fovText;
    public float fovStep = 5f;
    public float minFov = 40f;
    public float maxFov = 110f;
    public float startingFov = 60f;

    [Header("Väriasetukset")]
    public Color selectedColor = Color.yellow;
    public Color unselectedColor = Color.white;
    public Color lockedColor = Color.gray;

    [Header("Seuraava scene")]
    public string gameSceneName = "Game";

    private readonly Difficulty[] options = { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard };
    private int selectedIndex = 1; // Normal keskellä ja oletuksena valittuna
    private Stage stage = Stage.Difficulty;
    private float currentFov;

    void Start()
    {
        currentFov = startingFov;
        UpdateDifficultyVisuals();
        UpdateFovVisuals();
    }

    void Update()
    {
        if (stage == Stage.Difficulty)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
                UpdateDifficultyVisuals();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                selectedIndex = (selectedIndex + 1) % options.Length;
                UpdateDifficultyVisuals();
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                ConfirmDifficulty();
            }
        }
        else // Stage.Fov
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                ChangeFov(-fovStep);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                ChangeFov(fovStep);
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                ConfirmFovAndStart();
            }
        }
    }

    private void UpdateDifficultyVisuals()
    {
        bool stageActive = stage == Stage.Difficulty;
        SetDifficultyTextState(easyText, Difficulty.Easy, stageActive);
        SetDifficultyTextState(normalText, Difficulty.Normal, stageActive);
        SetDifficultyTextState(hardText, Difficulty.Hard, stageActive);
    }

    private void SetDifficultyTextState(TMP_Text text, Difficulty forValue, bool stageActive)
    {
        if (text == null) return;

        bool isCurrentSelection = options[selectedIndex] == forValue;

        if (!stageActive)
        {
            // Vaihe on jo vahvistettu: näytetään yhä mikä valittiin, mutta himmeämpänä,
            // koska A/D eivät enää vaikuta tähän.
            text.color = isCurrentSelection ? selectedColor : lockedColor;
            text.fontStyle = isCurrentSelection ? FontStyles.Bold : FontStyles.Normal;
            return;
        }

        text.color = isCurrentSelection ? selectedColor : unselectedColor;
        text.fontStyle = isCurrentSelection ? (FontStyles.Bold | FontStyles.Underline) : FontStyles.Normal;
    }

    private void ChangeFov(float delta)
    {
        currentFov = Mathf.Clamp(currentFov + delta, minFov, maxFov);
        UpdateFovVisuals();
    }

    private void UpdateFovVisuals()
    {
        if (fovText == null) return;

        fovText.text = $"FOV: {currentFov:F0}";

        bool active = stage == Stage.Fov;
        fovText.color = active ? selectedColor : unselectedColor;
        fovText.fontStyle = active ? (FontStyles.Bold | FontStyles.Underline) : FontStyles.Normal;
    }

    private void ConfirmDifficulty()
    {
        Difficulty chosen = options[selectedIndex];
        EnsureDifficultyManager();
        DifficultyManager.Instance.SelectedDifficulty = chosen;

        stage = Stage.Fov;
        UpdateDifficultyVisuals(); // himmennetään vaikeustaso-tekstit "lukituiksi"
        UpdateFovVisuals();        // korostetaan FOV-teksti aktiiviseksi
    }

    private void ConfirmFovAndStart()
    {
        EnsureDifficultyManager();
        DifficultyManager.Instance.SelectedFov = currentFov;
        SceneManager.LoadScene(gameSceneName);
    }

    private void EnsureDifficultyManager()
    {
        if (DifficultyManager.Instance == null)
        {
            GameObject go = new GameObject("DifficultyManager");
            go.AddComponent<DifficultyManager>();
        }
    }
}