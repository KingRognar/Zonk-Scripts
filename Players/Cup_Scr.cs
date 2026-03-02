using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using static Extensions_Scr;

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

    private Boundaries boundaries = new();

    private float prevSpeed = 0, speed = 0;
    [SerializeField] private float speedThreshold = 3f;
    [SerializeField] private float shakeDelay = 0.1f;
    private float lastShake = -1f;


    public int dicesIn = -1;
    [System.Serializable]
    public struct AudioClips
    {
        [SerializeField] public List<AudioClip> clips;
    }
    [SerializeField] private List<AudioClips> shakingSfx;
    [SerializeField] AudioClip cupOverturnSfx;

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
        {
            MoveCup();
            ShakingSound();
        }     
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
        Vector3 oldPos = transform.position;

        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 16f);

        if (!boundaries.initialized)
            boundaries.SetupBoundaries(plane, player.transform, -14, 14, 0, 17);
        transform.position = boundaries.ClampPointToBoundaries(transform.position);

        prevSpeed = speed;
        speed = (transform.position - oldPos).magnitude;

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
        sequence.Append(transform.DOMove(dropPos, 0.4f).SetEase(Ease.InCirc));
        sequence.Insert(0, transform.DORotateQuaternion(endRot, 0.4f).SetEase(Ease.InCirc));
        sequence.AppendCallback(() => { 
            state = CupState.overturned;
            SoundManager_Scr.instance.PlaySingleSound(cupOverturnSfx, 1f, transform.position);
        });
        //transform.position = Vector3.Lerp(transform.position, initialPos, Time.deltaTime * 2f);
        //transform.position = initialPos;
    }
    private void ResetCup()
    {
        player.DropDicesFromCup();

        sequence = DOTween.Sequence();

        Vector3 playerPos = player.transform.position;
        Vector3 newPos = player.transform.position + player.transform.GetOppositeOffset(transform.position, 10f);

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

    private void ShakingSound()
    {
        float difference = Mathf.Abs(prevSpeed - speed);
        float volume = 1f * Mathf.InverseLerp(10, -5, difference);

        if (difference > speedThreshold && Time.time > lastShake + shakeDelay)
        {
            if (dicesIn < 0) return;
            List<AudioClip> clips = shakingSfx[dicesIn].clips;

            SoundManager_Scr.instance.PlaySingleSound(clips[Random.Range(0, clips.Count)], volume, transform.position);
            lastShake = Time.time;
        }


        Debug.Log("cur speed - " + speed + " difference - " + (prevSpeed - speed).ToString() + " volume - " + volume);
    }

    public Vector3 GetPosOnPlane()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float dist;
        bool isHit = plane.Raycast(ray, out dist);
        return ray.GetPoint(dist);
    }
}
