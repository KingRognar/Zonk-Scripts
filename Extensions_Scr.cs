using System;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
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
    public struct UiRefs
    {
        public TMP_Text turnScore;
        public TMP_Text totalScore;
        public Button endTurn;
        public Scores_Scr playersScores;
        public GameObject yourTurnSign;
    }


    public enum CupState
    {
        empty,
        filling,
        filled,
        overturned,
    }


    public class Boundaries
    {
        //TODO: возможно нужно сделать разные границы для правой и левой руки

        private Vector3 bottomLeftPoint;
        private Vector3 bottomVector;
        private Vector3 leftVector;

        private float bottomSqreMagn;
        private float leftSqreMagn;

        public bool initialized = false;

        public void SetupBoundaries(Plane plane, Transform playerTrans, float minHor, float maxHor, float minVer, float maxVer)
        {
            Vector3 planeFirstPos = plane.ClosestPointOnPlane(Vector3.zero);
            bottomLeftPoint = planeFirstPos + playerTrans.right * minHor;
            bottomVector = playerTrans.right * (maxHor - minHor);
            leftVector = playerTrans.up * (maxVer - minVer);

            bottomSqreMagn = bottomVector.sqrMagnitude;
            leftSqreMagn = leftVector.sqrMagnitude;

            initialized = true;
        }
        public Vector3 ClampPointToBoundaries(Vector3 point)
        {
            if (!initialized) { Debug.Log("Boundaries not initialized"); return point; }

            Vector3 pointVector = point - bottomLeftPoint;

            float t1 = Vector3.Dot(pointVector, bottomVector) / bottomSqreMagn;
            float t2 = Vector3.Dot(pointVector, leftVector) / leftSqreMagn;

            t1 = Mathf.Clamp(t1, 0, 1);
            t2 = Mathf.Clamp(t2, 0, 1);

            Vector3 clampedVector = bottomLeftPoint + bottomVector * t1 + leftVector * t2;

            return clampedVector;
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
    public static Vector3 GetPositionRelativeToPlayer(this Transform player, Vector3 vector)
    {
        Vector3 result = Vector3.zero;
        Transform playerTrans = player.transform;

        result = playerTrans.right * vector.x + playerTrans.up * vector.y + playerTrans.forward * vector.z;
        result = playerTrans.position + result;

        return result;
    }
    public static Vector3 GetOppositeOffset(this Transform player, Vector3 curPosition, float offset)
    {
        Vector3 result = Vector3.zero;
        Transform playerTrans = player.transform;

        if (playerTrans.position.x == 0)
            result += curPosition.x > 0 ? new Vector3(-offset, 0, 0) : new Vector3(offset, 0, 0);
        else
            result += curPosition.z > 0 ? new Vector3(0, 0, -offset) : new Vector3(0, 0, offset);

        return result;
    }

    public static void GizmoPointer(this Transform transform, Vector3 position)
    {
        Debug.DrawLine(position, position + Vector3.right, Color.red);
        Debug.DrawLine(position, position + Vector3.up, Color.green);
        Debug.DrawLine(position, position + Vector3.forward, Color.blue);
    }
    public static void GizmoPointer(this Transform transform, Vector3 position, float duration)
    {
        Debug.DrawLine(position, position + Vector3.right, Color.red, duration);
        Debug.DrawLine(position, position + Vector3.up, Color.green, duration);
        Debug.DrawLine(position, position + Vector3.forward, Color.blue, duration);
    }
}
