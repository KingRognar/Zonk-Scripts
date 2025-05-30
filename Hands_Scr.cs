using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Hands_Scr : MonoBehaviour
{
    public Transform leftHandTarget, rightHandTarget;
    public Transform leftHandHint, rightHandHint;
    public TwoBoneIKConstraint leftHandTBIK, rightHandTBIK;
    public MultiParentConstraint leftHandMPC, rightHandMPC;
}
