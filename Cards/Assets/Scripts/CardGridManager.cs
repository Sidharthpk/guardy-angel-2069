using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGridManager : MonoBehaviour
{
    public static CardGridManager Instance;
    public static int gameSize = 2;

    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject cardList;
    [SerializeField] private Sprite cardBack;
    [SerializeField] private Sprite[] sprites;
    private Card[] cards;

    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject info;
    [SerializeField] private Card spritePreload;

    [SerializeField] private Text sizeLabel;
    [SerializeField] private Slider sizeSlider;
    [SerializeField] private Text timeLabel;

    // Score + Combo UI
    [SerializeField] private Text scoreLabel;

    private float time;
    private int spriteSelected;
    private int cardSelected;
    private int cardLeft;
    private bool gameStart;

    // Score & Combo variables
    private int score = 0;
    private int combo = 0;
    private int maxCombo = 0;
    private float comboTimer = 0f;
    private float comboResetTime = 3f; // Combo expires after 3 seconds without a match
    private int highScore = 0;

    void Awake()
    {
        Instance = this;

        // Load high score from SaveManager
        highScore = SaveManager.Instance.LoadHighScore();
    }

    void Start()
    {
        gameStart = false;
        panel.SetActive(false);
    }

    private void PreloadCardImage()
    {
        for (int i = 0; i < sprites.Length; i++)
            spritePreload.SpriteID = i;

        spritePreload.gameObject.SetActive(false);
    }

    public void StartCardGame()
    {
        if (gameStart) return;
        gameStart = true;

        panel.SetActive(true);
        info.SetActive(false);

        SetGamePanel();

        cardSelected = spriteSelected = -1;
        cardLeft = cards.Length;

        SpriteCardAllocation();

        StartCoroutine(HideFace());
        time = 0;

        score = 0;
        combo = 0;
        maxCombo = 0;
        UpdateScoreUI();
    }

    private void SetGamePanel()
    {
        int isOdd = gameSize % 2;
        cards = new Card[gameSize * gameSize - isOdd];

        foreach (Transform child in cardList.transform)
            Destroy(child.gameObject);

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
        for (int i = 0; i < gameSize; i++)
        {
            curX = initialX;
            for (int j = 0; j < gameSize; j++)
            {
                GameObject c;
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
                c.transform.localPosition = new Vector3(curX, curY, 0);
                curX += xInc;
            }
            curY += yInc;
        }
    }

    IEnumerator HideFace()
    {
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < cards.Length; i++)
            cards[i].Flip();
        yield return new WaitForSeconds(0.5f);
    }

    private void SpriteCardAllocation()
    {
        int[] selectedID = new int[cards.Length / 2];

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

        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].Active();
            cards[i].SpriteID = -1;
            cards[i].ResetRotation();
        }

        for (int i = 0; i < cards.Length / 2; i++)
            for (int j = 0; j < 2; j++)
            {
                int value = Random.Range(0, cards.Length - 1);
                while (cards[value].SpriteID != -1)
                    value = (value + 1) % cards.Length;

                cards[value].SpriteID = selectedID[i];
            }
    }

    public void SetGameSize()
    {
        gameSize = (int)sizeSlider.value;
        sizeLabel.text = gameSize + " X " + gameSize;
    }

    public Sprite GetSprite(int spriteId) => sprites[spriteId];
    public Sprite CardBack() => cardBack;
    public bool canClick() => gameStart;

    public void cardClicked(int spriteId, int cardId)
    {
        if (spriteSelected == -1)
        {
            spriteSelected = spriteId;
            cardSelected = cardId;
        }
        else
        {
            if (spriteSelected == spriteId)
            {
                // Correct match
                cards[cardSelected].Inactive();
                cards[cardId].Inactive();
                cardLeft -= 2;

                combo++;
                if (combo > maxCombo) maxCombo = combo;

                int comboBonus = 50 * combo;
                score += 100 + comboBonus;

                AudioPlayer.Instance.PlayAudio(1); // match sound
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

                AudioPlayer.Instance.PlayAudio(2); // mismatch sound
            }
            cardSelected = spriteSelected = -1;
        }
    }

    private void CheckGameWin()
    {
        if (cardLeft == 0)
        {
            EndGame();
            AudioPlayer.Instance.PlayAudio(3); // game over sound

            SaveManager.Instance.SaveHighScore(score);
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
        if (!gameStart) return;

        time += Time.deltaTime;
        timeLabel.text = "Time: " + time.ToString("F1") + "s";

        comboTimer += Time.deltaTime;
        if (combo > 0 && comboTimer > comboResetTime)
        {
            combo = 0;
            UpdateScoreUI();
        }
    }

    private void UpdateScoreUI()
    {
        if (!scoreLabel) return;

        scoreLabel.text = $"Score: {score}\n\nCombo: x{combo}\n\nBest: {highScore}";
    }
}
