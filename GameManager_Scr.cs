using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class GameManager_Scr : NetworkBehaviour
{
    public static GameManager_Scr instance;

    [SerializeField] private GameObject playerPref;
    [SerializeField] private GameObject CupPref;
    [SerializeField] private GameObject DicePref;
    [SerializeField] private List<Player_Scr> listOfPlayers = new();
    private NetworkVariable<int> numberOfPlayers = new NetworkVariable<int>(0);
    [Space(10)]
    private Transform canvasTrans;
    [SerializeField] private GameObject scorePref;
    [SerializeField] private GameObject turnScorePref;
    [SerializeField] private GameObject endBtnPref;

    private NetworkManager netMan;

    private List<Vector3> spawnPositions = new() {
        new Vector3 (0,0,-30), new Vector3 (0,0,30), new Vector3 (30,0,0), new Vector3(-30,0,0)};

    private void Start()
    {
        netMan = NetworkManager.Singleton;
        netMan.OnClientConnectedCallback += Middlenman;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log("тут1");
    }


    public void Middlenman(ulong clientId)
    {
        Debug.Log("тут2");
        SpawnNewPlayerServerRpc(clientId);
    }
    [ServerRpc]
    public void SpawnNewPlayerServerRpc(ulong clientId)
    {
        int id = (int)clientId;
        //TODO: защиту нада
        if (id >= 4)
        { Debug.Log("Попытка превысить макс кол-во игроков"); return; } 


        Vector3 spawnPos = spawnPositions[id];
        Quaternion spawnQuat = Quaternion.LookRotation(-spawnPos, Vector3.up);
        Player_Scr newPlayer = Instantiate(playerPref, spawnPos, spawnQuat).GetComponent<Player_Scr>();
        newPlayer.GetComponent<NetworkObject>().Spawn(true);
        listOfPlayers.Add(newPlayer);

        Cup_Scr newCup = SpawnCup(spawnPos);
        List<Dice_Scr> newDices = SpawnDices(spawnPos);
        canvasTrans = GameObject.Find("Canvas").transform;
        newPlayer.SetupPlayer(newCup, newDices);
        newPlayer.Initialize();
        SetupCameraClientRpc(clientId);
    }
    [ClientRpc]
    private void SetupCameraClientRpc(ulong clientID)
    {
        if (OwnerClientId != clientID)
            return;
        Camera.main.tag = "Untagged";
        Player_Scr.players[(int)clientID].transform.GetChild(0).tag = "MainCamera";
    }

    private Cup_Scr SpawnCup(Vector3 playerPos)
    {
        Vector3 vectorA = -playerPos.normalized;
        Vector3 vectorB = Vector3.up;
        Vector3 spawnPos = Vector3.Cross(vectorA, vectorB) * 10f;

        GameObject cup = Instantiate(CupPref, playerPos + spawnPos, Quaternion.identity);
        cup.GetComponent<NetworkObject>().Spawn(true);
        return cup.GetComponent<Cup_Scr>();
    }
    private List<Dice_Scr> SpawnDices(Vector3 playerPos)
    {
        List<Dice_Scr> dices = new();
        Vector3 vectorA = -playerPos.normalized;
        Vector3 vectorB = -Vector3.up;
        Vector3 spawnPos = Vector3.Cross(vectorA, vectorB) * 10f;

        Vector2 regionSize = Vector2.one * 8f;
        List<Vector2> diceOffsets;
        int tries = 10;
        do
        {
            diceOffsets = PoissonDiscSampling_Scr.GeneratePoints(2.83f, regionSize, 30);
        } while (diceOffsets.Count < 6 && tries-- > 0);

        for (int i = 0; i < 6; i++)
        {
            Vector3 dicePos = spawnPos + new Vector3(0, 1, 0);
            dicePos += new Vector3(diceOffsets[i].x - regionSize.x / 2, 0, diceOffsets[i].y - regionSize.y / 2);
            Dice_Scr dice = Instantiate(DicePref, playerPos + dicePos, Quaternion.identity).GetComponent<Dice_Scr>();
            dice.GetComponent<NetworkObject>().Spawn(true);

            dices.Add(dice);
        }

        return dices;
    }
}
