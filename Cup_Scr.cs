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
        plane = new Plane(-transform.forward, transform.position); //TODO: нужно делать в зависимости от позиции игрока
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
        player.HandOverturnCupSequence();
        sequence = DOTween.Sequence();
        Vector3 landPos = new Vector3(transform.position.x, 8.5f, transform.position.z);
        sequence.Append(transform.DOMove(landPos, 0.4f).SetEase(Ease.InBack));
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
        Vector3 newPos = playerPos;
        if (playerPos.x == 0)
            newPos += transform.position.x > 0 ? new Vector3(-10, 0, 0) : new Vector3(10, 0, 0);
        else
            newPos += transform.position.z > 0 ? new Vector3(0, 0, -10) : new Vector3(0, 0, 10);

        sequence.Append(transform.DOMove(newPos, 0.5f));
        sequence.Insert(0, transform.DORotate(new Vector3(0, 0, 0), 0.5f));
        sequence.AppendCallback(() => { state = CupState.empty; isRotated = false; });
    }
    private void RotateCup()
    {
        transform.DORotate(new Vector3(0, 0, 90), 0.2f);
        player.HandChangeGrabSequence();
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
