using UnityEngine;
using TMPro; // Required for TextMeshPro UI components

// Handles the main menu UI, specifically displaying the high score
public class MainMenuUI : MonoBehaviour
{
    // Reference to the TextMeshPro UI element to show the high score
    [SerializeField] private TMP_Text highScoreText; // Assign in the Inspector

    void Start()
    {
        // Get the high score from SaveManager (persistent storage)
        int highScore = SaveManager.Instance.LoadHighScore();

        // Update the TMP_Text to display the high score
        highScoreText.text = "High Score: " + highScore;
    }
}
