using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class Player_Scr : MonoBehaviour
{
    [SerializeField] private List<Dice_Scr> diceSet = new(), diceSelected = new(), diceUnselected = new();
    [SerializeField] private Cup_Scr cup;
    [SerializeField] private TMP_Text UI_Score;
    public int[] diceValues = new int[6] { 0, 0, 0, 0, 0, 0 };
    public List<int[]> diceCombos = new List<int[]>();
    public int tempScore = 0;
    [Space(10)]
    public float radius = 1.5f;
    public Vector2 regionSize = Vector2.one * 4.5f;
    public int rejectionSamples = 30;
    public float displayRadius = .75f;
    List<Vector2> diceOffsets;

    //TODO: прибраться


    private void Start()
    {
        cup.player = this;
        for (int i = 0; i < diceSet.Count; i++)
        {
            diceSet[i].player = this;
            diceSet[i].id = i;
        }
    }


    public void MoveDicesToCup()
    {
        diceUnselected = GetDiceSelected(false);
        cup.state = Cup_Scr.CupState.filling;
        int i = 0;
        foreach (Dice_Scr dice in diceUnselected)
        {
            Sequence sequence = DOTween.Sequence(this);
            sequence.AppendInterval(i * 0.2f);
            sequence.Append(dice.transform.DOJump(cup.transform.position + new Vector3(0, 9, 0), 3f, 1, 0.3f));
            sequence.Append(dice.transform.DOMove(cup.transform.position + new Vector3(0, 1.5f, 0), 0.15f));
            sequence.AppendCallback(() => { dice.transform.SetParent(cup.transform); });
            if (i == diceUnselected.Count - 1)
                sequence.AppendCallback(() => { cup.state = Cup_Scr.CupState.filled; });
            i++;
        }
        MoveCombosToBack();
    }
    public void DropDicesFromCup()
    {
        RollDices();
        int tries = 10;
        do
        {
            diceOffsets = PoissonDiscSampling_Scr.GeneratePoints(radius, regionSize, rejectionSamples);
            tries--;
        } while (diceOffsets.Count < diceUnselected.Count && tries > 0);

        for (int i = 0; i < diceUnselected.Count; i++)
        {
            Transform diceTrans = diceUnselected[i].transform;
            diceTrans.parent = null;
            Vector3 newPos = new Vector3(cup.transform.position.x, 1, cup.transform.position.z);
            newPos += new Vector3(diceOffsets[i].x - regionSize.x / 2, 0, diceOffsets[i].y - regionSize.y / 2);
            diceTrans.position = newPos;
        }
    }
    public void RollDices()
    {
        for (int i = 0; i < diceUnselected.Count; i++)
            RollDice(diceUnselected[i].transform);
        CalculateSelectedDices();

        //diceOffsets = PoissonDiscSampling_Scr.GeneratePoints(radius, regionSize, rejectionSamples);
    }
    private void RollDice(Transform diceTrans)
    {
        diceTrans.up = diceTrans.GetRandomDirection();
        diceTrans.RotateAround(diceTrans.position, Vector3.up, Random.Range(0f, 360f));
        //Debug.Log(diceTrans.up, diceTrans.gameObject);
    }

    private void CalculateSelectedDices()
    {
        tempScore = 0;
        diceSelected = GetDiceSelected(true);
        GetDiceValues();
        diceCombos = new List<int[]>();
        CheckForFlashes();
        CheckForDuplicates();
        UI_Score.text = tempScore.ToString();
    }
    private List<Dice_Scr> GetDiceSelected(bool isSelected)
    {
        List<Dice_Scr> ans = new List<Dice_Scr>();
        foreach (Dice_Scr dice in diceSet)
        {
            if (dice.isLeft == isSelected)
                ans.Add(dice);
        }

        return ans;
    }
    private void GetDiceValues()
    {
        diceValues = new int[6] { 0, 0, 0, 0, 0, 0 };
        for (int i = 0; i < diceSelected.Count; i++)
        {
            diceValues[diceSelected[i].UpdateDiceValue() - 1]++;
        }
    }
    private void CheckForFlashes()
    {
        int value = 0;
        int k = 0;
        bool firstFlash = false;
        for (int i = 0; i < diceValues.Length; i++)
        {
            if ((i >= 1 && i <= 4) && diceValues[i] == 0)
                return;
            if (diceValues[i] >= 1)
                k++;
            if (k + 2 < i)
                return;

            if (k == 5 && i == 4)
            { 
                value = 500; 
                diceValues.SubtractArray(new int[] { 1, 1, 1, 1, 1 }); 
                diceCombos.Add(new int[6] { 1, 1, 1, 1, 1, 0 }); 
                firstFlash = true; 
            }
            if (k == 5 && i == 5 && !firstFlash)
            { 
                value = 750; 
                diceValues.SubtractArray(new int[] { 0, 1, 1, 1, 1, 1 });
                diceCombos.Add(new int[6] { 0, 1, 1, 1, 1, 1 });
            }
            if (k == 6)
            { 
                value = 1500; 
                diceValues = new int[] { 0, 0, 0, 0, 0, 0 };
                diceCombos.Clear(); diceCombos.Add(new int[6] { 1, 1, 1, 1, 1, 1 });
            }
        }
        //Debug.Log("falsh - " + value);
        tempScore += value;
    }
    private void CheckForDuplicates()
    {
        int value = 0;
        for (int dice = 0; dice < diceValues.Length; dice++)
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
                if (diceValues[dice] < dicesCount)
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
                diceCombo[dice] = diceValues[dice];
                diceCombos.Add(diceCombo);
            }
            value += temp;
        }

        tempScore += value;
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

    private void MoveCombosToBack()
    {
        for (int i = 0; i < diceCombos.Count; i++)
        {
            List<Transform> comboTransforms = GetDiceCombo(diceCombos[i]);
            MoveComboToBack(comboTransforms, i);
        }
    }
    private void DisableDices()
    {
        //TODO: отключать подсветку дайсов, убирать возможность их использовать до следующего круга/раунда
    }
    private List<Transform> GetDiceCombo(int[] diceCombo)
    {
        List<Transform> ans = new List<Transform>();
        List<Dice_Scr> diceSetCopy = new List<Dice_Scr>(diceSelected);

        for (int i = 0; i < diceCombo.Length; i++)
        {
            while (diceCombo[i] > 0)
            {
                /*foreach (Dice_Scr dice in diceSetCopy)
                {
                    if (dice.value == i + 1)
                    { ans.Add(dice.transform); break; }
                }*/
                for (int j = diceSetCopy.Count - 1; j >= 0; j--)
                {
                    if (diceSetCopy[j].value == i + 1)
                    { ans.Add(diceSetCopy[j].transform); diceSetCopy.Remove(diceSetCopy[j]); break; }
                }

                diceCombo[i]--;
            }
        }

        return ans;
    }
    private void MoveComboToBack(List<Transform> diceTransforms, int comboNum)
    {
        float diceDist = 3f;
        float comboStartX = ((diceTransforms.Count - 1) * diceDist) / -2;
        float comboDist = -20f + (comboNum * diceDist * 1.5f);
        for (int i = 0; i < diceTransforms.Count; i++)
        {
            Vector3 newPos = new Vector3(comboStartX, 1, comboDist);
            comboStartX += diceDist;
            diceTransforms[i].DOMove(newPos, 0.4f);
        }
    }

    public void OnDiceSelectChange()
    {
        CalculateSelectedDices();
    }

    /*void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(regionSize / 2, regionSize);
        if (diceOffsets != null)
        {
            foreach (Vector2 point in diceOffsets)
            {
                Gizmos.DrawSphere(point, displayRadius);
            }
        }
    }*/
}
