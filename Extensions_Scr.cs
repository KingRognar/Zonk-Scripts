using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public static class Extensions_Scr
{
    public static int[] SubtractArray(this int[] array, int[] subArray)
    {
        int i = 0;
        int[] result = new int[array.Length];
        while (i < array.Length && i < subArray.Length)
        {
            result[i] = array[i] - subArray[i++];
        }

        return result;
    }
    public static Vector3 GetRandomDirection(this Transform trans)
    {
        int rand = Random.Range(0, 6);
        switch (rand)
        {
            case 0: return -Vector3.forward; //0
            case 1: return -Vector3.up; //1
            case 2: return Vector3.right; //2
            case 3: return -Vector3.right; //3
            case 4: return Vector3.up; //4
            case 5: return Vector3.forward; //5
            default: return Vector3.zero;
        }
    }
}
