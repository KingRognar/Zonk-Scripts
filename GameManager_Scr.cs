using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class GameManager_Scr : NetworkBehaviour
{
    public static GameManager_Scr instance;

    [SerializeField] private GameObject playerPref;
    [SerializeField] private GameObject cupPref;
    [SerializeField] private GameObject dicePref;
    [SerializeField] private GameObject handsPref;
    [Space(10)]
    [SerializeField] private List<Player_Scr> listOfPlayers = new();
    [SerializeField] private int playerTurn = 0;

    private NetworkManager netMan;

    //TODO: ���������� 
    //TODO: �������� ����

    private List<Vector3> spawnPositions = new() {
        new Vector3 (0,0,-30), new Vector3 (0,0,30), new Vector3 (30,0,0), new Vector3(-30,0,0)};

    private void Awake()
    {
        netMan = NetworkManager.Singleton;
    }
    private void Start()
    {
        netMan.SceneManager.OnLoadEventCompleted += Middleman;
        //netMan.OnClientConnectedCallback += Middlenman;
        //netMan.SceneManager.OnLoadComplete += Middlenman;
        if (instance == null) instance = this;
    }

    private void Middleman(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Debug.Log("���2");
        if (!IsHost) return;
        foreach (ulong clientId in clientsCompleted)
            SpawnNewPlayerServerRpc(clientId);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SpawnNewPlayerServerRpc(netMan.LocalClientId);

        Debug.Log("���1");
    }

    public void Middlenman(ulong clientId)
    {
        Debug.Log("���2");
        if (!IsHost) return;
        SpawnNewPlayerServerRpc(clientId);
    }
    [ServerRpc]
    public void SpawnNewPlayerServerRpc(ulong clientId) //TODO: ��������� �� ������ ������
    {
        int id = (int)clientId;
        //TODO: ������ ����
        if (id >= 4)
        { Debug.Log("������� ��������� ���� ���-�� �������"); return; } 


        Vector3 spawnPos = spawnPositions[id];
        Player_Scr newPlayer = Instantiate(playerPref, spawnPos, Quaternion.identity).GetComponent<Player_Scr>();
        listOfPlayers.Add(newPlayer);
        newPlayer.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

        GameObject cupObj = Instantiate(cupPref, transform.position, Quaternion.identity);
        Cup_Scr cupScr = cupObj.GetComponent<Cup_Scr>();
        cupObj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        cupObj.GetComponent<NetworkObject>().AllowOwnerToParent = true;
        cupObj.transform.parent = newPlayer.transform;


        Vector3 handsPos = spawnPos.normalized * 45 + new Vector3(0, 8, 0);
        GameObject handsObj = Instantiate(handsPref, handsPos, Quaternion.identity);
        handsObj.transform.rotation *= Quaternion.LookRotation(-spawnPos, Vector3.up);
        handsObj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        handsObj.GetComponent<NetworkObject>().AllowOwnerToParent = true;
        handsObj.transform.parent = newPlayer.transform;

        List<Dice_Scr> dicesScr = new();
        for (int i = 0; i < 6; i++)
        {
            GameObject diceObj = Instantiate(dicePref, transform.position, Quaternion.identity);
            dicesScr.Add(diceObj.GetComponent<Dice_Scr>());
            diceObj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            diceObj.GetComponent<NetworkObject>().AllowOwnerToParent = true;
            diceObj.transform.parent = newPlayer.transform;

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

         //TODO: ����� ���� ��������� c ������ ������

        listOfPlayers[(int)nextTarget].PlayerTurnStartRpc(rpcParams);
    }

}
