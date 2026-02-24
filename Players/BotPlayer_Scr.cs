using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BotPlayer_Scr : MonoBehaviour
{
    #region Varibles
    //References to other objects
    [SerializeField] public BotCup_Scr cup;
    [SerializeField] private Transform canvasTrans;
    //private UiRefs uiRefs = new();
    [SerializeField] public List<BotDice_Scr> diceSet = new();
    [SerializeField] private Hands_Scr hands;

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
    protected void Initialize()
    {
        SetupUI();
        SetupCupAndDices();
        SetupHands();
        SetInitialPositions();
        LoadDiceColoringSchemes();
    }
    protected void SetupUI()
    {
        //TODO:
    }
    protected void SetupCupAndDices()
    {
        cup = transform.GetChild(1).GetComponent<BotCup_Scr>();
        cup.player = this;


        diceSet = new();
        for (int i = 0; i < 6; i++)
        {
            Transform diceTrans = transform.GetChild(2 + i);

            diceSet.Add(diceTrans.GetComponent<BotDice_Scr>());
            diceSet[i].player = this;
            diceSet[i].id = i;
        }

        LoadDiceColoringSchemes();
    }
    protected void SetupHands()
    {
        hands = transform.GetChild(0).GetComponent<Hands_Scr>();
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

    protected void RollDice(Transform diceTrans)
    {
        diceTrans.up = diceTrans.GetRandomDirection();
        diceTrans.RotateAround(diceTrans.position, Vector3.up, Random.Range(0f, 360f));
    }
}
