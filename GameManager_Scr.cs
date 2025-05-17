using System.Collections.Generic;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class GameManager_Scr : NetworkBehaviour
{
    public static GameManager_Scr instance;

    [SerializeField] private GameObject playerPref;
    [SerializeField] private GameObject CupPref;
    [SerializeField] private GameObject DicePref;
    [SerializeField] private List<Player_Scr> listOfPlayers = new();
    private NetworkVariable<int> numberOfPlayers = new NetworkVariable<int>(0);

    private List<Vector3> spawnPositions = new() {
        new Vector3 (0,0,-30), new Vector3 (0,0,30), new Vector3 (30,0,0), new Vector3(-30,0,0)};

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnNewPlayer();
        }

    }

    public void SpawnNewPlayer()
    {
        //TODO: защиту нада
        if (numberOfPlayers.Value >= 4)
        { Debug.Log("ѕопытка превысить макс кол-во игроков"); return; } 


        Vector3 spawnPos = spawnPositions[numberOfPlayers.Value];
        Quaternion spawnQuat = Quaternion.LookRotation(-spawnPos, Vector3.up);
        Player_Scr newPlayer = Instantiate(playerPref, spawnPos, spawnQuat).GetComponent<Player_Scr>();
        newPlayer.GetComponent<NetworkObject>().Spawn(true);
        listOfPlayers.Add(newPlayer);
        if (IsSpawned)
            numberOfPlayers.Value++;

        Cup_Scr newCup = SpawnCup(spawnPos);
        List<Dice_Scr> newDices = SpawnDices(spawnPos);
        newPlayer.SetupPlayer(newCup, newDices);
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

    public virtual void OnStartClient()
    {
        //NewPlayerServerRpc();
    }


}
