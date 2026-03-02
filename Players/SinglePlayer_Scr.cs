using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static Extensions_Scr;

public class SinglePlayer_Scr : MonoBehaviour
{
    #region Varibles
    //References to other objects
    [SerializeField] public SingleCup_Scr cup;
    [SerializeField] private Transform canvasTrans;
    private UiRefs uiRefs = new();
    [SerializeField] public List<SingleDice_Scr> diceSet = new();
    [SerializeField] private SingleHands_Scr hands;

    //Dice selection
    private List<Dice_Scr> diceSelected = new(), diceToRoll = new();
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

    bool isShowingGest = false;
    #endregion

    void Start()
    {
        //Initialize();
    }

    #region Init
    public void Initialize()
    {
        SetupCamera();
        SetupUI();
        SetupCupAndDices();
        SetupHands();
        SetInitialPositions();

        //UpdateScore();
        //UpdateTurnScore();

        //if (OwnerClientId == 0) isMyTurn = true;
    }
    protected void SetupUI()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas)
            canvasTrans = canvas.transform;
        else
            return;

        Transform gameUiTrans = canvasTrans.GetChild(0);
        gameUiTrans.gameObject.SetActive(true);
        uiRefs.totalScore = gameUiTrans.GetChild(1).GetComponent<TMP_Text>();
        uiRefs.turnScore = gameUiTrans.GetChild(2).GetComponent<TMP_Text>();
        uiRefs.endTurn = gameUiTrans.GetChild(0).GetComponent<Button>();
        uiRefs.playersScores = GameObject.FindAnyObjectByType<Scores_Scr>();
        uiRefs.yourTurnSign = gameUiTrans.GetChild(4).gameObject;
        //TODO:
        /*if (OwnerClientId != 0)
            uiRefs.playersScores.EnableAnotherScoreRpc(OwnerClientId);
        else
            uiRefs.yourTurnSign.SetActive(true);

        uiRefs.endTurn.onClick.AddListener(EndTurnBtn);*/
    }
    private void EnableOthersScores()
    {
        //TODO: for bots
    }
    private void SetupCamera()
    {
        Vector3 camPos = transform.GetPositionRelativeToPlayer(new Vector3(0, 0, -20)) + new Vector3(0, 20, 0);

        Transform newCamTrans = Instantiate(Camera.main, camPos, Quaternion.identity).transform;
        newCamTrans.rotation *= Quaternion.LookRotation(-transform.position, Vector3.up);
        newCamTrans.rotation *= Quaternion.Euler(20, 0, 0);

        Camera.main.gameObject.SetActive(false);
        Camera.main.tag = "Untagged";
        newCamTrans.tag = "MainCamera";
    }
    protected void SetupCupAndDices()
    {
        //TODO: запихуить cup при спавне
        //cup = netCup.GetComponent<Cup_Scr>();
        //cup.transform.parent = transform;
        //cup.Initialization();
        cup = transform.GetChild(1).GetComponent<SingleCup_Scr>();
        cup.player = this;

        //TODO: запихуить dices при спавне
        //diceSet = new();
        for (int i = 0; i < 6; i++)
        {
            diceSet[i] = transform.GetChild(2 + i).GetComponent<SingleDice_Scr>();
            diceSet[i].player = this;
            diceSet[i].id = i;
        }

        LoadDiceColoringSchemes();
    }
    protected void SetupHands()
    {
        hands = transform.GetChild(0).GetComponent<SingleHands_Scr>();
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
        //TODO: нада придумать как получше загружать необходимые маериалы, сейчас я тупа гружу все через едиотр

        string saveFilePath = Application.persistentDataPath + "/diceColoring.json";
        int[] dicesColoringSchemeIds = new int[6] { 0, 0, 0, 0, 0, 0 };

        if (File.Exists(saveFilePath))
        {
            string jsonString = File.ReadAllText(saveFilePath);
            dicesColoringSchemeIds = JsonUtility.FromJson<IntArrayWrapper>(jsonString).intArray;
        }
    }
    #endregion


    protected void RollDice(Transform diceTrans)
    {
        diceTrans.up = diceTrans.GetRandomDirection();
        diceTrans.RotateAround(diceTrans.position, Vector3.up, Random.Range(0f, 360f));
    }
}
