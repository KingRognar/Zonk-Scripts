using Unity.Netcode;
using UnityEngine;

public class RPCManager_Scr : NetworkBehaviour
{
    public static RPCManager_Scr instance;

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
        NetworkManager_Scr.instance.playerData.playerDict.Add(ngoID, new PlayerData_Scr.PlayerNetData(steamID, steamName));
        Debug.Log(steamName + ": " + ngoID + ", " + steamID);
    }
}
