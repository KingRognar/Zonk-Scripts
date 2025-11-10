using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class Cup_Scr : NetworkBehaviour
{
    [HideInInspector] public BasePlayer_Scr player;

    private Plane plane;
    //Tween movementTween;
    Sequence sequence;
    Vector3 newPos;

    //[HideInInspector] public bool isMovable = true;
    //[HideInInspector] public bool dicesAreIn = false;
    [HideInInspector] public CupState state = CupState.empty;
    private bool isRotated = false;

    private Boundaries boundaries = new();

    private void OnMouseDown()
    {
        if (!IsOwner) return;
        if (!player.isMyTurn) return;

        if (state == CupState.empty && player.rerollAvailable)
            FillCup();
        if (state == CupState.overturned)
            ResetCup();
    }
    private void OnMouseDrag()
    {
        if (!IsOwner) return;

        if (state == CupState.filled)
            MoveCup();
    }
    private void OnMouseUp()
    {
        if (!IsOwner) return;

        if (state == CupState.filled)
            OverturnCup();
    }

    public void Initialization()
    {
        //plane = new Plane(-player.transform.forward, transform.position); //TODO: нужно делать в зависимости от позиции игрока // готово?
        Debug.Log("tp:" + transform.position);
        plane = new Plane(-player.transform.forward, transform.position);
        Debug.Log("dist:" + plane.distance);
        Debug.Log("norm:" + plane.normal);
    }

    private void FillCup()
    {
        player.MoveDicesToCup();
    }
    private void MoveCup()
    {
        if (sequence != null)
            sequence.Kill();

        if (!isRotated)
            RotateCup();

        newPos = GetPosOnPlane();

        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 16f);

        if (!boundaries.initialized)
            boundaries.SetupBoundaries(plane, player.transform, -14, 14, 0, 17);
        transform.position = boundaries.ClampPointToBoundaries(transform.position);

    }
    private void OverturnCup()
    {
        player.HandOverturnCupSequence(player.startAnimWithRightHand);
        player.HandResetSequence(!player.startAnimWithRightHand);

        Vector3 dropPos = new Vector3(transform.position.x, 8.5f, transform.position.z);

        Quaternion endRot;
        if (player.startAnimWithRightHand)
            endRot = Quaternion.AngleAxis(180, player.transform.forward);
        else
            endRot = Quaternion.AngleAxis(-180, player.transform.forward);

        player.diceDropPos = dropPos;
        player.diceDropPos.y = 3;

        sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(dropPos, 0.4f).SetEase(Ease.InBack));
        sequence.Insert(0, transform.DORotateQuaternion(endRot, 0.4f).SetEase(Ease.InBack));
        sequence.AppendCallback(() => { state = CupState.overturned; });
        //transform.position = Vector3.Lerp(transform.position, initialPos, Time.deltaTime * 2f);
        //transform.position = initialPos;
    }
    private void ResetCup()
    {
        player.DropDicesFromCup();

        sequence = DOTween.Sequence();

        Vector3 playerPos = player.transform.position;
        Vector3 newPos = player.transform.position + player.GetOppositeOffset(transform.position, 10f);

        sequence.Append(transform.DOMove(newPos, 0.5f));
        sequence.Insert(0, transform.DORotate(new Vector3(0, 0, 0), 0.5f));
        sequence.AppendCallback(() => { state = CupState.empty; isRotated = false; });
    }
    private void RotateCup()
    {
        //Vector3 endRot = player.startAnimWithRightHand ? new(0, 0, 90) : new(0, 0, -90);
        Quaternion endRot;
        if (player.startAnimWithRightHand)
            endRot = Quaternion.AngleAxis(90, player.transform.forward);
        else
            endRot = Quaternion.AngleAxis(-90, player.transform.forward);

        transform.DORotateQuaternion(endRot, 0.2f);
        player.HandChangeGrabSequence(player.startAnimWithRightHand);
        player.HandChangeCoverSequence(!player.startAnimWithRightHand);
        isRotated = true;
    }

    public Vector3 GetPosOnPlane()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float dist;
        bool isHit = plane.Raycast(ray, out dist);
        return ray.GetPoint(dist);
    }

    public enum CupState
    {
        empty,
        filling,
        filled,
        overturned,
    }
    private class Boundaries
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
}
