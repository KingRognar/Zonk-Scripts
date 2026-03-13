using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using static Extensions_Scr;

public class BotPlayer_Scr : MonoBehaviour
{
    #region Varibles
    //References to other objects
    [SerializeField] public BotCup_Scr cup;
    [SerializeField] private Transform canvasTrans;
    //private UiRefs uiRefs = new();
    [SerializeField] public List<BotDice_Scr> diceSet = new();
    [SerializeField] private Hands_Scr hands;
    [HideInInspector] public SPGameManager_Scr spGM;

    //Dice selection
    private List<BotDice_Scr> diceSelected = new(), diceToRoll = new();
    public int[] diceValues = new int[6] { 0, 0, 0, 0, 0, 0 };
    public List<int[]> diceCombos = new List<int[]>();

    private int comboCount = 0;
    [HideInInspector] public bool firstRoll = true;

    //TODO: нада придумать как получше загружать необходимые маериалы, сейчас я тупа гружу все через едиотр
    [SerializeField] private List<DiceMaterialSetSO_Scr> diceMaterialSets;

    //Score related
    private int score = 0;
    private int maxScore = 4000;
    private int turnScore = 0;
    private int tempScore = 0;
    public bool combosExist = false;
    public bool rerollAvailable = true;
    private bool all6 = false;

    public bool isMyTurn = false;

    [HideInInspector] public bool startAnimWithRightHand = true;
    public Vector3 diceDropPos = Vector3.zero;

    //Poisson Disc Sampling variables
    private float radius = 2.83f;
    private Vector2 regionSize = Vector2.one * 8f;
    private int rejectionSamples = 30;
    //private float displayRadius = 1.4f;

    Dictionary<ulong, NetworkObject> netObjects;

    bool isShowingGest = false;
    #endregion

    #region Init
    public void Initialize()
    {
        SetupUI();
        SetInitialPositions();
        LoadDiceColoringSchemes();
        cup.Initialization();
    }
    protected void SetupUI()
    {
        //TODO:
    }
    protected void SetInitialPositions()
    {
        cup.transform.position = transform.GetPositionRelativeToPlayer(new Vector3(10, 0, 0));
        cup.Initialization();
        diceDropPos = transform.GetPositionRelativeToPlayer(new Vector3(-10, 0, 0)) + new Vector3(0, 3, 0);
        Vector2 regionSize = Vector2.one * 8f;
        List<Vector2> diceOffsets;
        int tries = 10;

        do
        {
            diceOffsets = PoissonDiscSampling_Scr.GeneratePoints(2.83f, regionSize, 30);
        } while (diceOffsets.Count < 6 && tries-- > 0);

        for (int i = 0; i < 6; i++)
        {
            Vector3 dicePos = transform.GetPositionRelativeToPlayer(new Vector3(-10, 0, 0));
            dicePos += new Vector3(diceOffsets[i].x - regionSize.x / 2, 0, diceOffsets[i].y - regionSize.y / 2);

            diceSet[i].transform.position = dicePos + new Vector3(0, 1, 0);
            RollDice(diceSet[i].transform);
        }
    }
    protected void LoadDiceColoringSchemes()
    {
        //TODO:
    }
    #endregion

    #region Bot Logic
    private void MakeTurn()
    {
        Debug.Log(gameObject.name + " делает ход");

        Sequence sequence = DOTween.Sequence(this);

        sequence.Append(MoveDicesToCup());
        sequence.AppendCallback(() => { Debug.Log("завершил первый сиквенс"); cup.RotateCup(); });
        sequence.Append(cup.MoveCup());
        sequence.AppendCallback(() => { OverturnCup(); });
    }
    private void OverturnCup()
    {
        Sequence sequence = DOTween.Sequence(this);

        sequence.Append(cup.OverturnCup());
        sequence.AppendInterval(0.5f);
        sequence.Append(cup.ResetCup());

        sequence.AppendCallback(() => { EndTurn(); Debug.Log("завершил второй сиквенс"); });
    }

    #endregion

    #region Dice Movement
    public Sequence MoveDicesToCup()
    {
        turnScore += tempScore;
        all6 = CheckAll6();
        if (firstRoll || all6)
        { diceToRoll = diceSet; firstRoll = false; }
        else
            diceToRoll = GetDiceSelected(false);
        cup.state = CupState.filling;
        cup.dicesIn = diceToRoll.Count - 1;

        /*int i = 0;
        foreach (Dice_Scr dice in diceToRoll)
        {
            bool last = i++ == diceToRoll.Count - 1 ? true : false;
            DiceCupSequence(dice, i, last);
        }*/

        CheckWhichHandToAnimate();
        HandGrabCupSequence(startAnimWithRightHand);
        Sequence sequence = DOTween.Sequence(this);
        sequence.Append(HandGrabDicesSequence(!startAnimWithRightHand));
        sequence.Append(HandCoverCupSequence(!startAnimWithRightHand));

        


        //HandCoverCupSequence(!startAnimWithRightHand);

        if (!all6)
            MoveCombosToBack(); //TODO: не в конце хода и мб при перебросе
        else
            ResetAllDices();

        return sequence;
    }
    private void CheckWhichHandToAnimate()
    {
        Vector3 cupLoaclPos = cup.transform.localPosition;
        startAnimWithRightHand = cupLoaclPos.x > 0;
        if (startAnimWithRightHand)
            Debug.Log("правая рука " + cupLoaclPos);
        else
            Debug.Log("левая рука " + cupLoaclPos);
    }
    private void DiceCupSequence(BotDice_Scr dice, int i, bool addLastCallback)
    {
        Vector3 endpoint = cup.transform.position + new Vector3(0, 1.5f, 0);
        Sequence sequence = DOTween.Sequence(this);
        sequence.AppendInterval(i * 0.2f);
        sequence.Append(dice.transform.DOJump(cup.transform.position + new Vector3(0, 9, 0), 3f, 1, 0.3f).SetEase(Ease.OutCirc));
        sequence.AppendCallback(() => { dice.transform.parent = cup.transform; });
        sequence.Append(dice.transform.DOMove(endpoint, 0.2f).SetEase(Ease.InCirc));
        if (addLastCallback)
            sequence.AppendCallback(() => { cup.state = CupState.filled; });

    }
    public void DropDicesFromCup()
    {
        RollDices();

        HandResetSequence(startAnimWithRightHand);

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
        Vector3 shift = transform.position.x == 0 ? new Vector3(startShift, 0, 0) : new Vector3(0, 0, startShift);

        for (int i = 0; i < diceTransforms.Count; i++)
        {

            Vector3 newPos = shift + distPos;
            shift += transform.position.x == 0 ? new Vector3(diceDist, 0, 0) : new Vector3(0, 0, diceDist);
            diceTransforms[i].DOMove(newPos, 0.4f);
        }
    }
    #endregion

    #region Hands Animations
    //TODO: надо как-то объединить эти методы с обычными, чтоб было лаконично
    //TODO: сделать имена по умному
    //TODO: вычислять endPos endRot и targetOffset в зависимости от позиции игрока
    //TODO: прикрутить slerp или curve
    //TODO: сделать анимашку возвращения стакана на место
    private void HandGrabCupSequence(bool isRightHand)
    {
        Transform handTarget = isRightHand ? hands.rightHandTarget : hands.leftHandTarget;
        MultiPositionConstraint handMPC = isRightHand ? hands.rightHandMPC : hands.leftHandMPC;
        RigBuilder rigBuilder = isRightHand ? hands.rightHandRigBuilder : hands.leftHandRigBuilder;

        //TODO: двигаться по кривой
        Vector3 mpcOffset = isRightHand ? new Vector3(5.2f, 2, 0) : new Vector3(-5.2f, 2, 0);
        Vector3 targetOffset = GetHandOffsetPosition(isRightHand, new Vector3(5.2f, 2, 0));
        Vector3 endPos = cup.transform.position + targetOffset;

        Quaternion endRot = GetHandInitRotation(isRightHand);
        endRot = endRot * Quaternion.AngleAxis(-30, Vector3.forward) * Quaternion.AngleAxis(-95, Vector3.up);



        Sequence sequence = DOTween.Sequence(this);
        sequence.Append(handTarget.DOMove(endPos, 0.5f));
        sequence.Insert(0, handTarget.DOLocalRotateQuaternion(endRot, 0.5f));
        sequence.InsertCallback(0.35f, () => {
            hands.AnimatorSetBool("Grab", isRightHand);
        });

        sequence.AppendCallback(() => {
            WeightedTransformArray weightedTransforms = new WeightedTransformArray();
            weightedTransforms.Add(new WeightedTransform(cup.transform, 1));
            handMPC.data.sourceObjects = weightedTransforms;
            handMPC.data.offset = mpcOffset;
            rigBuilder.Build();
            handMPC.weight = 1;
        });
    }
    public void HandChangeGrabSequence(bool isRightHand)
    {
        Transform handTarget = isRightHand ? hands.rightHandTarget : hands.leftHandTarget;
        MultiPositionConstraint handMPC = isRightHand ? hands.rightHandMPC : hands.leftHandMPC;

        //Vector3 targetOffset = GetHandOffsetPosition(isRightHand, new Vector3(1, -2, 0));
        Vector3 mpcOffset = isRightHand ? new Vector3(1, -2, 0) : new Vector3(-1, -2, 0);

        Quaternion endRot = GetHandInitRotation(isRightHand);
        endRot = endRot * GetHandLocalRotation(90, -90, 0);

        hands.AnimatorSetBool("ChangeGrab", isRightHand);
        Sequence sequence = DOTween.Sequence(this);
        sequence.Append(DOTween.To(() => handMPC.data.offset, x => handMPC.data.offset = x, mpcOffset, 0.2f));
        sequence.Insert(0, handTarget.DOLocalRotateQuaternion(endRot, 0.5f));
    }
    private Sequence HandCoverCupSequence(bool isRightHand)
    {
        Transform handTarget = isRightHand ? hands.rightHandTarget : hands.leftHandTarget;
        MultiPositionConstraint handMPC = isRightHand ? hands.rightHandMPC : hands.leftHandMPC;
        RigBuilder rigBuilder = isRightHand ? hands.rightHandRigBuilder : hands.leftHandRigBuilder;


        Vector3 mpcOffset = isRightHand ? new Vector3(3.4f, 9.4f, 0) : new Vector3(-3.4f, 9.4f, 0);
        Vector3 targetOffset = GetHandOffsetPosition(isRightHand, new Vector3(3.4f, 9.4f, 0));
        Vector3 endPos = cup.transform.position + targetOffset;

        Quaternion endRot = GetHandInitRotation(isRightHand);
        endRot = endRot * GetHandLocalRotation(0, 0, -90);

        Sequence sequence = DOTween.Sequence(this);
        sequence.Append(handTarget.DOJump(endPos, 2f, 1, 0.5f));
        sequence.Insert(0, handTarget.DOLocalRotateQuaternion(endRot, 0.5f));
        sequence.InsertCallback(0.35f, () => {
            //hands.PlayHandCoverAnimation(isRightHand);
            hands.AnimatorSetBool("Cover", isRightHand);
        });

        sequence.AppendCallback(() => {
            WeightedTransformArray weightedTransforms = new WeightedTransformArray();
            weightedTransforms.Add(new WeightedTransform(cup.transform, 1));
            handMPC.data.sourceObjects = weightedTransforms;
            handMPC.data.offset = mpcOffset;
            rigBuilder.Build();
            handMPC.weight = 1;

            foreach (BotDice_Scr dice in diceToRoll)
            {
                dice.leadTrans = null;
                dice.transform.parent = cup.transform;
                dice.transform.DOLocalMove(new Vector3(0, 1.5f, 0), 0.1f);
            }
            cup.state = CupState.filled;
        });

        return sequence;
    }
    public void HandChangeCoverSequence(bool isRightHand)
    {
        Transform handTarget = isRightHand ? hands.rightHandTarget : hands.leftHandTarget;
        MultiPositionConstraint handMPC = isRightHand ? hands.rightHandMPC : hands.leftHandMPC;

        //Vector3 targetOffset = GetHandOffsetPosition(isRightHand, new Vector3(9.4f, -3.4f, 0));
        Vector3 mpcOffset = isRightHand ? new Vector3(9.4f, -3.4f, 0) : new Vector3(-9.4f, -3.4f, 0);

        Quaternion endRot = GetHandInitRotation(isRightHand);
        endRot = endRot * GetHandLocalRotation(90, -90, 0);

        Sequence sequence = DOTween.Sequence(this);
        sequence.Append(DOTween.To(() => handMPC.data.offset, x => handMPC.data.offset = x, mpcOffset, 0.2f));
        sequence.Insert(0, handTarget.DOLocalRotateQuaternion(endRot, 0.5f));
    }
    public void HandOverturnCupSequence(bool isRightHand)
    {
        Transform handTarget = isRightHand ? hands.rightHandTarget : hands.leftHandTarget;
        MultiPositionConstraint handMPC = isRightHand ? hands.rightHandMPC : hands.leftHandMPC;

        Vector3 mpcOffset = isRightHand ? new Vector3(2, 1, 0) : new(-2, 1, 0);

        Quaternion endRot = GetHandInitRotation(isRightHand);
        endRot = endRot * GetHandLocalRotation(0, 0, -90);

        Sequence sequence = DOTween.Sequence(this);
        sequence.Append(DOTween.To(() => handMPC.data.offset, x => handMPC.data.offset = x, mpcOffset, 0.2f).SetEase(Ease.InBack));
        sequence.Insert(0, handTarget.DOLocalRotateQuaternion(endRot, 0.5f).SetEase(Ease.InBack));
    }
    private Sequence HandGrabDicesSequence(bool isRightHand)
    {
        Transform handTarget = isRightHand ? hands.rightHandTarget : hands.leftHandTarget;

        //TODO: позиция в зависимости от положения игрока
        //TODO: двигаться по кривой

        Vector3 endPos = diceDropPos;

        Quaternion endRot = GetHandInitRotation(isRightHand);
        endRot *= GetHandLocalRotation(0, 0, -75);

        hands.AnimatorSetBool("Free", isRightHand);
        Sequence sequence = DOTween.Sequence(this);

        sequence.Append(handTarget.DOJump(endPos, 2f, 1, 0.5f));
        sequence.Insert(0, handTarget.DOLocalRotateQuaternion(endRot, 0.5f));
        sequence.InsertCallback(0.4f, () => {
            hands.AnimatorSetBool("Dice", isRightHand);

            foreach (BotDice_Scr dice in diceToRoll)
            {
                dice.leadTrans = handTarget;
            }
        });
        sequence.AppendInterval(0.1f);

        return sequence;
    }
    public void HandResetSequence(bool isRightHand)
    {
        Transform handTarget = isRightHand ? hands.rightHandTarget : hands.leftHandTarget;
        MultiPositionConstraint handMPC = isRightHand ? hands.rightHandMPC : hands.leftHandMPC;
        RigBuilder rigBuilder = isRightHand ? hands.rightHandRigBuilder : hands.leftHandRigBuilder;

        hands.AnimatorResetAllBool(isRightHand);

        Vector3 endPos = isRightHand ? hands.rightHandStartPos : hands.leftHandStartPos;

        Quaternion endRot = GetHandInitRotation(isRightHand);
        endRot = endRot * GetHandLocalRotation(0, 0, -30);

        Sequence sequence = DOTween.Sequence(this);
        sequence.Append(handTarget.DOMove(endPos, 0.5f));
        sequence.Insert(0, handTarget.DOLocalRotateQuaternion(endRot, 0.5f));
        sequence.AppendCallback(() => {
            handMPC.data.sourceObjects.Clear();
            rigBuilder.Build();
            handMPC.weight = 0;
        });
    }
    private Vector3 GetHandOffsetPosition(bool isRightHand, Vector3 offset)
    {
        Vector3 ret;

        if (isRightHand)
            ret = transform.right * offset.x + transform.up * offset.y + transform.forward * offset.z;
        else
            ret = transform.right * -offset.x + transform.up * offset.y + transform.forward * offset.z;

        return ret;
    }
    private Vector3 GetHandMPCPosition(bool isRightHand, Vector3 offset)
    {
        Vector3 ret = new();

        return ret;
    }
    private Quaternion GetHandInitRotation(bool isRightHand)
    {
        return isRightHand ? Quaternion.LookRotation(Vector3.up, Vector3.forward) : Quaternion.LookRotation(-Vector3.up, -Vector3.forward);
    }
    private Quaternion GetHandLocalRotation(Vector3 euler)
    {
        return Quaternion.AngleAxis(euler.x, transform.right) * Quaternion.AngleAxis(euler.y, transform.up) * Quaternion.AngleAxis(euler.z, transform.forward);
    }
    private Quaternion GetHandLocalRotation(float x, float y, float z)
    {
        return Quaternion.AngleAxis(x, Vector3.right) * Quaternion.AngleAxis(y, Vector3.up) * Quaternion.AngleAxis(z, Vector3.forward);
    }
    private void HandShowFck(bool isRightHand)
    {
        Transform handTarget = isRightHand ? hands.rightHandTarget : hands.leftHandTarget;

        hands.AnimatorSetBool("Fck", isRightHand);
        isShowingGest = true;

        Quaternion endRot = GetHandInitRotation(isRightHand);
        endRot = endRot * GetHandLocalRotation(90, -180, 45);

        handTarget.DOLocalRotateQuaternion(endRot, 0.5f);
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
    }
    private int[] GetDiceValues(List<BotDice_Scr> dices)
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
        List<BotDice_Scr> activeDices = GetDiceActive();
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
    private List<BotDice_Scr> GetDiceSelected(bool isSelected)
    {
        List<BotDice_Scr> ans = new List<BotDice_Scr>();
        foreach (BotDice_Scr dice in diceSet)
        {
            if (dice.isLeft == isSelected && dice.isActive == true)
                ans.Add(dice);
        }

        return ans;
    }
    private List<BotDice_Scr> GetDiceActive()
    {
        List<BotDice_Scr> ans = new List<BotDice_Scr>();
        foreach (BotDice_Scr dice in diceSet)
        {
            if (dice.isActive)
                ans.Add(dice);
        }

        return ans;
    }
    private List<Transform> GetDiceCombo(int[] diceCombo)
    {
        List<Transform> ans = new List<Transform>();
        List<BotDice_Scr> diceSetCopy = new List<BotDice_Scr>(diceSelected);

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
    protected void RollDice(Transform diceTrans)
    {
        diceTrans.up = diceTrans.GetRandomDirection();
        diceTrans.RotateAround(diceTrans.position, Vector3.up, Random.Range(0f, 360f));
    }
    private void DisableSelectedDices()
    {
        foreach (BotDice_Scr dice in diceSelected)
        {
            dice.ChangeSelected();
            dice.isActive = false;
        }
        OnDiceSelectChange();
    }
    private void ResetAllDices()
    {
        foreach (BotDice_Scr dice in diceSet)
        {
            dice.ResetDie();
        }
    }
    public void OnDiceSelectChange()
    {
        CalculateSelectedDices();
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
    #endregion

    #region TBS
    public void StartTurn()
    {
        isMyTurn = true;
        MakeTurn();
        //uiRefs.yourTurnSign.gameObject.SetActive(true);
        //TODO: мб сделать надпись чей ход?
    }
    private void BreakStreakAndEndTurn()
    {
        EndTurn();
        //ResetValues();
        //DropScore();
    }
    private void EndTurn()
    {
        isMyTurn = false;
        //uiRefs.yourTurnSign.gameObject.SetActive(false);
        spGM.TurnPass();
    }
    #endregion
}
