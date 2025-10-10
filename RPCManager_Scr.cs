using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class RPCManager_Scr : NetworkBehaviour
{
    public static RPCManager_Scr instance;
    [SerializeField] private UI_HostGame_Scr hostGameUI;

    public NetworkVariable<FixedString128Bytes>[] playerNamesNV = new NetworkVariable<FixedString128Bytes>[4] { new NetworkVariable<FixedString128Bytes>("") , new NetworkVariable<FixedString128Bytes>("") , new NetworkVariable<FixedString128Bytes>("") , new NetworkVariable<FixedString128Bytes>("") };


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }
    private void onNameChange(FixedString128Bytes oldString, FixedString128Bytes newString)
    {

    }


    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void AddPlayerToDictionaryServerRPC(ulong ngoID, ulong steamID, string steamName)
    {
        NetworkManager_Scr.instance.playerData.playerDict.Add(ngoID, new PlayerData_Scr.PlayerNetData(steamID, steamName));
        Debug.Log(steamName + ": " + ngoID + ", " + steamID);
    }

    [Rpc(SendTo.Server)]
    public void ChangeNameServerRpc(int playerNum, FixedString128Bytes name)
    {
        Debug.Log("ChangeNameServerRpc " + playerNum + " " + name);
        //nameNV.Value = name;
        playerNamesNV[playerNum].Value = name;
        ChangeNameClientRpc(playerNum);

    }
    [Rpc(SendTo.ClientsAndHost)]
    private void ChangeNameClientRpc(int playerNum)
    {
        Debug.Log("ChangeNameClientRpc " + playerNum);
        hostGameUI.playerNames[playerNum].text = playerNamesNV[playerNum].Value.ToString();
    }
}
