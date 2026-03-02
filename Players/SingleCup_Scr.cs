using DG.Tweening;
using UnityEngine;
using static Cup_Scr;
using static Extensions_Scr;

public class SingleCup_Scr : MonoBehaviour
{
    [HideInInspector] public SinglePlayer_Scr player;

    private Plane plane;
    //Tween movementTween;
    Sequence sequence;
    Vector3 newPos;

    //[HideInInspector] public bool isMovable = true;
    //[HideInInspector] public bool dicesAreIn = false;
    [HideInInspector] public CupState state = CupState.empty;
    private bool isRotated = false;

    private Boundaries boundaries = new();

    private float prevSpeed = 0, speed = 0;
    [SerializeField] private float speedThreshold = 3f;
    [SerializeField] private float shakeDelay = 0.1f;
    private float lastShake = -1f;


    public int dicesIn = -1;


    public void Initialization()
    {
        //plane = new Plane(-player.transform.forward, transform.position); //TODO: нужно делать в зависимости от позиции игрока // готово?
        Debug.Log("tp:" + transform.position);
        plane = new Plane(-player.transform.forward, transform.position);
        Debug.Log("dist:" + plane.distance);
        Debug.Log("norm:" + plane.normal);
    }
}
