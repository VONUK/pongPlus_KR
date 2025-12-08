using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAudio : MonoBehaviour
{
    public AudioSource asSounds;

    public AudioClip wallSound;
    public AudioClip paddleSound;
    public AudioClip scoreSound;

    public void PlayWallSound() {
        asSounds.PlayOneShot(wallSound);
    }
    public void PlayPaddleSound() {
        asSounds.PlayOneShot(paddleSound);
    }
    public void PlayScoreSound() {
        asSounds.PlayOneShot(scoreSound);
    }
}   
