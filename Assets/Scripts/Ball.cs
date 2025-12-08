using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Rigidbody2D rd2d;
    public float maxInitialAngle = 0.67f;
    public float startMaxInitialAngle = 0.37f;
    public float moveSpeed = 4f;
    private float startX = 0f;
    public float speedMultiplier = 0.5f;
    private bool firstBounce = true;


    private void Start() {
        GameManager.instance.onReset += ResetBall;
        GameManager.instance.gameUI.onStartGame += ResetBall;
    }

    private void ResetBall() {
        ResetBallPosition();
        InitialPush();
    }
    private void ResetBallPosition() {
        Vector2 position = new Vector2(startX, 0f); // Старт из середины строки Y
        transform.position = position;
    }

    private void InitialPush() {
        Vector2 dir = Random.value < 0.5f ? Vector2.left : Vector2.right;
        dir.y = Random.Range(-startMaxInitialAngle, startMaxInitialAngle);
        rd2d.velocity = dir * moveSpeed/2;
        firstBounce = true;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        ScoreZone scoreZone = collision.GetComponent<ScoreZone>();
        if (scoreZone) {
            GameManager.instance.OnScoreZoneReached(scoreZone.scoreZoneId);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        Paddle paddle = collision.collider.GetComponent<Paddle>();
        if (paddle) {
            if (firstBounce) {
                rd2d.velocity = rd2d.velocity.normalized * moveSpeed;
                firstBounce = false;
            }
            GameManager.instance.gameAudio.PlayPaddleSound();
            rd2d.velocity *= speedMultiplier;
        }
        Wall wall = collision.collider.GetComponent<Wall>();
        if (wall) {
                GameManager.instance.gameAudio.PlayWallSound();
        }
    }
}
 