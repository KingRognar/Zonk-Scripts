using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UI_NetworkManager_Scr : NetworkBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private GameObject GMPrefab;

    private void Awake()
    {
        hostBtn.onClick.AddListener(StartHost);
        clientBtn.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        if (!NetworkManager.Singleton.StartHost())
            return;
        //GameObject gm = Instantiate(GMPrefab, Vector3.zero, Quaternion.identity);
        //gm.GetComponent<NetworkObject>().Spawn();
        //GameManager_Scr.instance.SpawnNewPlayer();
    }
    private void StartClient()
    {
        if (!NetworkManager.Singleton.StartClient())
            return;

        //GameManager_Scr.instance.SpawnNewPlayer();
    }
}
