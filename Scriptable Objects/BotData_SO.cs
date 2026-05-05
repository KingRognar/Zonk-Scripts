using UnityEngine;

[CreateAssetMenu(fileName = "New Bot Data", menuName = "Scriptable Objects/Bot Data")]
public class BotData_SO : ScriptableObject
{
    public string botName;
    //public difficulty;
    public Texture2D portrait;
    public GameObject headModel;
}
