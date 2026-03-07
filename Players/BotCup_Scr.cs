using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using static Extensions_Scr;

public class BotCup_Scr : MonoBehaviour
{
    [HideInInspector] public BotPlayer_Scr player;

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

    [System.Serializable]
    public struct AudioClips
    {
        [SerializeField] public List<AudioClip> clips;
    }
    [SerializeField] private List<AudioClips> shakingSfx;
    [SerializeField] AudioClip cupOverturnSfx;

    public int dicesIn = -1;


    public void Initialization()
    {
        plane = new Plane(-player.transform.forward, transform.position);
    }

    public Sequence MoveCup()
    {
        //TODO: переделать на умное!

        Sequence movementSeq = DOTween.Sequence(this);
        int shakesNum = Random.Range(3, 5);
        for (int i = 0; i < shakesNum; i++)
        {
            movementSeq.Append(transform.DOMove(GetPosOnPlane(), 0.4f).SetEase(Ease.InOutCubic));
        }

        return movementSeq;
    }
    public void RotateCup()
    {
        Debug.Log("начианаю rotate cup");

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
        Vector3 randomPoint = new Vector3(Random.Range(-30, 30), Random.Range(0, 30), 0);
        return plane.ClosestPointOnPlane(randomPoint);
    }
}
