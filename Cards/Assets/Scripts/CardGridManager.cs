using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Manages overall gameplay, card generation, logic, scoring, and layout
public class CardGridManager : MonoBehaviour
{
    // Singleton reference for global access
    public static CardGridManager Instance;

    // Game grid size (default 2x2)
    public static int gameSize = 2;

    // Prefab reference for each card object
    [SerializeField]
    private GameObject prefab;

    // Parent container that holds all card instances
    [SerializeField]
    private GameObject cardList;

    // Sprite for the back side of each card
    [SerializeField]
    private Sprite cardBack;

    // Array of available front sprites for random card assignment
    [SerializeField]
    private Sprite[] sprites;

    // List holding all active card objects
    private Card[] cards;

    // UI references
    [SerializeField]
    private GameObject panel;   // Game panel that holds all cards
    [SerializeField]
    private GameObject info;    // Info panel (for instructions or size selector)
    [SerializeField]
    private Card spritePreload; // Used to preload sprites and prevent lag
    [SerializeField]
    private Text sizeLabel;     // Text showing current grid size
    [SerializeField]
    private Slider sizeSlider;  // Slider for selecting grid size
    [SerializeField]
    private Text timeLabel;     // Displays elapsed time

    // Timer variable
    private float time;

    // Tracks selected cards and total remaining
    private int spriteSelected;
    private int cardSelected;
    private int cardLeft;

    // Indicates if the game is currently running
    private bool gameStart;

    // Initialize singleton instance
    void Awake()
    {
        Instance = this;
    }

    // Initialize game state
    void Start()
    {
        gameStart = false;
        panel.SetActive(false); // Hide gameplay panel until start
    }

    // Preload card sprites to memory to avoid lag during first play
    private void PreloadCardImage()
    {
        for (int i = 0; i < sprites.Length; i++)
            spritePreload.SpriteID = i;
        spritePreload.gameObject.SetActive(false);
    }

    // Called when the player starts the game
    public void StartCardGame()
    {
        // Prevent multiple games running simultaneously
        if (gameStart) return;
        gameStart = true;

        // Enable gameplay panel, hide info UI
        panel.SetActive(true);
        info.SetActive(false);

        // Generate and position cards on the grid
        SetGamePanel();

        // Reset gameplay tracking variables
        cardSelected = spriteSelected = -1;
        cardLeft = cards.Length;

        // Randomly assign sprite pairs to cards
        SpriteCardAllocation();

        // Flip cards briefly before hiding to start gameplay
        StartCoroutine(HideFace());

        // Reset timer
        time = 0;
    }

    // Creates grid layout and positions all card objects dynamically
    private void SetGamePanel()
    {
        // Adjust grid for odd-numbered sizes (so there’s always an even number of cards)
        int isOdd = gameSize % 2;

        cards = new Card[gameSize * gameSize - isOdd];

        // Clear existing cards from parent container
        foreach (Transform child in cardList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // Get panel size to calculate card positioning
        RectTransform panelsize = panel.transform.GetComponent(typeof(RectTransform)) as RectTransform;
        float row_size = panelsize.sizeDelta.x;
        float col_size = panelsize.sizeDelta.y;

        // Scale and spacing calculation based on grid size
        float scale = 1.0f / gameSize;
        float xInc = row_size / gameSize;
        float yInc = col_size / gameSize;

        float curX = -xInc * (float)(gameSize / 2);
        float curY = -yInc * (float)(gameSize / 2);

        // Adjust start position for even grids
        if (isOdd == 0)
        {
            curX += xInc / 2;
            curY += yInc / 2;
        }

        float initialX = curX;

        // Loop through rows (Y-axis)
        for (int i = 0; i < gameSize; i++)
        {
            curX = initialX;

            // Loop through columns (X-axis)
            for (int j = 0; j < gameSize; j++)
            {
                GameObject c;

                // For odd grids, the last card is skipped to maintain pairing
                if (isOdd == 1 && i == (gameSize - 1) && j == (gameSize - 1))
                {
                    int index = gameSize / 2 * gameSize + gameSize / 2;
                    c = cards[index].gameObject;
                }
                else
                {
                    // Create new card object
                    c = Instantiate(prefab);
                    c.transform.parent = cardList.transform;

                    int index = i * gameSize + j;
                    cards[index] = c.GetComponent<Card>();
                    cards[index].ID = index;

                    // Scale card based on grid size
                    c.transform.localScale = new Vector3(scale, scale);
                }

                // Position card within panel
                c.transform.localPosition = new Vector3(curX, curY, 0);
                curX += xInc;
            }
            curY += yInc;
        }
    }

    // Resets all cards to face-down rotation
    void ResetFace()
    {
        for (int i = 0; i < gameSize; i++)
            cards[i].ResetRotation();
    }

    // Hides all cards after a short delay to start the round
    IEnumerator HideFace()
    {
        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < cards.Length; i++)
            cards[i].Flip();

        yield return new WaitForSeconds(0.5f);
    }

    // Randomly assigns matching pairs of sprites to cards
    private void SpriteCardAllocation()
    {
        int i, j;
        int[] selectedID = new int[cards.Length / 2];

        // Select random unique sprites for pairs
        for (i = 0; i < cards.Length / 2; i++)
        {
            int value = Random.Range(0, sprites.Length - 1);

            // Ensure no duplicate sprite selections in this batch
            for (j = i; j > 0; j--)
            {
                if (selectedID[j - 1] == value)
                    value = (value + 1) % sprites.Length;
            }
            selectedID[i] = value;
        }

        // Reset card visuals and states
        for (i = 0; i < cards.Length; i++)
        {
            cards[i].Active();
            cards[i].SpriteID = -1;
            cards[i].ResetRotation();
        }

        // Assign each sprite ID to two random cards
        for (i = 0; i < cards.Length / 2; i++)
            for (j = 0; j < 2; j++)
            {
                int value = Random.Range(0, cards.Length - 1);
                while (cards[value].SpriteID != -1)
                    value = (value + 1) % cards.Length;

                cards[value].SpriteID = selectedID[i];
            }
    }

    // Called when slider changes to update grid size label
    public void SetGameSize()
    {
        gameSize = (int)sizeSlider.value;
        sizeLabel.text = gameSize + " X " + gameSize;
    }

    // Returns the front sprite based on its assigned ID
    public Sprite GetSprite(int spriteId)
    {
        return sprites[spriteId];
    }

    // Returns the shared card back sprite
    public Sprite CardBack()
    {
        return cardBack;
    }

    // Checks whether cards can be clicked (only during gameplay)
    public bool canClick()
    {
        if (!gameStart)
            return false;
        return true;
    }

    // Called when a card is clicked, handles matching logic
    public void cardClicked(int spriteId, int cardId)
    {
        // First card selection
        if (spriteSelected == -1)
        {
            spriteSelected = spriteId;
            cardSelected = cardId;
        }
        else
        {
            // Second card selection
            if (spriteSelected == spriteId)
            {
                // Correct match, fade out both cards
                cards[cardSelected].Inactive();
                cards[cardId].Inactive();
                cardLeft -= 2;
                CheckGameWin();
            }
            else
            {
                // Incorrect match, flip both cards back
                cards[cardSelected].Flip();
                cards[cardId].Flip();
            }
            cardSelected = spriteSelected = -1;
        }
    }

    // Checks if all cards have been matched
    private void CheckGameWin()
    {
        if (cardLeft == 0)
        {
            EndGame();
            AudioPlayer.Instance.PlayAudio(1); // Play win sound
        }
    }

    // Ends the game, hides panel
    private void EndGame()
    {
        gameStart = false;
        panel.SetActive(false);
    }

    // Button event to stop the game prematurely
    public void GiveUp()
    {
        EndGame();
    }

    // Toggles visibility of info panel
    public void DisplayInfo(bool i)
    {
        info.SetActive(i);
    }

    // Updates timer display while the game is active
    private void Update()
    {
        if (gameStart)
        {
            time += Time.deltaTime;
            timeLabel.text = "Time: " + time + "s";
        }
    }
}
