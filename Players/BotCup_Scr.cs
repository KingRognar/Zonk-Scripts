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


    private Vector3 dropPos;
    private Quaternion endRot;

    public void Initialization()
    {
        plane = new Plane(-player.transform.forward, transform.position);
        dropPos = transform.position;
        endRot = transform.rotation;
    }

    public Sequence MoveCup()
    {
        //TODO: переделать на умное!

        Sequence movementSeq = DOTween.Sequence(this);
        int shakesNum = Random.Range(4, 7);
        Vector3[] line = GetMovementLine();

        Debug.Log(line);
        for (int i = 0; i < shakesNum; i++)
        {
            Vector3 nextPoint = line[i % 2] + GetRandomDiviation(3f);
            //Debug.Log(nextPoint);
            movementSeq.Append(transform.DOMove(nextPoint, 0.2f).SetEase(Ease.InOutCirc));
            //movementSeq.Append(transform.DOMove(GetPosOnPlane(), 0.25f).SetEase(Ease.InOutCubic));
        }

        return movementSeq;
    }
    private Vector3[] GetMovementLine()
    {
        Vector3[] line = new Vector3[2];

        line[0] = GetPosOnPlane(-14, 14, 9, 17);
        int tries = 5;
        do
        {
            line[1] = GetPosOnPlane(-14, 14, 9, 17);
            if ((line[0] - line[1]).magnitude > 10)
                break;
        }
        while (tries-- > 0);


        if (tries != 0)
            return line;
        else
        {
            line[0] = new Vector3(-10, 10, 0);
            line[1] = new Vector3(10, 15, 0);
            return line;
        }
    }
    private Vector3 GetRandomDiviation(float inSqr)
    {
        Vector3 divi = new Vector3(Random.Range(-inSqr, inSqr), Random.Range(-inSqr, inSqr), 0);
        return divi;
    }


    public Sequence OverturnCup()
    {
        sequence = DOTween.Sequence();

        dropPos = new Vector3(transform.position.x, 8.5f, transform.position.z);
        if (player.startAnimWithRightHand)
            endRot = Quaternion.AngleAxis(180, player.transform.forward);
        else
            endRot = Quaternion.AngleAxis(-180, player.transform.forward);

        sequence.AppendCallback(() =>
        {
            player.HandOverturnCupSequence(player.startAnimWithRightHand);
            player.HandResetSequence(!player.startAnimWithRightHand);

            player.diceDropPos = dropPos;
            player.diceDropPos.y = 3;
        });
        sequence.Append(transform.DOMove(dropPos, 0.4f).SetEase(Ease.InCirc));
        sequence.Insert(0, transform.DORotateQuaternion(endRot, 0.4f).SetEase(Ease.InCirc));
        sequence.AppendCallback(() => {
            state = CupState.overturned;
            SoundManager_Scr.instance.PlaySingleSound(cupOverturnSfx, 1f, transform.position);
        });

        return sequence;
    }
    public Sequence ResetCup()
    {
        sequence = DOTween.Sequence();

        Vector3 playerPos = player.transform.position;
        Vector3 newPos = player.transform.position + player.transform.GetOppositeOffset(transform.position, 10f);

        sequence.AppendCallback(() => { player.DropDicesFromCup();});
            sequence.Append(transform.DOMove(newPos, 0.5f));
        sequence.Insert(0, transform.DORotate(new Vector3(0, 0, 0), 0.5f));
        sequence.AppendCallback(() => { state = CupState.empty; isRotated = false; });

        return sequence;
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

    public Vector3 GetPosOnPlane(float xMin, float xMax, float yMin, float yMax)
    {
        Vector3 randomPoint = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), 0);
        return plane.ClosestPointOnPlane(randomPoint);
    }
}
