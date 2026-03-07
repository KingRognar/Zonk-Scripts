using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Extensions_Scr;

public class SPGameManager_Scr : MonoBehaviour
{
    public static SPGameManager_Scr instance;

    [SerializeField] private GameObject singlePlayerPref;
    [SerializeField] private GameObject botPlayerPref;
    [Space(10)]
    [SerializeField] private SinglePlayer_Scr mainPlayer;
    [SerializeField] private List<BotPlayer_Scr> bots;
    //[SerializeField] private List<Player_Scr> listOfPlayers = new(); переделать на ботов
    private int numberOfPlayers = 2;
    [SerializeField] private int playerTurn = 0;
    [SerializeField] private int[] playerScores = new int[4];
    private int maxScore = 4000; //TODO: sync it

    //TODO: прибраться

    private List<Vector3> spawnPositions = new() {
        new Vector3 (0,0,-30), new Vector3 (0,0,30), new Vector3 (30,0,0), new Vector3(-30,0,0)};

    private void Awake()
    {
        SceneManager.activeSceneChanged += ChangedActiveScene;
    }
    private void Start()
    {
        if (instance == null) instance = this;
    }



    private void SpawnPlayer()
    {
        mainPlayer = Instantiate(singlePlayerPref, spawnPositions[0], Quaternion.identity).GetComponent<SinglePlayer_Scr>();
        mainPlayer.transform.rotation *= Quaternion.LookRotation(-spawnPositions[0], Vector3.up);
        mainPlayer.Initialize();
        mainPlayer.spGM = this;
        mainPlayer.isMyTurn = true;


    }
    private void SpawnBots()
    {
        for (int i = 1; i < numberOfPlayers; i++)
        {
            int botId = i - 1;
            bots.Add(Instantiate(botPlayerPref, spawnPositions[i], Quaternion.identity).GetComponent<BotPlayer_Scr>());
            bots[botId].transform.rotation *= Quaternion.LookRotation(-spawnPositions[i], Vector3.up);
            bots[botId].Initialize();
            bots[botId].spGM = this;


        }
    }

    public void TurnPass()
    {
        if (++playerTurn >= numberOfPlayers)
            playerTurn = 0;

        if (playerTurn == 0)
        {
            mainPlayer.StartTurn();
            return;
        }

        int botId = playerTurn - 1;
        bots[botId].StartTurn();
    }

    private void ChangedActiveScene(Scene current, Scene next)
    {
        //TODO: мб проверять сцены по имени?
        /*string currentName = current.name;

        if (currentName == null)
        {
            // Scene1 has been removed
            currentName = "Replaced";
        }

        Debug.Log("Scenes: " + currentName + ", " + next.name);*/
        SpawnPlayer();
        SpawnBots();
    }
}
