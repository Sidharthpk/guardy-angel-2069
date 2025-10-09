using UnityEngine;
using TMPro; // TextMeshPro namespace

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_Text highScoreText; // assign in inspector

    void Start()
    {
        // Load high score from SaveManager and display
        int highScore = SaveManager.Instance.LoadHighScore();
        highScoreText.text = "High Score: " + highScore;
    }
}
