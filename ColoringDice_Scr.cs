using System.Xml.Serialization;
using UnityEngine;

public class ColoringDice_Scr : MonoBehaviour
{
    [SerializeField] private int diceId;
    [SerializeField] private Vector3 prevRotationVector = Vector3.zero, nextRotationVector = Vector3.zero, curRotationVector = Vector3.zero;
    [SerializeField] private float nextRotVectorChange = 0f, rotVectorChangeDelay = 15f;

    void Update()
    {
        ChangeRotationOnTimer();
        DiceComplexRotation();
    }

    private void ChangeRotationOnTimer()
    {
        if (nextRotVectorChange > Time.time)
            return;

        nextRotVectorChange += rotVectorChangeDelay;

        prevRotationVector = nextRotationVector;
        nextRotationVector = new Vector3(Random.Range(-15, 15), Random.Range(-15, 15), Random.Range(-15, 15));
    }
    private void DiceComplexRotation()
    {
        curRotationVector = Vector3.Lerp(prevRotationVector, nextRotationVector, (rotVectorChangeDelay - nextRotVectorChange + Time.time) / rotVectorChangeDelay);
        transform.Rotate(curRotationVector * Time.deltaTime);
    }

    private void OnMouseDown()
    {
        DiceUICallback();
    }
    private void DiceUICallback()
    {
        if (transform.parent == null) { Debug.Log("Coloring Dice missing a parent (Dice Menu)"); return; }

        GetComponentInParent<UI_DiceColoring_Scr>().ChangeDiceColor(diceId);
    }
}
