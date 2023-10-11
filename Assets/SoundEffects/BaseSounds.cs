using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BaseSounds : MonoBehaviour
{
    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public AudioSource audio_play_piece;
    public AudioSource audio_option_select;
    public AudioSource audio_error;

    public void PlayMovePieceSound()
    {
        audio_play_piece.Play();
    }

    public void PlaySelectOptionMenu()
    {
        audio_option_select.Play();
    }

    public void PlayErrorSound()
    {
        audio_error.Play();
    }
   
}
