using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine;

public class Player_Scr : MonoBehaviour
{
    [SerializeField] private List<Dice_Scr> diceSet = new();
    [SerializeField] private Cup_Scr cup;
    public int[] diceValues = new int[6] { 0, 0, 0, 0, 0, 0 };
    public int tempScore = 0;

    private void Start()
    {
        cup.player = this;
        for (int i = 0; i < diceSet.Count; i++)
        {
            diceSet[i].id = i;
        }
        CalculateSelectedDices();
    }

    public void MoveDicesToCup()
    {
        cup.isMovable = false;
        int i = 0;
        foreach (Dice_Scr dice in diceSet)
        {
            Sequence sequence = DOTween.Sequence(this);
            sequence.AppendInterval(i * 0.2f);
            sequence.Append(dice.transform.DOJump(cup.transform.position + new Vector3(0, 9, 0), 3f, 1, 0.3f));
            sequence.Append(dice.transform.DOMove(cup.transform.position + new Vector3(0, 1.5f, 0), 0.15f));
            sequence.AppendCallback(() => { dice.transform.SetParent(cup.transform); });
            if (i == diceSet.Count - 1)
                sequence.AppendCallback(() => { cup.isMovable = true; cup.dicesAreIn = true; });
            i++;
        }
    }

    private void CalculateSelectedDices()
    {
        tempScore = 0;
        GetDiceValues();
        CheckForFlashes();
        CheckForDuplicates();
    }
    private void GetDiceValues()
    {
        diceValues = new int[6] { 0, 0, 0, 0, 0, 0 };
        for (int i = 0; i < diceSet.Count; i++)
        {
            diceValues[diceSet[i].CheckDiceValue() - 1]++;
        }
    }
    private void CheckForFlashes()
    {
        int value = 0;
        int k = 0;
        for (int i = 0; i < diceValues.Length; i++)
        {
            if (diceValues[i] >= 1)
                k++;
            if (k == 5)
            {
                if (i == 4)
                { value = 500; break; }
                else
                { value = 750; break; }
            }
            if (k == 6)
                value = 1500;
        }
        tempScore += value;
    }
    private void CheckForDuplicates()
    {
        //TODO: 
    }
}
