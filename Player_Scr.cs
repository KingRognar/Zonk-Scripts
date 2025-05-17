using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

public class Player_Scr : MonoBehaviour
{
    //References to other objects
    [SerializeField] private Cup_Scr cup;
    [SerializeField] private TMP_Text UI_Score;
    [SerializeField] private TMP_Text UI_TurnScore;
    [SerializeField] private Button UI_EndTurnBtn;
    [SerializeField] private List<Dice_Scr> diceSet = new();

    //Dice selection
    private List<Dice_Scr> diceSelected = new(), diceToRoll = new();
    public int[] diceValues = new int[6] { 0, 0, 0, 0, 0, 0 };
    public List<int[]> diceCombos = new List<int[]>();

    private int comboCount = 0;
    private bool firstRoll = true;

    //Score related
    private int score = 0;
    private int maxScore = 4000;
    private int turnScore = 0;
    private int tempScore = 0;
    public bool combosExist = false;
    public bool rerollAvailable = true;
    private bool all6 = false;

    //Poisson Disc Sampling variables
    private float radius = 2.83f;
    private Vector2 regionSize = Vector2.one * 8f;
    private int rejectionSamples = 30;
    //private float displayRadius = 1.4f;


    //TODO: прибратьс€
    //TODO: конец хода, когда нет возможных комбо
    //TODO: нельз€ выбирать дайсы до первого броска
    //TODO: писать ли скор, когда выбраны лишние дайсы?

    private void Start()
    {
        cup.player = this;
        for (int i = 0; i < diceSet.Count; i++)
        {
            diceSet[i].player = this;
            diceSet[i].id = i;
        }
        UI_EndTurnBtn.onClick.AddListener(EndTurnBtn);
        UpdateScore();
        UpdateTurnScore();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //SetSpecialValues();
        }

    }
    private void SetSpecialValues()
    {
        diceSet[0].transform.up = -Vector3.forward;
        diceSet[1].transform.up = -Vector3.up;
        diceSet[2].transform.up = Vector3.right;
        diceSet[3].transform.up = -Vector3.right;
        diceSet[4].transform.up = Vector3.up;
        diceSet[5].transform.up = Vector3.up;
        CalculateSelectedDices();
    }


    #region Dice Movement
    public void MoveDicesToCup()
    {
        turnScore += tempScore;
        all6 = CheckAll6();
        if (firstRoll || all6)
            { diceToRoll = diceSet;  firstRoll = false; }
        else
            diceToRoll = GetDiceSelected(false);
        cup.state = Cup_Scr.CupState.filling;
        int i = 0;
        foreach (Dice_Scr dice in diceToRoll)
        {
            Sequence sequence = DOTween.Sequence(this);
            sequence.AppendInterval(i * 0.2f);
            sequence.Append(dice.transform.DOJump(cup.transform.position + new Vector3(0, 9, 0), 3f, 1, 0.3f).SetEase(Ease.OutCirc));
            sequence.Append(dice.transform.DOMove(cup.transform.position + new Vector3(0, 1.5f, 0), 0.1f).SetEase(Ease.InCirc));
            sequence.AppendCallback(() => { dice.transform.SetParent(cup.transform); });
            if (i == diceToRoll.Count - 1)
                sequence.AppendCallback(() => { cup.state = Cup_Scr.CupState.filled; });
            i++;
        }
        if (!all6)
            MoveCombosToBack(); //TODO: не в конце хода и мб при перебросе
        else
            ResetAllDices();
    }
    public void DropDicesFromCup()
    {
        RollDices();

        List<Vector2> diceOffsets;
        int tries = 10;
        do
        {
            diceOffsets = PoissonDiscSampling_Scr.GeneratePoints(radius, regionSize, rejectionSamples);
            tries--;
        } while (diceOffsets.Count < diceToRoll.Count && tries > 0);

        for (int i = 0; i < diceToRoll.Count; i++)
        {
            Transform diceTrans = diceToRoll[i].transform;
            diceTrans.parent = null;
            Vector3 newPos = new Vector3(cup.transform.position.x, 1, cup.transform.position.z);
            newPos += new Vector3(diceOffsets[i].x - regionSize.x / 2, 0, diceOffsets[i].y - regionSize.y / 2);
            diceTrans.position = newPos;
        }

        CheckCombosInActive();
        if (!combosExist)
            BreakStreakAndEndTurn();
    }
    private void MoveCombosToBack()
    {
        for (int i = 0; i < diceCombos.Count; i++)
        {
            List<Transform> comboTransforms = GetDiceCombo(diceCombos[i]);
            MoveComboToBack(comboTransforms, comboCount++);
        }
        DisableSelectedDices();
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
    #endregion

    #region Score Calcs
    private void CalculateSelectedDices()
    {
        tempScore = 0;
        diceSelected = GetDiceSelected(true);
        diceValues = GetDiceValues(diceSelected);
        diceCombos = new List<int[]>();
        CheckForFlashes(ref diceValues, diceCombos, true);
        CheckForDuplicates(ref diceValues, diceCombos, true);
        CheckCombosInSelected();
        UpdateTurnScore();
    }
    private int[] GetDiceValues(List<Dice_Scr> dices)
    {
        int[] values = new int[6] { 0, 0, 0, 0, 0, 0 };
        for (int i = 0; i < dices.Count; i++)
        {
            values[dices[i].UpdateDiceValue() - 1]++;
        }
        return values;
    }
    private void CheckForFlashes(ref int[] values, List<int[]> combos, bool countScore)
    {
        int value = 0;
        int k = 0;
        bool firstFlash = false;
        for (int i = 0; i < values.Length; i++)
        {
            if ((i >= 1 && i <= 4) && values[i] == 0)
                return;
            if (values[i] >= 1)
                k++;
            if (k + 2 < i)
                return;

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
            tempScore += value;
    }
    private void CheckForDuplicates(ref int[] values, List<int[]> combos, bool countScore)
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
    private void CheckCombosInSelected()
    {
        int dicesNumInCombos = 0;
        foreach (int[] diceCombo in diceCombos)
            foreach (int dice in diceCombo) 
                dicesNumInCombos += dice;

        bool allDicesUsed = dicesNumInCombos == diceSelected.Count ? true : false;
        rerollAvailable = (diceCombos.Count > 0 && allDicesUsed) ? true : false;
    }
    private void CheckCombosInActive()
    {
        List<Dice_Scr> activeDices = GetDiceActive();
        int[] values = GetDiceValues(activeDices);
        List<int[]> availableCombos = new List<int[]>();
        CheckForFlashes(ref values, availableCombos, false);
        CheckForDuplicates(ref values, availableCombos, false);
        combosExist = availableCombos.Count > 0 ? true : false;
    }
    private bool CheckAll6() //TODO: мб надо где-то обнул€ть all6
    {
        if (diceSelected.Count != GetDiceActive().Count)
            return false;
        if (rerollAvailable)
            return true;
        return false;
    }
    #endregion

    #region Dice Selection
    private List<Dice_Scr> GetDiceSelected(bool isSelected)
    {
        List<Dice_Scr> ans = new List<Dice_Scr>();
        foreach (Dice_Scr dice in diceSet)
        {
            if (dice.isLeft == isSelected && dice.isActive == true)
                ans.Add(dice);
        }

        return ans;
    }
    private List<Dice_Scr> GetDiceActive()
    {
        List<Dice_Scr> ans = new List<Dice_Scr>();
        foreach (Dice_Scr dice in diceSet)
        {
            if (dice.isActive)
                ans.Add(dice);
        }

        return ans;
    }
    private List<Transform> GetDiceCombo(int[] diceCombo)
    {
        List<Transform> ans = new List<Transform>();
        List<Dice_Scr> diceSetCopy = new List<Dice_Scr>(diceSelected);

        for (int i = 0; i < diceCombo.Length; i++)
        {
            while (diceCombo[i] > 0)
            {
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
    #endregion

    #region Dice Things
    public void RollDices()
    {
        for (int i = 0; i < diceToRoll.Count; i++)
            RollDice(diceToRoll[i].transform);
        CalculateSelectedDices();
    }
    private void RollDice(Transform diceTrans)
    {
        diceTrans.up = diceTrans.GetRandomDirection();
        diceTrans.RotateAround(diceTrans.position, Vector3.up, Random.Range(0f, 360f));
        //Debug.Log(diceTrans.up, diceTrans.gameObject);
    }
    private void DisableSelectedDices()
    {
        foreach (Dice_Scr dice in diceSelected)
        {
            dice.ChangeSelected();
            dice.isActive = false;
        }
        OnDiceSelectChange();
    }
    private void ResetAllDices()
    {
        foreach (Dice_Scr dice in diceSet)
        {
            dice.ResetDie();
        }
    }
    #endregion

    public void OnDiceSelectChange()
    {
        CalculateSelectedDices();
    }

    #region UI Thingies
    private void UpdateTurnScore()
    {
        if (turnScore + tempScore == 0)
        { UI_TurnScore.text = ""; return; }

        int textSize = 50 + Mathf.Max(0, (turnScore + tempScore - 500) / 200);
        UI_TurnScore.text = "+ " + (turnScore + tempScore).ToString();
        UI_TurnScore.fontSize = textSize;
    }
    private void UpdateScore()
    {
        UI_Score.text = score.ToString() + " / " + maxScore.ToString();
    }
    private void AddTurnScore() //TODO: возможно надо изменить пор€док вызова функций в callback
    {
        score += turnScore + tempScore;
        turnScore = 0;
        tempScore = 0;
        Sequence sequence = DOTween.Sequence();
        Vector3 textStartPos = UI_TurnScore.rectTransform.position;

        sequence.Append(UI_TurnScore.rectTransform.DOMove(textStartPos + new Vector3(0, -30, 0), 0.3f).SetEase(Ease.OutCirc));
        sequence.Append(UI_TurnScore.rectTransform.DOMove(UI_Score.rectTransform.position, 0.3f).SetEase(Ease.InCirc));
        sequence.Insert(0.3f, DOTween.To(() => UI_TurnScore.color, x => UI_TurnScore.color = x, new Color(1, 1, 1, 0), 0.3f));
        sequence.AppendCallback(() => {
            UI_TurnScore.color = Color.white;
            UI_TurnScore.rectTransform.position = textStartPos;
            UpdateScore();
            UpdateTurnScore();
        });
    }
    private void DropScore()
    {
        turnScore = 0;
        tempScore = 0;

        Sequence sequence = DOTween.Sequence();
        Vector3 textStartPos = UI_TurnScore.rectTransform.position;

        sequence.Append(UI_TurnScore.DOColor(Color.red, 0.3f));
        sequence.Append(DOTween.To(() => UI_TurnScore.color, x => UI_TurnScore.color = x, new Color(1, 1, 1, 0), 0.3f));
        sequence.Insert(0.3f, UI_TurnScore.rectTransform.DOMove(textStartPos + new Vector3(0, -50, 0), 0.3f).SetEase(Ease.InQuart));
        sequence.AppendCallback(() => {
            UI_TurnScore.color = Color.white;
            UI_TurnScore.rectTransform.position = textStartPos;
            UpdateTurnScore();
        });
    }
    private void EndTurnBtn() //TODO разделить на несколько методов
    {
        if (!rerollAvailable)
            return;

        ResetValues();
        AddTurnScore();
    }
    private void ResetValues()
    {
        comboCount = 0;

        diceSelected = new();
        diceToRoll = new();
        diceCombos = new();

        firstRoll = true;
        rerollAvailable = true; // TODO: помен€ть когда добавлю StartTurn()?

        ResetAllDices();
    }
    private void BreakStreakAndEndTurn()
    {
        ResetValues();
        DropScore();
    }
    #endregion

    public void SetupPlayer(Cup_Scr newCup, List<Dice_Scr> newDices)
    {
        cup = newCup;
        diceSet = newDices;
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
