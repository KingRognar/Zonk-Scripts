using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bot List", menuName = "Scriptable Objects/Bot List")]
public class BotList_SO : ScriptableObject
{
    public List<BotData_SO> easyBots;
    public List<BotData_SO> midBots;
    public List<BotData_SO> hardBots;
}
