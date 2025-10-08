using UnityEngine;

[CreateAssetMenu(fileName = "New Dice Material Set", menuName = "Scriptable Objects/Dice Material Set")]
public class DiceMaterialSetSO_Scr : ScriptableObject
{
    public Material baseMaterial;
    public Material dotsMaterial;
    public Sprite image;
}
