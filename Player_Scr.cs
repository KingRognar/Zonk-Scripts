using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

public class Player_Scr : NetworkBehaviour
{
    //References to other objects
    [SerializeField] public Cup_Scr cup;
    [SerializeField] private Transform canvasTrans;
    private UiRefs uiRefs = new();
    [SerializeField] public List<Dice_Scr> diceSet = new();
    [SerializeField] private Hands_Scr hands;

    //Dice selection
    private List<Dice_Scr> diceSelected = new(), diceToRoll = new();
    public int[] diceValues = new int[6] { 0, 0, 0, 0, 0, 0 };
    public List<int[]> diceCombos = new List<int[]>();

    private int comboCount = 0;
    [HideInInspector] public bool firstRoll = true;

    //Score related
    private int score = 0;
    private int maxScore = 4000;
    private int turnScore = 0;
    private int tempScore = 0;
    public bool combosExist = false;
    public bool rerollAvailable = true;
    private bool all6 = false;


    public bool isMyTurn = false;

    //Poisson Disc Sampling variables
    private float radius = 2.83f;
    private Vector2 regionSize = Vector2.one * 8f;
    private int rejectionSamples = 30;
    //private float displayRadius = 1.4f;

    //TODO: прибраться
    //TODO: писать ли скор, когда выбраны лишние дайсы?
    //TODO: как-то обозначить первый бросок
    //TODO: не забыть с onClickEvent'ами разобраться - когда чашка стоит за кнопкой может получится так, что она сработает первее

    //MIND: можно использовать rpc.notMe чтобы всех пингануть, чтобы они послали rpc тому кто пинганул что надо!!!


    private void Start()
    {
        Initialize();
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


    #region Init
    public void Initialize()
    {
        SetupCamera();
        SetupUI();
        SetupCupAndDices();
        SetupHands();
        SetInitialPositions();

        UpdateScore();
        UpdateTurnScore();

        if (OwnerClientId == 0) isMyTurn = true; 
    }
    private void SetupUI()
    {
        //Transform canvasTrans = GameObject.Find("Canvas").transform;
        //canvasTrans.GetChild(1).gameObject.SetActive(true);

        if (!IsOwner) return;

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas)
            canvasTrans = canvas.transform;
        else
            return;

        canvasTrans.GetChild(1).gameObject.SetActive(true);
        uiRefs.totalScore = canvasTrans.GetChild(1).GetChild(1).GetComponent<TMP_Text>();
        uiRefs.turnScore = canvasTrans.GetChild(1).GetChild(2).GetComponent<TMP_Text>();
        uiRefs.endTurn = canvasTrans.GetChild(1).GetChild(0).GetComponent<Button>();
        uiRefs.playersScores = GameObject.FindAnyObjectByType<Scores_Scr>();
        uiRefs.yourTurnSign = canvasTrans.GetChild(1).GetChild(4).gameObject;

        EnableOthersScores();



        if (OwnerClientId != 0)
            uiRefs.playersScores.EnableAnotherScoreRpc(OwnerClientId);
        else
            uiRefs.yourTurnSign.SetActive(true);

        uiRefs.endTurn.onClick.AddListener(EndTurnBtn);
    }
    private void EnableOthersScores()
    {
        if (OwnerClientId == 0 /*&& NetworkManager.Singleton.ConnectedClientsIds.Count <= 1*/)
            return;

        ulong[] ids = NetworkManager.Singleton.ConnectedClientsIds.ToArray();
        for (int i = 0; i < ids.Length; i++)
        {
            if (ids[i] == OwnerClientId) continue;
            uiRefs.playersScores.EnableAnotherScore(ids[i]);
        }
    }
    private void SetupCamera()
    {
        if (!IsOwner)
            return;

        Vector3 camPos = transform.position.normalized * 50;
        camPos += new Vector3(0, 20, 0);
        Transform newCamTrans = Instantiate(Camera.main, camPos, Quaternion.identity).transform;
        newCamTrans.rotation *= Quaternion.LookRotation(-transform.position, Vector3.up);
        newCamTrans.rotation *= Quaternion.Euler(20, 0, 0);

        Camera.main.gameObject.SetActive(false);
        Camera.main.tag = "Untagged";
        newCamTrans.tag = "MainCamera";
    }
    private void SetupCupAndDices()
    {
        cup = transform.GetChild(0).GetComponent<Cup_Scr>();
        cup.player = this;

        diceSet = new();
        int i = 0;
        while (i < 6 && i+2 < transform.childCount)
        {
            diceSet.Add(transform.GetChild(i + 2).GetComponent<Dice_Scr>());

            diceSet[i].player = this;
            diceSet[i].id = i++;
        }
        int k = 0;
        while (i < 6 && k < cup.transform.childCount)
        {
            diceSet.Add(cup.transform.GetChild(k++).GetComponent<Dice_Scr>());

            diceSet[i].player = this;
            diceSet[i].id = i++;
        }
    }
    private void SetupHands()
    {
        /*if (!transform.GetChild(1).TryGetComponent<Hands_Scr>(out hands))
            Debug.Log("не получилось взять реф рук", this);*/
        hands = transform.GetChild(1).GetComponent<Hands_Scr>();

        hands.rightHandMPC.data.sourceObjects.Add(new WeightedTransform(cup.transform, 0f));
        hands.leftHandMPC.data.sourceObjects.Add(new WeightedTransform(cup.transform, 0f));
    }
    private void SetInitialPositions()
    {
        /*if (!IsOwner)
            return;*/

        //CUP
        Vector3 vectorA = -transform.position.normalized;
        Vector3 vectorB = Vector3.up;
        Vector3 posOffset = Vector3.Cross(vectorA, vectorB) * 10f;

        cup.transform.position = transform.position + posOffset;

        //DICES
        posOffset = -posOffset;
        Vector2 regionSize = Vector2.one * 8f;
        List<Vector2> diceOffsets;
        int tries = 10;

        do
        {
            diceOffsets = PoissonDiscSampling_Scr.GeneratePoints(2.83f, regionSize, 30);
        } while (diceOffsets.Count < 6 && tries-- > 0);

        for (int i = 0; i < 6; i++)
        {
            Vector3 dicePos = posOffset + new Vector3(0, 1, 0);
            dicePos += new Vector3(diceOffsets[i].x - regionSize.x / 2, 0, diceOffsets[i].y - regionSize.y / 2);

            diceSet[i].transform.position = transform.position + dicePos;
            RollDice(diceSet[i].transform);
        }
    }
    #endregion

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
            bool last = i++ == diceToRoll.Count - 1 ? true : false;
            DiceCupSequence(dice, i, last);
        }

        HandGrabCupSequence();

        if (!all6)
            MoveCombosToBack(); //TODO: не в конце хода и мб при перебросе
        else
            ResetAllDices();
    }
    private void DiceCupSequence(Dice_Scr dice,int i, bool addLastCallback)
    {
        Vector3 endpoint = cup.transform.position + new Vector3(0, 1.5f, 0);
        Sequence sequence = DOTween.Sequence(this);
        sequence.AppendInterval(i * 0.2f);
        sequence.Append(dice.transform.DOJump(cup.transform.position + new Vector3(0, 9, 0), 3f, 1, 0.3f).SetEase(Ease.OutCirc));
        sequence.AppendCallback(() => { dice.transform.parent = cup.transform; });
        sequence.Append(dice.transform.DOMove(endpoint, 0.2f).SetEase(Ease.InCirc));
        if (addLastCallback)
            sequence.AppendCallback(() => { cup.state = Cup_Scr.CupState.filled; });

    }
    private void HandGrabCupSequence()
    {
        //TODO: позиция в зависимости от положения игрока
        //TODO: двигаться по кривой

        Sequence sequence = DOTween.Sequence(this);
        sequence.Append(hands.rightHandTarget.DOMove(cup.transform.position + new Vector3(6, 2, 0), 0.5f));
        sequence.AppendCallback(() => { hands.rightHandMPC.weight = 1f; });
        
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
            diceTrans.parent = transform;
            Vector3 newPos = new Vector3(cup.transform.position.x, 1, cup.transform.position.z);
            newPos += new Vector3(diceOffsets[i].x - regionSize.x / 2, 0, diceOffsets[i].y - regionSize.y / 2);
            diceTrans.position = newPos;
            diceTrans.GetComponent<NetworkTransform>().Teleport(diceTrans.position, diceTrans.rotation, diceTrans.localScale);
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
        Vector3 distPos = transform.position.normalized * (20f - comboNum * diceDist * 1.5f) + new Vector3(0, 1, 0);
        
        float startShift = ((diceTransforms.Count - 1) * diceDist) / -2;
        Vector3 shift = transform.position.x == 0 ? new Vector3(startShift, 0, 0) : new Vector3 (0, 0, startShift);

        for (int i = 0; i < diceTransforms.Count; i++)
        {

            Vector3 newPos = shift + distPos;
            shift += transform.position.x == 0 ? new Vector3(diceDist, 0, 0) : new Vector3(0, 0, diceDist);
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
    private bool CheckAll6() //TODO: мб надо где-то обнулять all6
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
    public void RollDice(Transform diceTrans)
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
        if (!IsOwner) return;

        if (turnScore + tempScore == 0)
        { uiRefs.turnScore.text = ""; return; }

        int textSize = 50 + Mathf.Max(0, (turnScore + tempScore - 500) / 200);
        uiRefs.turnScore.text = "+ " + (turnScore + tempScore).ToString();
        uiRefs.turnScore.fontSize = textSize;
    }
    private void UpdateScore()
    {
        if (!IsOwner) return;

        Debug.Log("обновил скор " + score);

        uiRefs.playersScores.UpdatePlayerScoreRpc(OwnerClientId, score);
        uiRefs.totalScore.text = score.ToString() + " / " + maxScore.ToString();
    }
    private void AddTurnScore() //TODO: возможно надо изменить порядок вызова функций в callback
    {
        score += turnScore + tempScore;
        turnScore = 0;
        tempScore = 0;
        Sequence sequence = DOTween.Sequence();
        Vector3 textStartPos = uiRefs.turnScore.rectTransform.position;

        sequence.Append(uiRefs.turnScore.rectTransform.DOMove(textStartPos + new Vector3(0, -30, 0), 0.3f).SetEase(Ease.OutCirc));
        sequence.Append(uiRefs.turnScore.rectTransform.DOMove(uiRefs.totalScore.rectTransform.position, 0.3f).SetEase(Ease.InCirc));
        sequence.Insert(0.3f, DOTween.To(() => uiRefs.turnScore.color, x => uiRefs.turnScore.color = x, new Color(1, 1, 1, 0), 0.3f));
        sequence.AppendCallback(() => {
            uiRefs.turnScore.color = Color.white;
            uiRefs.turnScore.rectTransform.position = textStartPos;
            UpdateScore();
            UpdateTurnScore();
        });
    }
    private void DropScore()
    {
        turnScore = 0;
        tempScore = 0;

        Sequence sequence = DOTween.Sequence();
        Vector3 textStartPos = uiRefs.turnScore.rectTransform.position;

        sequence.Append(uiRefs.turnScore.DOColor(Color.red, 0.3f));
        sequence.Append(DOTween.To(() => uiRefs.turnScore.color, x => uiRefs.turnScore.color = x, new Color(1, 1, 1, 0), 0.3f));
        sequence.Insert(0.3f, uiRefs.turnScore.rectTransform.DOMove(textStartPos + new Vector3(0, -50, 0), 0.3f).SetEase(Ease.InQuart));
        sequence.AppendCallback(() => {
            uiRefs.turnScore.color = Color.white;
            uiRefs.turnScore.rectTransform.position = textStartPos;
            UpdateTurnScore();
        });
    }
    private void EndTurnBtn() //TODO разделить на несколько методов
    {
        if (!rerollAvailable)
            return;

        EndTurn();
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
        rerollAvailable = true; // TODO: поменять когда добавлю StartTurn()?

        ResetAllDices();
    }
    private void BreakStreakAndEndTurn()
    {
        EndTurn();
        ResetValues();
        DropScore();
    }
    private struct UiRefs
    {
        public TMP_Text turnScore;
        public TMP_Text totalScore;
        public Button endTurn;
        public Scores_Scr playersScores;
        public GameObject yourTurnSign;
    }
    #endregion

    private void EndTurn()
    {
        isMyTurn = false;
        uiRefs.yourTurnSign.gameObject.SetActive(false);

        GameManager_Scr.instance.PlayerTurnEndRpc();
    }
    [Rpc(SendTo.SpecifiedInParams)]
    public void PlayerTurnStartRpc(RpcParams rpcParams)
    {
        isMyTurn = true;
        uiRefs.yourTurnSign.gameObject.SetActive(true);

        //TODO: добавить надпись что мой ход
    }


}
