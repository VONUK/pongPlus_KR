using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreText : MonoBehaviour
{
    private string HIGHTLIGHT_NAME = "HightLight";
    public TextMeshProUGUI DisplayText; 
    public Animator Animator;

    public void HightLight() {
        Animator.SetTrigger(HIGHTLIGHT_NAME);
    }
    public void SetScore(int Value) {
        DisplayText.text = Value.ToString();
    }
}
