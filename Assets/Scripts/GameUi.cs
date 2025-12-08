using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUi : MonoBehaviour
{
    public ScoreText scoreTextPlayer1, scoreTextplayer2;
    public GameObject menuObject;
    public GameObject LeadeBoardObject;
    public TextMeshProUGUI winText;

    public TMP_InputField playerOneInput;
    public TMP_InputField playerTwoInput;

    public System.Action onStartGame;

    public void UpdateScores(int scorePlayer1, int scorePlayer2)
    {
        scoreTextPlayer1.SetScore(scorePlayer1);
        scoreTextplayer2.SetScore(scorePlayer2);
    }

    public void HightLightScore(int scoreZoneId)
    {
        if (scoreZoneId == 2)
        {
            scoreTextPlayer1.HightLight();
        }
        if (scoreZoneId == 1)
        {
            scoreTextplayer2.HightLight();
        }
    }

    public void OnMenuGameButtonClicked()
    {
        menuObject.SetActive(true);
        LeadeBoardObject.SetActive(true);
        LeadeBoardObject.SetActive(false);
    }

    public void OnStartGameButtonClicked()
    {
        menuObject.SetActive(false);
        LeadeBoardObject.SetActive(false);
        onStartGame?.Invoke();
    }

    public void OnGameEnds(int winnerId)
    {
        LeadeBoardObject.SetActive(true);
        string winnerPlayer;
        if (winnerId == 1)
            winnerPlayer = string.IsNullOrWhiteSpace(playerOneInput.text) ? "PlayerOne" : playerOneInput.text;
        else
            winnerPlayer = string.IsNullOrWhiteSpace(playerTwoInput.text) ? "PlayerTwo" : playerTwoInput.text;
        winText.text = $"{winnerPlayer} wins!";
    }
}
