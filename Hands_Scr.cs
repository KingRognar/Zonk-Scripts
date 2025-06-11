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

    //TODO: сделать дефолтный метод для проигрывания анимаций


    private void Start()
    {
        leftHandStartPos = leftHandTarget.position;
        rightHandStartPos = rightHandTarget.position;
    }

    public void PlayHandRestAnimation(bool isRightHand = true)
    {
        Animator animator = isRightHand ? rightHandAnimator : leftHandAnimator;

        animator.SetBool("Grab", false);
        animator.SetBool("ChangeGrab", false);
        animator.SetBool("Cover", false);
    }
    public void PlayHandGrabAnimation(bool isRightHand = true)
    {
        Animator animator = isRightHand ? rightHandAnimator : leftHandAnimator;

        animator.SetBool("Grab", true);
    }
    public void PlayHandGrab2Animation(bool isRightHand = true)
    {
        Animator animator = isRightHand ? rightHandAnimator : leftHandAnimator;

        animator.SetBool("ChangeGrab", true);
    }
    public void PlayHandCoverAnimation(bool isRightHand = true)
    {
        Animator animator = isRightHand ? rightHandAnimator : leftHandAnimator;

        animator.SetBool("Cover", true);
    }

}
