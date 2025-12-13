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
                Debug.Log("[DB - pongPlus][GOOOOOOOOOOOL!]");
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
        int scorePlayer2,
        DateTime matchStart,
        int winnerId
        )
    {
        int[] verifiedPlayers = new int[2];
        if (GetPlayerIdByNickname(playerOneNickname) == "IsNull" && GetPlayerIdByNickname(playerTwoNickname) == "IsNull")
        {
            Debug.Log("SKIP: All nicknamePlayers is empty");
        }
        else
        {
            string[] players = { playerOneNickname, playerTwoNickname };
            for (int i = 0; i < players.Length; i++)
            {
                string nicknameResult = GetPlayerIdByNickname(players[i]);
                if (nicknameResult != "IsNull" && nicknameResult != "ErrorPOST")
                {
                    verifiedPlayers[i] = Convert.ToInt32(nicknameResult);
                    Debug.Log("GOOOOOOOOOOOL! nicknamePlayer" + (i + 1) + "=\"" + players[i] + "\" in database");
                    bool isWinner = (winnerId == (i + 1));
                    int scored = (i == 0) ? scorePlayer1 : scorePlayer2;
                    int missed = (i == 0) ? scorePlayer2 : scorePlayer1;

                    UpsertPlayerStats(verifiedPlayers[i], scored, missed, isWinner, players[i]);
                }
                else if (nicknameResult == "IsNull")
                {
                    verifiedPlayers[i] = (i == 0) ? 12 : 11;
                    Debug.Log("SKIP: nicknamePlayer" + (i + 1) + " is empty");
                }
                else
                {
                    verifiedPlayers[i] = (i == 0) ? 12 : 11;
                    Debug.LogError("[DB][Error connect] - nickname Player" + (i + 1) + " is SYSval");
                }
            }
            try
            {
                const string insertMatchSql = @"
                INSERT INTO matches (playerone, playertwo, pl1score, pl2score, matchstart, matchend, levelid)
                VALUES (
                    @playerone,
                    @playertwo,
                    @pl1score,
                    @pl2score,
                    @matchstart,
                    @matchend,
                    @levelid
                );";
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(insertMatchSql, conn))
                    {
                        cmd.Parameters.AddWithValue("playerone", verifiedPlayers[0]);
                        cmd.Parameters.AddWithValue("playertwo", verifiedPlayers[1]);
                        cmd.Parameters.AddWithValue("pl1score", scorePlayer1);
                        cmd.Parameters.AddWithValue("pl2score", scorePlayer2);
                        cmd.Parameters.AddWithValue("matchstart", matchStart);
                        cmd.Parameters.AddWithValue("matchend", DateTime.Now);
                        cmd.Parameters.AddWithValue("levelid", 0);
                        cmd.ExecuteNonQuery();
                    }
                }
                Debug.Log("GOOOOOOOOOOOL! match is save");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[DB][Error in SaveMatchResult - insert match]: " + ex.Message);
            }
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
            Debug.LogError("[DB][Errore in GetPlayerIdByNickname - request playerid]: " + errorCheckNicknamePOST.Message);
            return "ErrorPOST";
        }
    }

    public void UpsertPlayerStats(int playerId, int scored, int missed, bool isWinner, string nickname)
    {
        try
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                const string selectSql = @"
                SELECT scoredpoints, missedpoints, winmatches, losmatches
                FROM playersstats
                WHERE playerid = @playerid;
            ";

                int currentScored = 0;
                int currentMissed = 0;
                int currentWins = 0;
                int currentLosses = 0;

                using (var selectCmd = new NpgsqlCommand(selectSql, conn))
                {
                    selectCmd.Parameters.AddWithValue("playerid", playerId);
                    using (var reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            currentScored = reader.GetInt32(0);
                            currentMissed = reader.GetInt32(1);
                            currentWins = reader.GetInt32(2);
                            currentLosses = reader.GetInt32(3);
                        }
                    }
                }
                int newScored = currentScored + scored;
                int newMissed = currentMissed + missed;
                int newWins = currentWins + (isWinner ? 1 : 0);
                int newLosses = currentLosses + (isWinner ? 0 : 1);

                if (currentScored == 0 && currentMissed == 0 && currentWins == 0 && currentLosses == 0)
                {
                    const string insertSql = @"
                    INSERT INTO playersstats (playerid, scoredpoints, missedpoints, winmatches, losmatches)
                    VALUES (@playerid, @scored, @missed, @wins, @losses);
                ";
                    using (var insertCmd = new NpgsqlCommand(insertSql, conn))
                    {
                        insertCmd.Parameters.AddWithValue("playerid", playerId);
                        insertCmd.Parameters.AddWithValue("scored", newScored);
                        insertCmd.Parameters.AddWithValue("missed", newMissed);
                        insertCmd.Parameters.AddWithValue("wins", newWins);
                        insertCmd.Parameters.AddWithValue("losses", newLosses);
                        insertCmd.ExecuteNonQuery();
                    }
                    Debug.Log("GOOOOOOOOOOOL! Player =\"" + nickname + "\" statistics have been created");
                }
                else
                {
                    const string updateSql = @"
                    UPDATE playersstats
                    SET scoredpoints = @scored,
                        missedpoints = @missed,
                        winmatches   = @wins,
                        losmatches   = @losses
                    WHERE playerid = @playerid;
                ";
                    using (var updateCmd = new NpgsqlCommand(updateSql, conn))
                    {
                        updateCmd.Parameters.AddWithValue("playerid", playerId);
                        updateCmd.Parameters.AddWithValue("scored", newScored);
                        updateCmd.Parameters.AddWithValue("missed", newMissed);
                        updateCmd.Parameters.AddWithValue("wins", newWins);
                        updateCmd.Parameters.AddWithValue("losses", newLosses);
                        updateCmd.ExecuteNonQuery();
                    }
                    Debug.Log("GOOOOOOOOOOOL! Player =\"" + nickname + "\" statistics have been update");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[DB][Error in UpsertPlayerStats]: " + ex.Message);
        }
    }

    public List<string[]> GetLeaderboardRows(int limit)
    {
        var rows = new List<string[]>();
        try
        {
            const string sql = @"
            SELECT
                p.playernickname,
                ps.winmatches,
                ps.losmatches,
                ps.scoredpoints,
                ps.missedpoints
            FROM public.players p
            JOIN public.playersstats ps ON p.playerid = ps.playerid
            ORDER BY ps.winmatches DESC, ps.losmatches ASC, ps.scoredpoints DESC, ps.missedpoints ASC
            LIMIT @limit;
        ";

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("limit", limit);

                    using (var reader = cmd.ExecuteReader())
                    {
                        int rank = 1;

                        while (reader.Read())
                        {
                            string nickname = reader.GetString(0);
                            string wins = reader.GetInt32(1).ToString();
                            string loses = reader.GetInt32(2).ToString();
                            string scored = reader.GetInt32(3).ToString();
                            string missed = reader.GetInt32(4).ToString();

                            // массив строк для одной строки таблицы
                            rows.Add(new string[]
                            {
                            rank.ToString(), // место
                            nickname,
                            wins,
                            loses,
                            scored,
                            missed
                            });

                            rank++;
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("[DB][Error in GetLeaderboardRows]: " + ex.Message);
        }

        return rows;
    }
}
