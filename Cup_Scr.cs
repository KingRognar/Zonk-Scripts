using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class Cup_Scr : NetworkBehaviour
{
    [HideInInspector] public Player_Scr player;

    private Plane plane;
    //Tween movementTween;
    Sequence sequence;
    Vector3 newPos;

    //[HideInInspector] public bool isMovable = true;
    //[HideInInspector] public bool dicesAreIn = false;
    [HideInInspector] public CupState state = CupState.empty;
    private bool isRotated = false;


    private void Start()
    {
        plane = new Plane(-player.transform.forward, transform.position); //TODO: нужно делать в зависимости от позиции игрока // готово?
    }

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

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float dist = 0;
        bool isHit = plane.Raycast(ray, out dist);
        //Vector3 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, dist));
        newPos = ray.GetPoint(dist);

        //transform.position = newPos;
        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 16f);
        SetPositionInBoundaries();
    }
    private void OverturnCup()
    {
        player.HandOverturnCupSequence(player.startAnimWithRightHand);
        player.HandResetSequence(!player.startAnimWithRightHand);
        sequence = DOTween.Sequence();
        Vector3 dropPos = new Vector3(transform.position.x, 8.5f, transform.position.z);
        player.diceDropPos = dropPos;
        player.diceDropPos.y = 3;
        sequence.Append(transform.DOMove(dropPos, 0.4f).SetEase(Ease.InBack));
        sequence.Insert(0, transform.DORotate(new Vector3(0, 0, 180f), 0.4f).SetEase(Ease.InBack));
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
        Vector3 endRot = player.startAnimWithRightHand ? new(0, 0, 90) : new(0, 0, -90);

        transform.DORotate(endRot, 0.2f);
        player.HandChangeGrabSequence(player.startAnimWithRightHand);
        player.HandChangeCoverSequence(!player.startAnimWithRightHand);
        isRotated = true;
    }

    private void SetPositionInBoundaries(float minX = -14f, float maxX = 14f, float minY = 0f, float maxY = 17f) //TODO: изменять границы в зависимости от размера экрана
    {
        Vector3 posInBound = transform.position;

        if (posInBound.y < minY)
            posInBound.y = minY;
        if (posInBound.y > maxY)
            posInBound.y = maxY;
        if (posInBound.x < minX)
            posInBound.x = minX;
        if (posInBound.x > maxX)
            posInBound.x = maxX;

        transform.position = posInBound;
    }

    public enum CupState
    {
        empty,
        filling,
        filled,
        overturned,
    }
}
