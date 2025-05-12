using UnityEngine;

public class Dice_Scr : MonoBehaviour
{
    private Outline outline;

    public int id = -1;

    bool isLeft = false;



    void Start()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }

    private void OnMouseDown()
    {
        outline.enabled = !outline.enabled;
        isLeft = !isLeft;
    }

    public int CheckDiceValue()
    {
        if (transform.up == Vector3.up)
            return 5;
        if (-transform.up == Vector3.up)
            return 2;
        if (transform.right == Vector3.up)
            return 4;
        if (-transform.right == Vector3.up)
            return 3;
        if (transform.forward == Vector3.up)
            return 1;
        if (-transform.forward == Vector3.up)
            return 6;
        return -1;
    }
}
