using UnityEngine;

public class Dice_Scr : MonoBehaviour
{
    private Outline outline;
    public Player_Scr player;

    public int id = -1;
    public int value = 0;

    public bool isLeft = false;



    void Start()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }

    private void OnMouseDown()
    {
        outline.enabled = !outline.enabled;
        isLeft = !isLeft;
        player.OnDiceSelectChange();
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
