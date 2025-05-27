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
    [SerializeField] private int playerTurn = 0;

    private NetworkManager netMan;

    //TODO: прибраться 
    //TODO: передача хода


    private List<Vector3> spawnPositions = new() {
        new Vector3 (0,0,-30), new Vector3 (0,0,30), new Vector3 (30,0,0), new Vector3(-30,0,0)};

    private void Start()
    {
        netMan = NetworkManager.Singleton;
        netMan.OnClientConnectedCallback += Middlenman;
        if (instance == null) instance = this;
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
        listOfPlayers.Add(newPlayer);
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

    
    [Rpc(SendTo.Server)]
    public void PlayerTurnEndRpc()
    {
        playerTurn++;
        if (playerTurn >= netMan.ConnectedClientsIds.Count) playerTurn = 0;

        ulong nextTarget = netMan.ConnectedClientsIds[playerTurn];
        RpcParams rpcParams = new RpcParams
        {
            Send = RpcTarget.Single(nextTarget, RpcTargetUse.Temp)
        };

         //TODO: могут быть проблемсы c мемори ликами

        listOfPlayers[(int)nextTarget].PlayerTurnStartRpc(rpcParams);
    }

}
