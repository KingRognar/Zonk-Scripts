using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class GameManager_Scr : NetworkBehaviour
{
    public static GameManager_Scr instance;

    [SerializeField] private GameObject playerPref;
    [SerializeField] private GameObject cupPref;
    [SerializeField] private GameObject dicePref;
    [SerializeField] private List<Player_Scr> listOfPlayers = new();
    private NetworkVariable<int> numberOfPlayers = new NetworkVariable<int>(0);
    [Space(10)]
    private Transform canvasTrans;
    [SerializeField] private GameObject scorePref;
    [SerializeField] private GameObject turnScorePref;
    [SerializeField] private GameObject endBtnPref;

    private NetworkManager netMan;

    //TODO: прибраться 
    //TODO: передача хода


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
        if (!IsHost) return;
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
        Player_Scr newPlayer = Instantiate(playerPref, spawnPos, Quaternion.identity).GetComponent<Player_Scr>();
        newPlayer.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

        GameObject cupObj = Instantiate(cupPref, transform.position, Quaternion.identity);
        Cup_Scr cupScr = cupObj.GetComponent<Cup_Scr>();
        cupObj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        cupObj.transform.parent = newPlayer.transform;
        cupObj.GetComponent<NetworkObject>().AllowOwnerToParent = true;

        List<Dice_Scr> dicesScr = new();
        for (int i = 0; i < 6; i++)
        {
            GameObject diceObj = Instantiate(dicePref, transform.position, Quaternion.identity);
            dicesScr.Add(diceObj.GetComponent<Dice_Scr>());
            diceObj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            diceObj.transform.parent = newPlayer.transform;
            diceObj.GetComponent<NetworkObject>().AllowOwnerToParent = true;
        }

        //CreateBackRefs(newPlayer, cupScr, dicesScr);
        //SetInitialPositions(newPlayer, cupScr, dicesScr);
    }
    private void CreateBackRefs(Player_Scr player, Cup_Scr cup, List<Dice_Scr> dices)
    {
        player.cup = cup;
        player.diceSet = dices;

        cup.player = player;

        for (int i = 0; i < dices.Count; i++)
        {
            dices[i].player = player;
            dices[i].id = i;
        }
    }
    private void SetInitialPositions(Player_Scr player, Cup_Scr cup, List<Dice_Scr> dices)
    {
        //CUP
        Vector3 vectorA = -player.transform.position.normalized;
        Vector3 vectorB = Vector3.up;
        Vector3 posOffset = Vector3.Cross(vectorA, vectorB) * 10f;

        cup.transform.position = player.transform.position + posOffset;

        //DICES
        posOffset = -posOffset;
        Vector2 regionSize = Vector2.one * 8f;
        List<Vector2> diceOffsets;
        int tries = 10;

        do
        {
            diceOffsets = PoissonDiscSampling_Scr.GeneratePoints(2.83f, regionSize, 30);
        } while (diceOffsets.Count < 6 && tries-- > 0);

        for (int i = 0; i < 6; i++)
        {
            Vector3 dicePos = posOffset + new Vector3(0, 1, 0);
            dicePos += new Vector3(diceOffsets[i].x - regionSize.x / 2, 0, diceOffsets[i].y - regionSize.y / 2);

            dices[i].transform.position = player.transform.position + dicePos;
            player.RollDice(dices[i].transform);
        }
    }

}
