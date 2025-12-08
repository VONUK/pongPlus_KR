using Npgsql;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
}
