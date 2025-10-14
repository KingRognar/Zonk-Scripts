using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Extensions_Scr;

public class PlayerData_Scr : NetworkBehaviour
{
    public static PlayerData_Scr instance;

    [SerializeField] public Dictionary<ulong, PlayerNetData> playerDict = new Dictionary<ulong, PlayerNetData>();


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
}
 