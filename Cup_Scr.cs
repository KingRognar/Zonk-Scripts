using DG.Tweening;
using UnityEngine;

public class Cup_Scr : MonoBehaviour
{
    [HideInInspector] public Player_Scr player;

    Vector3 initialPos;
    private Plane plane;
    Tween movementTween;
    Vector3 newPos;

    [HideInInspector] public bool isMovable = true;
    [HideInInspector] public bool dicesAreIn = false;


    private void Start()
    {
        plane = new Plane(-transform.forward, transform.position);
        initialPos = transform.position;
    }

    private void OnMouseDown()
    {
        if (!dicesAreIn)
            player.MoveDicesToCup();
    }
    private void OnMouseDrag()
    {
        if (!isMovable)
            return;
        if (movementTween != null)
            movementTween.Kill();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float dist = 0;
        bool isHit = plane.Raycast(ray, out dist);
        //Vector3 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, dist));
        newPos = ray.GetPoint(dist);

        //transform.position = newPos;
        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 16f);
        SetPositionInBoundaries();
    }
    private void OnMouseUp()
    {
        movementTween = transform.DOMove(initialPos, 0.5f);
        //transform.position = Vector3.Lerp(transform.position, initialPos, Time.deltaTime * 2f);
        //transform.position = initialPos;
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
}
