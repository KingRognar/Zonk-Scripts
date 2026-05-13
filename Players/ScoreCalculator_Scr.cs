using System.Collections.Generic;
using UnityEngine;

public class ScoreCalculator_Scr : MonoBehaviour
{
    int[] diceValues;
    public List<int[]> diceCombos = new List<int[]>();
    public bool rerollAvailable;

    public int CalculateSelectedDices(List<SingleDice_Scr> diceSelected)
    {
        int tempScore = 0;
        //diceSelected = GetDiceSelected(true);
        diceValues = GetDiceValues(diceSelected);
        diceCombos = new List<int[]>();
        tempScore += CheckForFlashes(ref diceValues, diceCombos, true);
        tempScore += CheckForDuplicates(ref diceValues, diceCombos, true);
        rerollAvailable = CheckRerollAvailability(diceSelected);
        ChangeDiceColors(diceSelected);
        //UpdateTurnScore();
        return tempScore;
    }
    private int[] GetDiceValues(List<SingleDice_Scr> dices)
    {
        int[] values = new int[6] { 0, 0, 0, 0, 0, 0 };
        for (int i = 0; i < dices.Count; i++)
        {
            values[dices[i].UpdateDiceValue() - 1]++;
        }
        return values;
    }
    private int CheckForFlashes(ref int[] values, List<int[]> combos, bool countScore)
    {
        int value = 0;
        int k = 0;
        bool firstFlash = false;
        for (int i = 0; i < values.Length; i++)
        {
            if ((i >= 1 && i <= 4) && values[i] == 0)
                return 0;
            if (values[i] >= 1)
                k++;
            if (k + 2 < i)
                return 0;

            if (k == 5 && i == 4)
            {
                value = 500;
                int[] combo = new int[6] { 1, 1, 1, 1, 1, 0 };
                combos.Add(combo);
                values = values.SubtractArray(combo);
                firstFlash = true;
            }
            if (k == 5 && i == 5 && !firstFlash)
            {
                value = 750;
                int[] combo = new int[6] { 0, 1, 1, 1, 1, 1 };
                combos.Add(combo);
                values = values.SubtractArray(combo);
            }
            if (k == 6)
            {
                value = 1500;
                int[] combo = new int[6] { 1, 1, 1, 1, 1, 1 };
                combos.Add(combo);
                values = values.SubtractArray(combo);
            }
        }
        if (countScore)
            return value;
        return 0;
    }
    private int CheckForDuplicates(ref int[] values, List<int[]> combos, bool countScore)
    {
        int value = 0;
        for (int dice = 0; dice < values.Length; dice++)
        {
            int baseValue = GetDiceBaseScore(dice);
            int temp = 0;

            int dicesCount;
            if (dice == 0 || dice == 4)
                dicesCount = 1;
            else
                dicesCount = 3;
            while (dicesCount <= 6)
            {
                if (values[dice] < dicesCount)
                    break;
                if (dicesCount <= 2)
                    temp = dicesCount * baseValue * 10;
                if (dicesCount == 3)
                    temp = baseValue * 100;
                if (dicesCount++ > 3)
                    temp = temp * 2;
            }
            if (temp != 0)
            {
                int[] diceCombo = new int[] { 0, 0, 0, 0, 0, 0 };
                diceCombo[dice] = values[dice];
                values.SubtractArray(diceCombo);
                combos.Add(diceCombo);
            }
            value += temp;
        }
        if (countScore)
            return value;
        return 0;
    }
    private int GetDiceBaseScore(int num)
    {
        switch (num)
        {
            case 0: return 10;
            case 1: return 2;
            case 2: return 3;
            case 3: return 4;
            case 4: return 5;
            case 5: return 6;
            default: return 0;
        }
    }
    private bool CheckRerollAvailability(List<SingleDice_Scr> diceSelected)
    {
        int dicesNumInCombos = 0;
        foreach (int[] diceCombo in diceCombos)
            foreach (int dice in diceCombo)
                dicesNumInCombos += dice;

        bool allDicesUsed = dicesNumInCombos == diceSelected.Count ? true : false;
        return (diceCombos.Count > 0 && allDicesUsed) ? true : false;
    }
    /// <summary>
    /// Returns True if there's at least one combo available in active dices, otherwise False
    /// </summary>
    /// <param name="activeDices">List of active Dices</param>
    /// <returns></returns>
    public bool CheckCombosInActive(List<SingleDice_Scr> activeDices)
    {
        int[] values = GetDiceValues(activeDices);
        List<int[]> availableCombos = new List<int[]>();
        CheckForFlashes(ref values, availableCombos, false);
        CheckForDuplicates(ref values, availableCombos, false);
        return availableCombos.Count > 0 ? true : false;
    }
    public bool CheckAll6(List<SingleDice_Scr> diceSelected, List<SingleDice_Scr> diceActive, bool rerollAvailable) //TODO: мб надо где-то обнулять all6
    {
        if (diceSelected.Count != diceActive.Count)
            return false;
        if (rerollAvailable)
            return true;
        return false;
    }
    private void ChangeDiceColors(List<SingleDice_Scr> diceSelected)
    {
        foreach (SingleDice_Scr die in diceSelected)
            die.ChangeOutlineColorToRed(true);

        List<SingleDice_Scr> changedColor = new List<SingleDice_Scr>();
        foreach (int[] diceCombo in diceCombos)
            for (int i = 0; i < 6; i++)
            {
                int j = diceCombo[i];
                if (j == 0)
                    continue;
                foreach (SingleDice_Scr die in diceSelected)
                    if (j > 0 && !changedColor.Contains(die) && die.value == i + 1)
                    {
                        die.ChangeOutlineColorToRed(false);
                        changedColor.Add(die);
                        j--;
                    }
            }
    }
}
