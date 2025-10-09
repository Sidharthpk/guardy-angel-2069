using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGridManager : MonoBehaviour
{
    // Singleton instance for global access
    public static CardGridManager Instance;

    // Game grid size (NxN)
    public static int gameSize = 2;

    // Prefab for individual cards
    [SerializeField] private GameObject prefab;
    // Parent object to hold all card instances
    [SerializeField] private GameObject cardList;
    // Sprite for the back of a card
    [SerializeField] private Sprite cardBack;
    // Array of front-facing sprites for cards
    [SerializeField] private Sprite[] sprites;
    // Array to store all instantiated card objects
    private Card[] cards;

    // Game UI panels
    [SerializeField] private GameObject panel; // main game panel
    [SerializeField] private GameObject info;  // info panel
    [SerializeField] private Card spritePreload; // for preloading sprites to avoid lag

    // UI elements for game size and timer
    [SerializeField] private Text sizeLabel;
    [SerializeField] private Slider sizeSlider;
    [SerializeField] private Text timeLabel;

    // UI element for displaying score, combo, and high score
    [SerializeField] private Text scoreLabel;

    // Game tracking variables
    private float time; // elapsed time
    private int spriteSelected; // sprite ID of selected card
    private int cardSelected;   // index of selected card
    private int cardLeft;       // number of cards remaining
    private bool gameStart;     // is game currently active?

    // Score and combo variables
    private int score = 0;
    private int combo = 0;
    private int maxCombo = 0;
    private float comboTimer = 0f;
    private float comboResetTime = 3f; // combo expires after 3 seconds without match
    private int highScore = 0;

    void Awake()
    {
        Instance = this; // set singleton instance

        // Load saved high score from SaveManager
        highScore = SaveManager.Instance.LoadHighScore();
    }

    void Start()
    {
        gameStart = false;
        panel.SetActive(false); // hide game panel initially
    }

    // Preload all sprites to prevent first-time lag
    private void PreloadCardImage()
    {
        for (int i = 0; i < sprites.Length; i++)
            spritePreload.SpriteID = i;

        spritePreload.gameObject.SetActive(false);
    }

    // Start the card game
    public void StartCardGame()
    {
        if (gameStart) return; // prevent starting if game is active
        gameStart = true;

        // Show game panel and hide info panel
        panel.SetActive(true);
        info.SetActive(false);

        // Setup grid and instantiate cards
        SetGamePanel();

        // Reset selection and remaining cards
        cardSelected = spriteSelected = -1;
        cardLeft = cards.Length;

        // Allocate sprites to cards
        SpriteCardAllocation();

        // Show cards briefly so player can memorize them
        StartCoroutine(HideFace());
        time = 0;

        // Reset score and combo counters
        score = 0;
        combo = 0;
        maxCombo = 0;
        UpdateScoreUI();
    }

    // Setup card grid dynamically
    private void SetGamePanel()
    {
        int isOdd = gameSize % 2; // special handling if grid size is odd
        cards = new Card[gameSize * gameSize - isOdd]; // total card slots

        // Remove existing cards
        foreach (Transform child in cardList.transform)
            Destroy(child.gameObject);

        // Calculate card size and positions
        RectTransform panelsize = panel.GetComponent<RectTransform>();
        float row_size = panelsize.sizeDelta.x;
        float col_size = panelsize.sizeDelta.y;
        float scale = 1.0f / gameSize;
        float xInc = row_size / gameSize;
        float yInc = col_size / gameSize;
        float curX = -xInc * (float)(gameSize / 2);
        float curY = -yInc * (float)(gameSize / 2);

        if (isOdd == 0)
        {
            curX += xInc / 2;
            curY += yInc / 2;
        }

        float initialX = curX;

        // Loop through grid and instantiate cards
        for (int i = 0; i < gameSize; i++)
        {
            curX = initialX;
            for (int j = 0; j < gameSize; j++)
            {
                GameObject c;
                // Handle last empty slot if odd grid
                if (isOdd == 1 && i == (gameSize - 1) && j == (gameSize - 1))
                {
                    int index = gameSize / 2 * gameSize + gameSize / 2;
                    c = cards[index].gameObject;
                }
                else
                {
                    c = Instantiate(prefab, cardList.transform);
                    int index = i * gameSize + j;
                    cards[index] = c.GetComponent<Card>();
                    cards[index].ID = index;
                    c.transform.localScale = new Vector3(scale, scale);
                }
                // Position the card
                c.transform.localPosition = new Vector3(curX, curY, 0);
                curX += xInc;
            }
            curY += yInc;
        }
    }

    // Briefly show all cards at start
    IEnumerator HideFace()
    {
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < cards.Length; i++)
            cards[i].Flip();
        yield return new WaitForSeconds(0.5f);
    }

    // Allocate pairs of sprites to random cards
    private void SpriteCardAllocation()
    {
        int[] selectedID = new int[cards.Length / 2];

        // Randomly select sprites for pairs
        for (int i = 0; i < cards.Length / 2; i++)
        {
            int value = Random.Range(0, sprites.Length - 1);
            for (int j = i; j > 0; j--)
            {
                if (selectedID[j - 1] == value)
                    value = (value + 1) % sprites.Length;
            }
            selectedID[i] = value;
        }

        // Reset all cards to default state
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].Active();
            cards[i].SpriteID = -1;
            cards[i].ResetRotation();
        }

        // Assign each pair to two random cards
        for (int i = 0; i < cards.Length / 2; i++)
            for (int j = 0; j < 2; j++)
            {
                int value = Random.Range(0, cards.Length - 1);
                while (cards[value].SpriteID != -1)
                    value = (value + 1) % cards.Length;

                cards[value].SpriteID = selectedID[i];
            }
    }

    // Change game grid size from UI slider
    public void SetGameSize()
    {
        gameSize = (int)sizeSlider.value;
        sizeLabel.text = gameSize + " X " + gameSize;
    }

    // Get card front sprite
    public Sprite GetSprite(int spriteId) => sprites[spriteId];
    // Get card back sprite
    public Sprite CardBack() => cardBack;
    // Check if game allows clicking cards
    public bool canClick() => gameStart;

    // Handles card selection and matching logic
    public void cardClicked(int spriteId, int cardId)
    {
        if (spriteSelected == -1)
        {
            // First card selected
            spriteSelected = spriteId;
            cardSelected = cardId;
        }
        else
        {
            // Second card selected
            if (spriteSelected == spriteId)
            {
                // Correct match
                cards[cardSelected].Inactive();
                cards[cardId].Inactive();
                cardLeft -= 2;

                // Increment combo
                combo++;
                if (combo > maxCombo) maxCombo = combo;

                int comboBonus = 50 * combo;
                score += 100 + comboBonus;

                AudioPlayer.Instance.PlayAudio(1); // play match sound
                UpdateScoreUI();
                comboTimer = 0f;

                CheckGameWin();
            }
            else
            {
                // Wrong match
                cards[cardSelected].Flip();
                cards[cardId].Flip();
                combo = 0;
                UpdateScoreUI();

                AudioPlayer.Instance.PlayAudio(2); // play mismatch sound
            }
            spriteSelected = cardSelected = -1; // reset selection
        }
    }

    // Check if all cards have been matched
    private void CheckGameWin()
    {
        if (cardLeft == 0)
        {
            EndGame();
            AudioPlayer.Instance.PlayAudio(3); // game over sound

            // Save high score
            SaveManager.Instance.SaveHighScore(score);
        }
    }

    // End the game and hide panel
    private void EndGame()
    {
        gameStart = false;
        panel.SetActive(false);
    }

    // Give up button handler
    public void GiveUp() => EndGame();

    // Show/hide info panel
    public void DisplayInfo(bool i) => info.SetActive(i);

    // Update loop for time and combo timeout
    private void Update()
    {
        if (!gameStart) return;

        // Track elapsed time
        time += Time.deltaTime;
        timeLabel.text = "Time: " + time.ToString("F1") + "s";

        // Combo expires if timer exceeds reset time
        comboTimer += Time.deltaTime;
        if (combo > 0 && comboTimer > comboResetTime)
        {
            combo = 0;
            UpdateScoreUI();
        }
    }

    // Update score, combo, and high score UI
    private void UpdateScoreUI()
    {
        if (!scoreLabel) return;

        scoreLabel.text = $"Score: {score}\n\nCombo: x{combo}\n\nBest: {highScore}";
    }
}
