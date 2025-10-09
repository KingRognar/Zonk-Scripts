using System;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Extensions_Scr;

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

    //TODO: прибраться

    private List<Vector3> spawnPositions = new() {
        new Vector3 (0,0,-30), new Vector3 (0,0,30), new Vector3 (30,0,0), new Vector3(-30,0,0)};

    //TODO: нада придумать как получше загружать необходимые маериалы, сейчас я тупа гружу все через едиотр
    [SerializeField] private List<DiceMaterialSetSO_Scr> diceMaterialSets; 

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
        Debug.Log("тут2");
        if (!IsHost) return;
        foreach (ulong clientId in clientsCompleted)
            SpawnNewPlayerServerRpc(clientId);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SpawnNewPlayerServerRpc(netMan.LocalClientId);

        Debug.Log("тут1");
    }

    public void Middlenman(ulong clientId)
    {
        Debug.Log("тут2");
        if (!IsHost) return;
        SpawnNewPlayerServerRpc(clientId);
    }
    [ServerRpc]
    public void SpawnNewPlayerServerRpc(ulong clientId) //TODO: разделить на мелкие методы
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
        cupObj.GetComponent<NetworkObject>().AllowOwnerToParent = true;
        cupObj.transform.parent = newPlayer.transform;


        Vector3 handsPos = spawnPos.normalized * 45 + new Vector3(0, 8, 0);
        GameObject handsObj = Instantiate(handsPref, handsPos, Quaternion.identity);
        handsObj.transform.rotation *= Quaternion.LookRotation(-spawnPos, Vector3.up);
        handsObj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        handsObj.GetComponent<NetworkObject>().AllowOwnerToParent = true;
        handsObj.transform.parent = newPlayer.transform;

        List<GameObject> dices = new List<GameObject>();
        for (int i = 0; i < 6; i++)
        {
            GameObject diceObj = Instantiate(dicePref, transform.position, Quaternion.identity);
            dices.Add(diceObj);
            diceObj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            diceObj.GetComponent<NetworkObject>().AllowOwnerToParent = true;
            diceObj.transform.parent = newPlayer.transform;

        }

        LoadDiceColoringSchemes(dices);
        //CreateBackRefs(newPlayer, cupScr, dicesScr);
        //SetInitialPositions(newPlayer, cupScr, dicesScr);
    }
    private void LoadDiceColoringSchemes(List<GameObject> dices)
    {
        //TODO: нада придумать как получше загружать необходимые маериалы, сейчас я тупа гружу все через едиотр

        string saveFilePath = Application.persistentDataPath + "/diceColoring.json";
        int[] dicesColoringSchemeIds = new int[6] { 0, 0, 0, 0, 0, 0 };


        if (File.Exists(saveFilePath))
        {
            string jsonString = File.ReadAllText(saveFilePath);
            dicesColoringSchemeIds = JsonUtility.FromJson<IntArrayWrapper>(jsonString).intArray;
        }

        for (int i = 0; i < 6; i++)
        {
            Renderer renderer = dices[i].GetComponent<Renderer>();
            List<Material> materials = diceMaterialSets[dicesColoringSchemeIds[i]].materials;
            renderer.SetMaterials(materials);
        }
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
