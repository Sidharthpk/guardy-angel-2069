using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Represents a single card in the memory game, handles flipping, fading, and interaction
public class Card : MonoBehaviour
{
    // The ID of the sprite assigned to this card
    private int spriteID;

    // Unique identifier for this card in the grid
    private int id;

    // Tracks if the card is currently face-up
    private bool flipped;

    // Tracks if the card is in the middle of a flip animation
    private bool turning;

    // Reference to the Image component that displays the card's sprite
    [SerializeField]
    private Image img;

    // Coroutine that rotates the card 90 degrees around the Y-axis for a flip animation
    // If changeSprite is true, it switches the visible sprite mid-flip
    private IEnumerator Flip90(Transform thisTransform, float time, bool changeSprite)
    {
        // Save the starting and ending rotation states
        Quaternion startRotation = thisTransform.rotation;
        Quaternion endRotation = thisTransform.rotation * Quaternion.Euler(new Vector3(0, 90, 0));

        // Determines how quickly the flip completes
        float rate = 1.0f / time;
        float t = 0.0f;

        // Smoothly rotate the card over time
        while (t < 1.0f)
        {
            t += Time.deltaTime * rate;
            thisTransform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        // Once rotated 90 degrees, switch sprite and continue flipping back to front
        if (changeSprite)
        {
            flipped = !flipped; // Toggle card state
            ChangeSprite();     // Change visible image
            StartCoroutine(Flip90(transform, time, false)); // Continue second half of flip
        }
        else
        {
            turning = false; // Flip completed
        }
    }

    // Triggers a flip animation when the player interacts with the card
    public void Flip()
    {
        turning = true; // Prevents further clicks during flip
        AudioPlayer.Instance.PlayAudio(0); // Plays flip sound
        StartCoroutine(Flip90(transform, 0.25f, true)); // Starts flip animation
    }

    // Updates the card’s visible sprite based on its flipped state
    private void ChangeSprite()
    {
        if (spriteID == -1 || img == null) return;

        // Show front sprite if flipped, otherwise show card back
        if (flipped)
            img.sprite = CardGridManager.Instance.GetSprite(spriteID);
        else
            img.sprite = CardGridManager.Instance.CardBack();
    }

    // Triggers fade animation when card is removed after a correct match
    public void Inactive()
    {
        StartCoroutine(Fade());
    }

    // Gradually fades the card’s image to transparent
    private IEnumerator Fade()
    {
        float rate = 1.0f / 2.5f;
        float t = 0.0f;

        while (t < 1.0f)
        {
            t += Time.deltaTime * rate;
            img.color = Color.Lerp(img.color, Color.clear, t); // Smooth fade-out
            yield return null;
        }
    }

    // Restores card color if reactivated (useful for restarts)
    public void Active()
    {
        if (img)
            img.color = Color.white;
    }

    // Property for setting and getting the card’s sprite ID
    public int SpriteID
    {
        set
        {
            spriteID = value;
            flipped = true;
            ChangeSprite();
        }
        get { return spriteID; }
    }

    // Property for setting and getting the card’s unique ID
    public int ID
    {
        set { id = value; }
        get { return id; }
    }

    // Resets card to face-down rotation for new games
    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        flipped = true;
    }

    // Handles card button click event from UI
    public void CardBtn()
    {
        // Ignore clicks if already flipped, turning, or game is inactive
        if (flipped || turning) return;
        if (!CardGridManager.Instance.canClick()) return;

        // Start flip and notify manager of selection
        Flip();
        StartCoroutine(SelectionEvent());
    }

    // Waits briefly after click before notifying manager (ensures smooth animation)
    private IEnumerator SelectionEvent()
    {
        yield return new WaitForSeconds(0.5f);
        CardGridManager.Instance.cardClicked(spriteID, id);
    }
}
