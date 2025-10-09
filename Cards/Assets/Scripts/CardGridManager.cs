using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGridManager : MonoBehaviour
{
    public static CardGridManager Instance; // Singleton for easy access from Card.cs
    public static int gameSize = 2;         // Size of the grid (NxN)

    [SerializeField] private GameObject prefab;      // Card prefab
    [SerializeField] private GameObject cardList;    // Parent for all cards
    [SerializeField] private Sprite cardBack;        // Sprite for card back
    [SerializeField] private Sprite[] sprites;       // Array of possible front sprites
    private Card[] cards;                            // Array holding all Card instances

    [SerializeField] private GameObject panel;       // Game panel
    [SerializeField] private GameObject info;        // Info panel
    [SerializeField] private Card spritePreload;     // Preload card for sprite caching

    [SerializeField] private Text sizeLabel;         // UI label showing grid size
    [SerializeField] private Slider sizeSlider;      // Slider to adjust grid size
    [SerializeField] private Text timeLabel;         // UI label for elapsed time

    [SerializeField] private Text scoreLabel;        // UI label showing score and combo

    private float time;           // Track elapsed game time
    private int spriteSelected;   // Currently selected sprite ID
    private int cardSelected;     // Currently selected card ID
    private int cardLeft;         // Cards left to match
    private bool gameStart;       // Is game running?

    // Score and combo system
    private int score = 0;        // Current score
    private int combo = 0;        // Current combo streak
    private int maxCombo = 0;     // Max combo achieved
    private float comboTimer = 0f;  // Timer to reset combo if too much time passes
    private float comboResetTime = 3f; // Combo expires after 3s of inactivity
    private int highScore = 0;     // High score saved to PlayerPrefs

    void Awake()
    {
        Instance = this;                        // Set singleton instance
        highScore = PlayerPrefs.GetInt("HighScore", 0); // Load saved high score
    }

    void Start()
    {
        gameStart = false;
        panel.SetActive(false); // Hide game panel initially
    }

    // Preload card sprites to avoid lag when flipping first time
    private void PreloadCardImage()
    {
        for (int i = 0; i < sprites.Length; i++)
            spritePreload.SpriteID = i;

        spritePreload.gameObject.SetActive(false);
    }

    // Starts a new card game
    public void StartCardGame()
    {
        if (gameStart) return;
        gameStart = true;

        panel.SetActive(true);
        info.SetActive(false);

        SetGamePanel();       // Setup card positions
        cardSelected = spriteSelected = -1;
        cardLeft = cards.Length;
        SpriteCardAllocation(); // Assign sprites to cards
        StartCoroutine(HideFace()); // Flip all cards face down after a brief delay
        time = 0;

        // Reset score and combo
        score = 0;
        combo = 0;
        maxCombo = 0;
        UpdateScoreUI();
    }

    // Setup grid panel: instantiate cards and position them
    private void SetGamePanel()
    {
        int isOdd = gameSize % 2;
        cards = new Card[gameSize * gameSize - isOdd]; // Reduce one card if odd number

        foreach (Transform child in cardList.transform)
            Destroy(child.gameObject); // Clear old cards

        RectTransform panelsize = panel.transform.GetComponent<RectTransform>();
        float row_size = panelsize.sizeDelta.x;
        float col_size = panelsize.sizeDelta.y;
        float scale = 1.0f / gameSize;  // Scale cards to fit panel
        float xInc = row_size / gameSize;
        float yInc = col_size / gameSize;
        float curX = -xInc * (float)(gameSize / 2);
        float curY = -yInc * (float)(gameSize / 2);

        if (isOdd == 0) { curX += xInc / 2; curY += yInc / 2; }

        float initialX = curX;

        for (int i = 0; i < gameSize; i++)
        {
            curX = initialX;
            for (int j = 0; j < gameSize; j++)
            {
                GameObject c;
                if (isOdd == 1 && i == (gameSize - 1) && j == (gameSize - 1))
                {
                    int index = gameSize / 2 * gameSize + gameSize / 2;
                    c = cards[index].gameObject; // Reuse middle card for odd grid
                }
                else
                {
                    c = Instantiate(prefab, cardList.transform); // Create new card
                    int index = i * gameSize + j;
                    cards[index] = c.GetComponent<Card>();
                    cards[index].ID = index;
                    c.transform.localScale = new Vector3(scale, scale); // Scale card
                }
                c.transform.localPosition = new Vector3(curX, curY, 0);
                curX += xInc;
            }
            curY += yInc;
        }
    }

    // Flip all cards face down after showing them briefly
    IEnumerator HideFace()
    {
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < cards.Length; i++)
            cards[i].Flip();
        yield return new WaitForSeconds(0.5f);
    }

    // Allocate sprites randomly to cards and create pairs
    private void SpriteCardAllocation()
    {
        int i, j;
        int[] selectedID = new int[cards.Length / 2];

        for (i = 0; i < cards.Length / 2; i++)
        {
            int value = Random.Range(0, sprites.Length - 1);
            for (j = i; j > 0; j--)
                if (selectedID[j - 1] == value)
                    value = (value + 1) % sprites.Length;

            selectedID[i] = value; // Save unique sprite IDs
        }

        // Reset all cards
        for (i = 0; i < cards.Length; i++)
        {
            cards[i].Active();
            cards[i].SpriteID = -1;
            cards[i].ResetRotation();
        }

        // Assign sprites to cards (pairing)
        for (i = 0; i < cards.Length / 2; i++)
            for (j = 0; j < 2; j++)
            {
                int value = Random.Range(0, cards.Length - 1);
                while (cards[value].SpriteID != -1)
                    value = (value + 1) % cards.Length;

                cards[value].SpriteID = selectedID[i];
            }
    }

    // Update grid size from slider
    public void SetGameSize()
    {
        gameSize = (int)sizeSlider.value;
        sizeLabel.text = gameSize + " X " + gameSize;
    }

    public Sprite GetSprite(int spriteId) => sprites[spriteId];
    public Sprite CardBack() => cardBack;
    public bool canClick() => gameStart;

    // Handle card selection, matching, scoring, and combo
    public void cardClicked(int spriteId, int cardId)
    {
        if (spriteSelected == -1)
        {
            spriteSelected = spriteId;
            cardSelected = cardId; // First card selected
        }
        else
        {
            if (spriteSelected == spriteId)
            {
                // ✅ Match found
                cards[cardSelected].Inactive();
                cards[cardId].Inactive();
                cardLeft -= 2;

                // Update combo & score
                combo++;
                if (combo > maxCombo) maxCombo = combo;
                int comboBonus = 50 * combo;
                score += 100 + comboBonus;

                AudioPlayer.Instance.PlayAudio(1);
                UpdateScoreUI();
                comboTimer = 0f; // reset combo timer

                CheckGameWin();
            }
            else
            {
                // ❌ Wrong match: reset combo
                cards[cardSelected].Flip();
                cards[cardId].Flip();
                combo = 0;
                UpdateScoreUI();
            }
            cardSelected = spriteSelected = -1;
        }
    }

    private void CheckGameWin()
    {
        if (cardLeft == 0)
        {
            EndGame();
            AudioPlayer.Instance.PlayAudio(1);

            // Save high score
            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt("HighScore", highScore);
                PlayerPrefs.Save();
            }
        }
    }

    private void EndGame()
    {
        gameStart = false;
        panel.SetActive(false);
    }

    public void GiveUp() => EndGame();
    public void DisplayInfo(bool i) => info.SetActive(i);

    private void Update()
    {
        if (gameStart)
        {
            time += Time.deltaTime;
            timeLabel.text = "Time: " + time.ToString("F1") + "s";

            // Reset combo if timer expires
            comboTimer += Time.deltaTime;
            if (combo > 0 && comboTimer > comboResetTime)
            {
                combo = 0;
                UpdateScoreUI();
            }
        }
    }

    // Update score & combo UI
    private void UpdateScoreUI()
    {
        if (scoreLabel)
            scoreLabel.text = $"Score: {score} | Combo: x{combo} | Best: {highScore}";
    }
}
