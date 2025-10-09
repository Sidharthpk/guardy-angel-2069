using UnityEngine;

// Singleton GameManager to handle score, combo, and save/load
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;   // Singleton instance

    public int playerScore = 0;           // Total score of the player
    public int comboCount = 0;            // Number of consecutive correct matches
    public int comboMultiplier = 1;       // Score multiplier based on combo

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Load progress when game starts
        LoadProgress();
    }

    // Adds score with combo multiplier
    public void AddScore(int basePoints)
    {
        int pointsEarned = basePoints * comboMultiplier;  // Apply multiplier
        playerScore += pointsEarned;
        SaveProgress();                                   // Save progress
    }

    // Call when player matches a pair correctly
    public void ComboSuccess()
    {
        comboCount++;                                    // Increment consecutive matches
        comboMultiplier = 1 + comboCount / 2;           // Increase multiplier every 2 matches
    }

    // Call when player mismatches a pair
    public void ComboFail()
    {
        comboCount = 0;                                  // Reset combo count
        comboMultiplier = 1;                             // Reset multiplier
    }

    // Save player progress using PlayerPrefs
    public void SaveProgress()
    {
        PlayerPrefs.SetInt("PlayerScore", playerScore);
        PlayerPrefs.SetInt("ComboCount", comboCount);
        PlayerPrefs.SetInt("ComboMultiplier", comboMultiplier);
        PlayerPrefs.Save();
    }

    // Load player progress from PlayerPrefs
    public void LoadProgress()
    {
        playerScore = PlayerPrefs.GetInt("PlayerScore", 0);
        comboCount = PlayerPrefs.GetInt("ComboCount", 0);
        comboMultiplier = PlayerPrefs.GetInt("ComboMultiplier", 1);
    }

    // Reset all progress
    public void ResetProgress()
    {
        playerScore = 0;
        comboCount = 0;
        comboMultiplier = 1;
        PlayerPrefs.DeleteAll();
    }
}
