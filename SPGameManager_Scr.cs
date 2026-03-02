using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Extensions_Scr;

public class SPGameManager_Scr : MonoBehaviour
{
    public static SPGameManager_Scr instance;

    [SerializeField] private GameObject singlePlayerPref;
    [Space(10)]
    [SerializeField] private List<Player_Scr> listOfPlayers = new();
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



    public void SpawnPlayer()
    {
        SinglePlayer_Scr sPlayer = Instantiate(singlePlayerPref, spawnPositions[0], Quaternion.identity).GetComponent<SinglePlayer_Scr>();
        sPlayer.Initialize();
    }
    public void SpawnBots()
    {

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
