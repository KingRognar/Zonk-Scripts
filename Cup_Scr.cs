using DG.Tweening;
using UnityEngine;

public class Cup_Scr : MonoBehaviour
{
    [HideInInspector] public Player_Scr player;

    private Plane plane;
    //Tween movementTween;
    Sequence sequence;
    Vector3 newPos;

    //[HideInInspector] public bool isMovable = true;
    //[HideInInspector] public bool dicesAreIn = false;
    [HideInInspector] public CupState state = CupState.empty;


    private void Start()
    {
        plane = new Plane(-transform.forward, transform.position);
    }

    private void OnMouseDown()
    {
        if (state == CupState.empty)
            FillCup();
        if (state == CupState.overturned)
            ResetCup();
    }
    private void OnMouseDrag()
    {
        if (state == CupState.filled)
            MoveCup();
    }
    private void OnMouseUp()
    {
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

        Vector3 newPos;
        if (transform.position.x > 0)
            newPos = new Vector3(-10, 0, -30);
        else
            newPos = new Vector3(10, 0, -30);
        sequence.Append(transform.DOMove(newPos, 0.5f));
        sequence.Insert(0, transform.DORotate(new Vector3(0, 0, 0), 0.5f));
        sequence.AppendCallback(() => { state = CupState.empty; });
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
