using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    // Unique identifier for the card sprite
    private int spriteID;
    // Unique identifier for the card instance
    private int id;
    // Tracks if the card is currently face-up
    private bool flipped;
    // Tracks if the card is currently being turned (to prevent double flips)
    private bool turning;
    [SerializeField]
    private Image img; // Reference to the Image component to display the card sprite

    // Coroutine to animate card flipping by 90 degrees
    // If changeSprite is true, changes the card's sprite midway through the flip
    private IEnumerator Flip90(Transform thisTransform, float time, bool changeSprite)
    {
        Quaternion startRotation = thisTransform.rotation;
        Quaternion endRotation = thisTransform.rotation * Quaternion.Euler(new Vector3(0, 90, 0));
        float rate = 1.0f / time;
        float t = 0.0f;

        // Rotate smoothly from startRotation to endRotation
        while (t < 1.0f)
        {
            t += Time.deltaTime * rate;
            thisTransform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        // Change sprite if specified, then complete the remaining 90 degrees flip
        if (changeSprite)
        {
            flipped = !flipped; // toggle flipped state
            ChangeSprite();     // update the sprite to front/back
            StartCoroutine(Flip90(transform, time, false)); // finish flip
        }
        else
        {
            turning = false; // finished flipping
        }
    }

    // Public method to flip the card 180 degrees
    public void Flip()
    {
        turning = true; // mark the card as currently turning
        AudioPlayer.Instance.PlayAudio(0); // play flip sound
        StartCoroutine(Flip90(transform, 0.25f, true)); // start flip animation
    }

    // Updates the card sprite depending on flipped state
    private void ChangeSprite()
    {
        if (spriteID == -1 || img == null) return;

        if (flipped)
            img.sprite = CardGridManager.Instance.GetSprite(spriteID); // show front
        else
            img.sprite = CardGridManager.Instance.CardBack(); // show back
    }

    // Starts fade animation to make the card disappear
    public void Inactive()
    {
        StartCoroutine(Fade());
    }

    // Gradually changes the card's alpha to make it invisible
    private IEnumerator Fade()
    {
        float rate = 1.0f / 2.5f;
        float t = 0.0f;

        while (t < 1.0f)
        {
            t += Time.deltaTime * rate;
            img.color = Color.Lerp(img.color, Color.clear, t); // fade out
            yield return null;
        }
    }

    // Resets the card to fully visible
    public void Active()
    {
        if (img)
            img.color = Color.white;
    }

    // Property to get/set the spriteID
    // Setting it automatically flips the card to show the front
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

    // Property to get/set the card ID
    public int ID
    {
        set { id = value; }
        get { return id; }
    }

    // Resets card rotation to default face-up state
    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        flipped = true;
    }

    // Called when the card is clicked by the player
    public void CardBtn()
    {
        if (flipped || turning) return; // ignore if already flipped or in animation
        if (!CardGridManager.Instance.canClick()) return; // ignore if game is not ready
        Flip(); // flip the card
        StartCoroutine(SelectionEvent()); // notify manager after delay
    }

    // Informs the CardGridManager that this card was clicked, with a slight delay
    private IEnumerator SelectionEvent()
    {
        yield return new WaitForSeconds(0.5f); // wait for flip animation to complete
        CardGridManager.Instance.cardClicked(spriteID, id); // notify manager
    }
}
