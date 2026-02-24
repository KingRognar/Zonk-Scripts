using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static Extensions_Scr;
using Sequence = DG.Tweening.Sequence;

public abstract class BasePlayer_Scr : NetworkBehaviour
{
    #region Variables
    //References to other objects
    [SerializeField] protected List<Dice_Scr> diceSet = new();
    [SerializeField] protected Hands_Scr hands;

    //Dice selection
    protected List<Dice_Scr> diceSelected = new(), diceToRoll = new();
    protected int[] diceValues = new int[6] { 0, 0, 0, 0, 0, 0 };
    protected List<int[]> diceCombos = new List<int[]>();

    protected int comboCount = 0;
    [HideInInspector] protected bool firstRoll = true;

    //TODO: нада придумать как получше загружать необходимые маериалы, сейчас я тупа гружу все через едиотр
    [SerializeField] protected List<DiceMaterialSetSO_Scr> diceMaterialSets;

    //Score related
    protected int score = 0;
    protected int maxScore = 4000;
    protected int turnScore = 0;
    protected int tempScore = 0;
    protected bool combosExist = false;
    public bool rerollAvailable = true;
    protected bool all6 = false;


    public bool isMyTurn = false;

    [HideInInspector] protected bool startAnimWithRightHand = true;
    protected Vector3 diceDropPos = Vector3.zero;

    //Poisson Disc Sampling variables
    protected float radius = 2.83f;
    protected Vector2 regionSize = Vector2.one * 8f;
    protected int rejectionSamples = 30;
    #endregion

    #region Init
    protected abstract void Initialize();
    protected abstract void SetupUI();
    protected abstract void SetupCupAndDices();
    protected abstract void SetupHands();
    protected void SetInitialPositions()
    {
        //cup.transform.position = this.GetPositionRelativeToPlayer(new Vector3(10, 0, 0));
        //cup.Initialization();
        //diceDropPos = this.GetPositionRelativeToPlayer(new Vector3(-10, 0, 0)) + new Vector3(0, 3, 0);
        Vector2 regionSize = Vector2.one * 8f;
        List<Vector2> diceOffsets;
        int tries = 10;

        do
        {
            diceOffsets = PoissonDiscSampling_Scr.GeneratePoints(2.83f, regionSize, 30);
        } while (diceOffsets.Count < 6 && tries-- > 0);

        for (int i = 0; i < 6; i++)
        {
            //Vector3 dicePos = this.GetPositionRelativeToPlayer(new Vector3(-10, 0, 0));
            //dicePos += new Vector3(diceOffsets[i].x - regionSize.x / 2, 0, diceOffsets[i].y - regionSize.y / 2);

            //diceSet[i].transform.position = dicePos + new Vector3(0, 1, 0);
            //RollDice(diceSet[i].transform);
        }
    }

    protected abstract void LoadDiceColoringSchemes();
    #endregion

}
