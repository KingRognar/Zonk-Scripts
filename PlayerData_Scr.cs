using System.Collections.Generic;
using UnityEngine;

public class PlayerData_Scr : MonoBehaviour
{
    [SerializeField] public Dictionary<ulong, PlayerNetData> playerDict = new Dictionary<ulong, PlayerNetData>();

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
 