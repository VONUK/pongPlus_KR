using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    public Rigidbody2D rb2d;
    public int PaddleId;
    public float PaddleMoveSpeed = 5f;
    private void Start() {
         
    }

    private void FixedUpdate() {
        float value = Processinput();
        Move(value);
    }

    private float Processinput() {
        float movement = 0f;
        if (PaddleId == 1) {
            movement = Input.GetAxis("MovementPlayer1");
        }
        if (PaddleId == 2) {
            movement = Input.GetAxis("MovementPlayer2");
        }
        return movement;
    }

    private void Move(float movement) {
        Vector2 velo = rb2d.velocity;
        velo.y = PaddleMoveSpeed * movement;
        rb2d.velocity = velo;
    }
}
