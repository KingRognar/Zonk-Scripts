using System;
using System.Collections.Generic;
using DG.Tweening;
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
    private bool weHaveWinner = false;
    [SerializeField] private int[] playerScores = new int[4];
    private int maxScore = 4000; //TODO: sync it

    //TODO: прибраться

    [SerializeField] private BotList_SO botDataList;

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
        mainPlayer.spGM = this;
        mainPlayer.isMyTurn = true;
        mainPlayer.maxScore = maxScore;
        mainPlayer.Initialize();


    }
    private void SpawnBots()
    {
        for (int i = 1; i < numberOfPlayers; i++)
        {
            int botId = i - 1;
            bots.Add(Instantiate(botPlayerPref, spawnPositions[i], Quaternion.identity).GetComponent<BotPlayer_Scr>());

            bots[botId].transform.rotation *= Quaternion.LookRotation(-spawnPositions[i], Vector3.up);
            bots[botId].botId = botId;
            bots[botId].data = botDataList.midBots[0]; //TODO: сделать рандомизацию
            bots[botId].spGM = this;
            bots[botId].maxScore = maxScore;
            bots[botId].Initialize();


        }
    }

    public void TurnPass()
    {
        CheckScores();

        if (weHaveWinner)
            return;

        //TODO: добавить попап для возврата в main menu

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
    private void CheckScores()
    {

        if (mainPlayer.score >= maxScore)
        {
            PlayerWon();
            return;
        }

        for (int i = 0; i < bots.Count; i++)
            if (bots[i].score >= maxScore)
            {
                BotWon(i);
                return;
            }
    }
    private void PlayerWon()
    {
        weHaveWinner = true;

        UI_Manager_Scr.instance.ChangeYourTurnToNewText("You Won!");
        UI_Manager_Scr.instance.ShowYourTurn();
    }
    private void BotWon(int botId)
    {
        weHaveWinner = true;   

        string botName = bots[botId].data.name;

        UI_Manager_Scr.instance.ChangeYourTurnToNewText(botName + " Won!");
        UI_Manager_Scr.instance.ShowYourTurn();

        Sequence sequence = DOTween.Sequence(this);
        sequence.AppendInterval(1);
        sequence.AppendCallback(() =>
        {
            UI_Manager_Scr.instance.ChangeYourTurnToNewText("And You...");
            UI_Manager_Scr.instance.ShowYourTurn();
        });
        sequence.AppendInterval(1);
        sequence.AppendCallback(() =>
        {
            UI_Manager_Scr.instance.ChangeYourTurnToNewText("LOST");
            UI_Manager_Scr.instance.ShowYourTurn();
        });
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
        //TODO: добавить destroy on load
    }
}
