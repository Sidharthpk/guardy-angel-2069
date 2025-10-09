using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string SCORE_KEY = "save_score";
    private const string HIGHSCORE_KEY = "save_highscore";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveScore(int score)
    {
        PlayerPrefs.SetInt(SCORE_KEY, score);
        PlayerPrefs.Save();
    }

    public int LoadScore()
    {
        return PlayerPrefs.GetInt(SCORE_KEY, 0);
    }

    public void SaveHighScore(int score)
    {
        int currentHigh = LoadHighScore();
        if (score > currentHigh)
        {
            PlayerPrefs.SetInt(HIGHSCORE_KEY, score);
            PlayerPrefs.Save();
        }
    }

    public int LoadHighScore()
    {
        return PlayerPrefs.GetInt(HIGHSCORE_KEY, 0);
    }

    public void ClearData()
    {
        PlayerPrefs.DeleteKey(SCORE_KEY);
        PlayerPrefs.DeleteKey(HIGHSCORE_KEY);
    }
}
