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

    public void AnimatorResetAllBool(bool isRightHand = true)
    {
        Animator animator = isRightHand ? rightHandAnimator : leftHandAnimator;

        animator.SetBool("Grab", false);
        animator.SetBool("ChangeGrab", false);
        animator.SetBool("Cover", false);
        animator.SetBool("Dice", false);
        animator.SetBool("Free", false);
    }
    public void AnimatorSetBool(string boolName, bool isRightHand = true)
    {
        Animator animator = isRightHand ? rightHandAnimator : leftHandAnimator;

        animator.SetBool(boolName, true);
    }

}
