using System.Globalization;
using UnityEngine;

public class SingleDice_Scr : MonoBehaviour
{
    private Outline outline;
    public SinglePlayer_Scr player;
    public Transform leadTrans;

    public int id = -1;
    public int value = 0;

    public bool isLeft = false, isActive = true;



    void Start()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }
    private void Update()
    {
        //TODO: мб что-то получше есть?
        if (leadTrans != null)
            transform.position = Vector3.Lerp(transform.position, leadTrans.position, Time.deltaTime * 5);
    }

    private void OnMouseDown()
    {
        if (!isActive) return;
        if (player.firstRoll) return;
        if (!player.isMyTurn) return;

        ChangeSelected();
        player.OnDiceSelectChange();
    }

    public void ChangeSelected()
    {
        outline.enabled = !outline.enabled;
        isLeft = !isLeft;
    }
    public void ResetDie()
    {
        isActive = true;
        isLeft = false;
        outline.enabled = false;
    }

    public int UpdateDiceValue()
    {
        if (transform.up == Vector3.up)
        { value = 5; return 5; }
        if (-transform.up == Vector3.up)
        { value = 2; return 2; }
        if (transform.right == Vector3.up)
        { value = 4; return 4; }
        if (-transform.right == Vector3.up)
        { value = 3; return 3; }
        if (transform.forward == Vector3.up)
        { value = 1; return 1; }
        if (-transform.forward == Vector3.up)
        { value = 6; return 6; }
        value = -1; return -1;
    }
}
