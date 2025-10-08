using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles all audio playback in the game using a Singleton pattern
public class AudioPlayer : MonoBehaviour
{
    // Static reference for global access (ensures only one instance exists)
    public static AudioPlayer Instance;

    // Reference to the AudioSource component that will play the sounds
    [SerializeField]
    private AudioSource audioSource;

    // Array of audio clips to be played (assigned in the Inspector)
    [SerializeField]
    private AudioClip[] audio;

    // Default volume level for playback
    private static float vol = 1;

    // Called before Start(), ensures this instance is globally accessible
    void Awake()
    {
        // Set this object as the Singleton instance
        Instance = this;
    }

    // Plays an audio clip from the array using its index with default volume
    public void PlayAudio(int id)
    {
        // Uses PlayOneShot so multiple sounds can overlap without interruption
        audioSource.PlayOneShot(audio[id]);
    }

    // Overloaded method that allows custom volume for a specific sound
    public void PlayAudio(int id, float vol)
    {
        // Plays the selected audio clip at the specified volume
        audioSource.PlayOneShot(audio[id], vol);
    }
}
