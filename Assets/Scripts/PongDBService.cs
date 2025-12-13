using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class PongDBService : MonoBehaviour
{
    [Header("DB Connection")]
    [SerializeField] private string host = "localhost";
    [SerializeField] private int port = 5432;
    [SerializeField] private string database = "pongPlus";
    [SerializeField] private string user = "postgres";
    [SerializeField] private string password = "admin";

    public TextMeshProUGUI DBStatus;
    public GameObject DBStatusFiled;

    private string _connectionString;

    private void Awake()
    {
        _connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password}";
    }

    private void Start()
    {
        TestConnection();
    }

    public void TestConnection()
    {
        try
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                Debug.Log("[DB - pongPlus][UP]");
                DBStatusFiled.SetActive(true);
                DBStatus.text = "Connected DB";
                DBStatus.color = Color.green;
            }
        }
        catch (System.Exception error)
        {
            DBStatusFiled.SetActive(true);
            Debug.LogError("[DB - pongPlus][Connection ERROR] - " + error.Message);
            DBStatus.text = "No connection DB";
            DBStatus.color = Color.red;
        }
    }

    public void SaveMatchResult(
        string playerOneNickname,
        string playerTwoNickname,
        int scorePlayer1,
        int scorePlayer2
        )
    {
        string[] players = { playerOneNickname, playerTwoNickname };
        for (int i = 0; i < players.Length; i++) 
        {
            string nickname = GetPlayerIdByNickname(players[i]);
            if (nickname != "IsNull" && nickname != "ErrorPOST")
            {
                Debug.Log("GOOOOOOOOOOOL! nicknamePlayer" + (i+1)  + "=\"" + nickname + "\" in database");

            }
            else if (nickname == "IsNull")
                Debug.Log("SKIP: nicknamePlayer"+ (i+1) + " is empty");
        }
    }

    // ErrorPOST - Ошибка запроса; IsNull - значение null;
    public string GetPlayerIdByNickname(string playerNickname)
    {
        if (playerNickname == "")
            return "IsNull";
        try
        {
            const string checkNicknamePOST = "SELECT playerid FROM players WHERE playernickname = @nickname LIMIT 1;";
            using (var connCheckNickname = new NpgsqlConnection(_connectionString))
            {
                connCheckNickname.Open();
                using (var cmdcheckNicknamePOST = new NpgsqlCommand(checkNicknamePOST, connCheckNickname))
                {
                    cmdcheckNicknamePOST.Parameters.AddWithValue("nickname", playerNickname);
                    var checkResult = cmdcheckNicknamePOST.ExecuteScalar();
                    if (checkResult == null || checkResult == System.DBNull.Value)
                    {
                        const string insertPlayerPOST = @"
                            INSERT INTO players (groupid, playernickname, datacreation)
                            VALUES (@groupid, @nickname, @datacreation)
                            RETURNING playerid;
                        ";
                        using (var insertCmd = new NpgsqlCommand(insertPlayerPOST, connCheckNickname))
                        {
                            insertCmd.Parameters.AddWithValue("groupid", 2);
                            insertCmd.Parameters.AddWithValue("nickname", playerNickname);
                            insertCmd.Parameters.AddWithValue("datacreation", DateTime.Now);

                            var newId = insertCmd.ExecuteScalar();
                            return Convert.ToString(newId);
                        }
                    }
                    return Convert.ToString(checkResult);
                }
            }
        }
        catch (System.Exception errorCheckNicknamePOST)
        {
            Debug.LogError("[DB][Errore in checkNicknamePOST]: " + errorCheckNicknamePOST.Message);
            return "ErrorPOST";
        }
    }

    public int CreateMatch()
    {
        return 0;
    }
}
