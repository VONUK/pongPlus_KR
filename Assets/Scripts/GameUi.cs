using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUi : MonoBehaviour
{
    public ScoreText scoreTextPlayer1, scoreTextplayer2;
    public GameObject menuObject;
    public GameObject winnerBoardObject;
    public GameObject leadeBoardObject;
    public GameObject scoreTextObject;
    public GameObject loginObject;
    public GameObject errorLogPass;
    public GameObject successDelete;
    public TextMeshProUGUI winText;

    public TMP_InputField playerOneInput;
    public TMP_InputField playerTwoInput;

    public TMP_InputField adminlogin;
    public TMP_InputField adminPassword;

    public System.Action onStartGame;

    public PongDBService dBService;
    public Transform leaderboardContent;
    public GameObject leaderboardRowPrefab;
    public int leaderboardLimit = 10;

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

    private void HideAllObject()
    {
        menuObject.SetActive(false);
        winnerBoardObject.SetActive(false);
        leadeBoardObject.SetActive(false);
        scoreTextObject.SetActive(false);
        loginObject.SetActive(false);
        errorLogPass.SetActive(false);
        successDelete.SetActive(false);
    }
    public void StartGame()
    {
        HideAllObject();
        scoreTextObject.SetActive(true);
        menuObject.SetActive(true);
    }

    public void OnMenuGameButtonClicked()
    {
        HideAllObject();
        menuObject.SetActive(true);
        scoreTextObject.SetActive(true);
        scoreTextPlayer1.SetScore(0);
        scoreTextplayer2.SetScore(0);
    }

    public void OnStartGameButtonClicked()
    {
        HideAllObject();
        scoreTextObject.SetActive(true);
        onStartGame?.Invoke();
    }

    public void OnStatGameButtonClicked()
    {
        HideAllObject();
        leadeBoardObject.SetActive(true);
        ShowLeaderboard();
    }
    public void OnDeleteStatGameButtonClicked()
    {
        HideAllObject();
        leadeBoardObject.SetActive(true);
        loginObject.SetActive(true);
        adminlogin.text = "";
        adminPassword.text = "";
    }

    public void OnAproveDeleteStatGameButtonClicked()
    {
        if (dBService.DeletePlayersStat(adminlogin.text, adminPassword.text))
        {
            successDelete.SetActive(true);
            errorLogPass.SetActive(false);
        }
        else
        {
            successDelete.SetActive(false);
            errorLogPass.SetActive(true);
        }
    }

    public void OnGameEnds(int winnerId)
    {
        winnerBoardObject.SetActive(true);
        string winnerPlayer;
        if (winnerId == 1)
            winnerPlayer = string.IsNullOrWhiteSpace(playerOneInput.text) ? "PlayerOne" : playerOneInput.text;
        else
            winnerPlayer = string.IsNullOrWhiteSpace(playerTwoInput.text) ? "PlayerTwo" : playerTwoInput.text;
        winText.text = $"{winnerPlayer} wins!";
    }

    public void ShowLeaderboard()
    {
        // 1. Очистить старые строки
        for (int i = leaderboardContent.childCount - 1; i >= 0; i--)
        {
            Destroy(leaderboardContent.GetChild(i).gameObject);
        }

        // 2. Получить данные из БД
        var rows = dBService.GetLeaderboardRows(leaderboardLimit);
        // rows — это List<string[]> из метода, который делали раньше

        // 3. Для каждой строки из БД создать UI-строку
        foreach (var rowData in rows)
        {
            // rowData: [0]=Rank, [1]=Nick, [2]=Wins, [3]=Loses, [4]=Scored, [5]=Missed

            GameObject rowObj = Instantiate(leaderboardRowPrefab, leaderboardContent);
            rowObj.SetActive(true);

            var texts = rowObj.GetComponentsInChildren<TMPro.TextMeshProUGUI>();

            // Важно: порядок TextMeshPro в префабе должен совпадать с этим присвоением
            texts[0].text = rowData[0]; // Rank
            texts[1].text = rowData[1]; // Nick
            texts[2].text = rowData[2]; // Wins
            texts[3].text = rowData[3]; // Loses
            texts[4].text = rowData[4]; // Scored
            texts[5].text = rowData[5]; // Missed
        }
    }
}
