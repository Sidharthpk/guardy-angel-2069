using UnityEngine;

// Class responsible for saving and loading game data like score and high score
public class SaveManager : MonoBehaviour
{
    // Singleton instance for global access
    public static SaveManager Instance;

    // Keys used to store data in PlayerPrefs
    private const string SCORE_KEY = "save_score";
    private const string HIGHSCORE_KEY = "save_highscore";

    private void Awake()
    {
        // Initialize singleton instance
        if (Instance == null)
        {
            Instance = this;
            // Prevent this object from being destroyed when loading new scenes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Destroy duplicate instances
            Destroy(gameObject);
        }
    }

    // Save current score to PlayerPrefs
    public void SaveScore(int score)
    {
        PlayerPrefs.SetInt(SCORE_KEY, score);
        PlayerPrefs.Save(); // Ensure data is written to disk
    }

    // Load current score from PlayerPrefs, returns 0 if none exists
    public int LoadScore()
    {
        return PlayerPrefs.GetInt(SCORE_KEY, 0);
    }

    // Save high score only if new score is greater than current high score
    public void SaveHighScore(int score)
    {
        int currentHigh = LoadHighScore();
        if (score > currentHigh)
        {
            PlayerPrefs.SetInt(HIGHSCORE_KEY, score);
            PlayerPrefs.Save();
        }
    }

    // Load high score from PlayerPrefs, returns 0 if none exists
    public int LoadHighScore()
    {
        return PlayerPrefs.GetInt(HIGHSCORE_KEY, 0);
    }

    // Clear all saved score and high score data
    public void ClearData()
    {
        PlayerPrefs.DeleteKey(SCORE_KEY);
        PlayerPrefs.DeleteKey(HIGHSCORE_KEY);
    }
}
