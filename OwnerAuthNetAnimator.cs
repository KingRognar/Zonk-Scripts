using Unity.Netcode.Components;
using UnityEngine;

public class OwnerAuthNetAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
        //return base.OnIsServerAuthoritative();
    }
}
