using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Lisää tämä skripti tyhjään GameObjectiin uudessa "DifficultySelect"-scenessä.
// A/D vaihtavat valintaa kolmen vaihtoehdon välillä, W vahvistaa ja lataa Game-scenen.
public class DifficultySelector : MonoBehaviour
{
    [Header("Näytettävät tekstit (raahaa Inspectorissa)")]
    public TMP_Text easyText;
    public TMP_Text normalText;
    public TMP_Text hardText;

    [Header("Väriasetukset")]
    public Color selectedColor = Color.yellow;
    public Color unselectedColor = Color.white;

    [Header("Seuraava scene")]
    public string gameSceneName = "Game";

    private readonly Difficulty[] options = { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard };
    private int selectedIndex = 1; // Normal keskellä ja oletuksena valittuna

    void Start()
    {
        UpdateVisuals();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
            UpdateVisuals();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            selectedIndex = (selectedIndex + 1) % options.Length;
            UpdateVisuals();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            ConfirmSelection();
        }
    }

    private void UpdateVisuals()
    {
        SetTextState(easyText, options[selectedIndex] == Difficulty.Easy);
        SetTextState(normalText, options[selectedIndex] == Difficulty.Normal);
        SetTextState(hardText, options[selectedIndex] == Difficulty.Hard);
    }

    private void SetTextState(TMP_Text text, bool isSelected)
    {
        if (text == null) return;
        text.color = isSelected ? selectedColor : unselectedColor;
        text.fontStyle = isSelected ? (FontStyles.Bold | FontStyles.Underline) : FontStyles.Normal;
    }

    private void ConfirmSelection()
    {
        Difficulty chosen = options[selectedIndex];

        // Varmuuden vuoksi: luodaan DifficultyManager jos sitä ei löydy scenestä valmiiksi.
        if (DifficultyManager.Instance == null)
        {
            GameObject go = new GameObject("DifficultyManager");
            go.AddComponent<DifficultyManager>();
        }
        DifficultyManager.Instance.SelectedDifficulty = chosen;

        SceneManager.LoadScene(gameSceneName);
    }
}