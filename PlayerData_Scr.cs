using System.Collections.Generic;
using UnityEngine;

public class PlayerData_Scr : MonoBehaviour
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

    public struct PlayerNetData
    {
        public PlayerNetData(ulong steamid, string name)
        {
            steamID = steamid;
            steamName = name;
        }

        public ulong steamID;
        public string steamName;
    }
}
 