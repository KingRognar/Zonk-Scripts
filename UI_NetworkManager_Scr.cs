using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UI_NetworkManager_Scr : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    private void Awake()
    {
        hostBtn.onClick.AddListener(() => { NetworkManager.Singleton.StartHost(); });
        clientBtn.onClick.AddListener(() => {NetworkManager.Singleton.StartClient(); });
    }
}
