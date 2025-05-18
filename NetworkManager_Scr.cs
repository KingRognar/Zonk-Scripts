using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class NetworkManager_Scr : MonoBehaviour
{
    public GameObject cupPrefab;
    private Vector3[] spawnPositions = new Vector3[4] {
        new Vector3 (0,0,0),
        new Vector3 (-30,0,0),
        new Vector3 (30,0,0),
        new Vector3 (0,0,30)};

    private NetworkManager NM;

    private void Awake()
    {
        NM = GetComponent<NetworkManager>();
        NM.OnClientConnectedCallback += AddNewPlayer;
    }

    private void AddNewPlayer(ulong clientId)
    {
        //NewPlayerServerRpc(clientId);
    }
    [ServerRpc]
    private void NewPlayerServerRpc(ulong clientId)
    {
        //GameManager_Scr.instance.SpawnNewPlayer();
        GameObject newCup = Instantiate(cupPrefab, spawnPositions[clientId], Quaternion.identity);
        newCup.GetComponent<NetworkObject>().Spawn();
    }
}
