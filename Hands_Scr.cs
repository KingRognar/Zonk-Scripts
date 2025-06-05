using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Hands_Scr : MonoBehaviour
{
    public Transform leftHandTarget, rightHandTarget;
    public Transform leftHandHint, rightHandHint;
    public TwoBoneIKConstraint leftHandTBIK, rightHandTBIK;
    public MultiPositionConstraint leftHandMPC, rightHandMPC;
    public Animator leftHandAnimator, rightHandAnimator;
    public RigBuilder leftHandRigBuilder, rightHandRigBuilder;
    [HideInInspector] public Vector3 leftHandStartPos, rightHandStartPos;


    private void Start()
    {
        leftHandStartPos = leftHandTarget.position;
        rightHandStartPos = rightHandTarget.position;
    }

    public void PlayHandRestAnimation(bool isRightHand = true)
    {
        rightHandAnimator.SetBool("Grab", false);
        rightHandAnimator.SetBool("ChangeGrab", false);
    }
    public void PlayHandGrabAnimation(bool isRightHand = true)
    {
        rightHandAnimator.SetBool("Grab", true);
    }
    public void PlayHandGrab2Animation(bool isRightHand = true)
    {
        rightHandAnimator.SetBool("ChangeGrab", true);
    }

}
