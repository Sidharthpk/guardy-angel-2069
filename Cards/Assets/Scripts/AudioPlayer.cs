using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class responsible for playing audio clips in the game
public class AudioPlayer : MonoBehaviour
{
    // Singleton instance for global access
    public static AudioPlayer Instance;

    // AudioSource component that will play the sounds
    [SerializeField]
    private AudioSource audioSource;

    // Array of audio clips for different game events
    // Example mapping: 0 = card flip, 1 = match, 2 = mismatch, 3 = game over
    [SerializeField]
    private AudioClip[] audio;

    // Default volume (not currently used)
    private static float vol = 1;

    void Awake()
    {
        // Initialize singleton instance
        Instance = this;
    }

    // Play audio by index with default volume
    public void PlayAudio(int id)
    {
        if (audioSource && id >= 0 && id < audio.Length)
        {
            audioSource.PlayOneShot(audio[id]);
        }
    }

    // Play audio by index with specified volume
    public void PlayAudio(int id, float vol)
    {
        if (audioSource && id >= 0 && id < audio.Length)
        {
            audioSource.PlayOneShot(audio[id], vol);
        }
    }
}
