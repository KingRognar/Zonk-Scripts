using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static Extensions_Scr;

public class RPCManager_Scr : NetworkBehaviour
{
    public static RPCManager_Scr instance;
    [SerializeField] private UI_HostGame_Scr hostGameUI;

    //TODO: просто брать данные из стим лобби или ngo (+ возможно наконец связать в плейер.скр)

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


    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void AddPlayerToDictionaryServerRPC(ulong ngoID, ulong steamID, string steamName)
    {
        PlayerData_Scr.instance.playerDict.Add(ngoID, new PlayerNetData(steamID, steamName));
        Debug.Log(steamName + ": " + ngoID + ", " + steamID);
        UpdateLobbyNamesServerRPC();
    }
    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void UpdateLobbyNamesServerRPC()
    {
        foreach (KeyValuePair<ulong, PlayerNetData> keyValuePair in PlayerData_Scr.instance.playerDict)
        {
            UpdateLobbyNameClientRPC(keyValuePair.Key, keyValuePair.Value.steamName);
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateLobbyNameClientRPC(ulong ngoID, string steamName)
    {
        hostGameUI.playerNames[(int)ngoID].text = steamName;
    }

    [Rpc(SendTo.Server)]
    public void ChangeNameServerRpc(int playerNum, FixedString128Bytes name)
    {
        Debug.Log("ChangeNameServerRpc " + playerNum + " " + name);
        //nameNV.Value = name;
        //playerNamesNV[playerNum].Value = name;
        ChangeNameClientRpc(playerNum);

    }
    [Rpc(SendTo.ClientsAndHost)]
    private void ChangeNameClientRpc(int playerNum)
    {
        Debug.Log("ChangeNameClientRpc " + playerNum);
        //hostGameUI.playerNames[playerNum].text = playerNamesNV[playerNum].Value.ToString();
    }
}
