using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dice Material Set", menuName = "Scriptable Objects/Dice Material Set")]
public class DiceMaterialSetSO_Scr : ScriptableObject
{
    public List<Material> materials;
    public Sprite image;
}
