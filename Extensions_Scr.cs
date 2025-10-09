using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Extensions_Scr
{
    [Serializable]
    public struct IntArrayWrapper
    {
        public int[] intArray;

        public IntArrayWrapper(int[] array)
        {
            intArray = array;
        }
    }

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
    public static Vector3 GetPositionRelativeToPlayer(this Player_Scr player, Vector3 vector)
    {
        Vector3 result = Vector3.zero;
        Transform playerTrans = player.transform;

        if (-playerTrans.position.normalized == -Vector3.forward)
            vector = -vector;
        else
            vector = Quaternion.FromToRotation(-playerTrans.position.normalized, Vector3.forward) * vector;
        result = playerTrans.position + vector;


        Debug.Log(result);
        return result;
    }
    public static Vector3 GetOppositeOffset(this Player_Scr player, Vector3 curPosition, float offset)
    {
        Vector3 result = Vector3.zero;
        Transform playerTrans = player.transform;

        if (playerTrans.position.x == 0)
            result += curPosition.x > 0 ? new Vector3(-offset, 0, 0) : new Vector3(offset, 0, 0);
        else
            result += curPosition.z > 0 ? new Vector3(0, 0, -offset) : new Vector3(0, 0, offset);

        return result;
    }
}
