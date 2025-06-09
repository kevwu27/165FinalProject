using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    [Tooltip("Audio clip to play on interaction.")]
    public AudioClip clickSound;

    [Tooltip("Optional AudioSource to play the sound from. If left empty, it will play at object's position.")]
    public AudioSource audioSource;

    public void playAudio()
    {
        if (clickSound == null) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clickSound, transform.position);
        }
    }
}