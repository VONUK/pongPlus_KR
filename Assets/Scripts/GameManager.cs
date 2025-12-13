using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameUi gameUI;
    public GameAudio gameAudio;

    public int scorePlayer1, scorePlayer2;
    public System.Action onReset;
    public int maxScore = 10;
    public int currectScore = 0;

    public TMP_InputField playerOneNickname;
    public TMP_InputField playerTwoNickname;

    public PongDBService DBService;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            gameUI.onStartGame += OnStartGame;
        }
    }

    public void OnScoreZoneReached(int scoreZoneId)
    {
        if (scoreZoneId == 2)
        {
            scorePlayer1++;
            currectScore++;
        }
        if (scoreZoneId == 1)
        {
            scorePlayer2++;
            currectScore++;
        }
        gameAudio.PlayScoreSound();
        gameUI.UpdateScores(scorePlayer1, scorePlayer2);
        gameUI.HightLightScore(scoreZoneId);
        CheckWin();
    }

    public void CheckWin()
    {
        int winnerId = scorePlayer1 == maxScore ? 1 : scorePlayer2 == maxScore ? 2 : 0;
        if (winnerId != 0)
        {
            if (Mathf.Abs(scorePlayer2 - scorePlayer1) > 1)
            {
                gameUI.OnGameEnds(winnerId);
                DBService.SaveMatchResult(
                    playerOneNickname.text,
                    playerTwoNickname.text,
                    scorePlayer1,
                    scorePlayer2
                    );
            }
            else
            {
                maxScore++;
                winnerId = 0;
                onReset?.Invoke();
            }
        }
        else
        {
            onReset?.Invoke();
        }
    }

    private void OnStartGame()
    {
        scorePlayer1 = 0;
        scorePlayer2 = 0;
        currectScore = 0;
        maxScore = 2;
        gameUI.UpdateScores(scorePlayer1, scorePlayer2);
    }
}
