using System.Collections.Generic;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_Manager_Scr : MonoBehaviour
{
    public static UI_Manager_Scr instance;

    bool yourTurnIsActive = false;

    private UIDocument doc;


    private Label score, tempScore, maxScore;
    private VisualElement yourTurn;
    public Button endTurnBtn;
    private Button menuBtn;
    private List<PlayerCard> playerCards = new List<PlayerCard>();

    [Space(10)]
    [SerializeField] private float YourTurnScaleMult = 1.5f;
    [SerializeField] private float firstPhaseTime = 0.2f;
    [SerializeField] private float firstTransTime = 0.05f;
    [SerializeField] private float lingerTime = 0.1f;
    [SerializeField] private float secondPhaseTime = 0.6f;
    [SerializeField] private float translateDist = 200f;

    [Space(10)]
    [SerializeField] private float tempScoreMoveUpTime = 0.4f;
    [SerializeField] private float tempScoreTranslateDist = -90;
    [SerializeField] private float tempScoreFadeDelay = 0.2f;
    [SerializeField] private float tempScoreRandRotationSpread = 60f;

    private Sequence seq_tempScoreShake;

    private void Awake()
    {
        if (instance == null) instance = this; else Destroy(this);

        doc = GetComponent<UIDocument>();

        endTurnBtn = doc.rootVisualElement.Q("EndTurn") as Button;
        menuBtn = doc.rootVisualElement.Q("Menu") as Button;
        menuBtn.RegisterCallback<ClickEvent>(MenuClick);

        yourTurn = doc.rootVisualElement.Q("YourTurn");

        score = doc.rootVisualElement.Q("Score") as Label;
        maxScore = doc.rootVisualElement.Q("MaxScore") as Label;
        tempScore = doc.rootVisualElement.Q("TempScore") as Label;

        playerCards.Add(new PlayerCard(doc.rootVisualElement.Q("PlayerCard1")));
        playerCards.Add(new PlayerCard(doc.rootVisualElement.Q("PlayerCard2")));
        playerCards.Add(new PlayerCard(doc.rootVisualElement.Q("PlayerCard3")));
        playerCards[0].SetName("Balbes");
        playerCards[0].SetActive(true);

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !yourTurnIsActive)
            ShowYourTurn();
    }

    public void ShowYourTurn()
    {
        if (yourTurnIsActive)
            return;

        yourTurnIsActive = true;
        AnimationYourTurnLabel();
    }
    private void AnimationYourTurnLabel()
    {
        yourTurn.style.display = DisplayStyle.Flex;
        yourTurn.style.translate = new StyleTranslate(new Translate(0, 0));

        Sequence sequence = DOTween.Sequence(this);

        // BAM!!
        float scaleT = YourTurnScaleMult;
        sequence.Append(
            DOTween.To(() => scaleT, x => scaleT = x, 1, firstPhaseTime).OnUpdate(() =>
            {
                yourTurn.style.scale = new StyleScale(new Vector2(scaleT, scaleT));
            }).SetEase(Ease.InBack));
        float opaciTy = 0f;
        sequence.Insert(0, 
            DOTween.To(() => opaciTy, x => opaciTy = x, 1, firstTransTime).OnUpdate(() =>
            {
                yourTurn.style.opacity = opaciTy;
            }));

        sequence.AppendInterval(lingerTime);

        // Slide
        float transT = 0f;
        sequence.Append(
            DOTween.To(() => transT, x => transT = x, translateDist, secondPhaseTime).OnUpdate(() =>
            {
                yourTurn.style.translate = new StyleTranslate(new Translate(0, transT));
            }).SetEase(Ease.InQuart));
        float opaciTy2 = 1f;
        sequence.Insert(firstPhaseTime + lingerTime,
            DOTween.To(() => opaciTy2, x => opaciTy2 = x, 0, secondPhaseTime).OnUpdate(() =>
            {
                yourTurn.style.opacity = opaciTy2;
            }));

        sequence.AppendCallback(() =>
        {
            yourTurnIsActive = false;
            yourTurn.style.display = DisplayStyle.None;
        });
    }

    public void SetActiveTurnBtn(bool isActive)
    {
        if (isActive)
            endTurnBtn.style.display = DisplayStyle.Flex;
        else
            endTurnBtn.style.display = DisplayStyle.None;
    }
    public void SetActiveTempScore(bool isActive)
    {
        if (isActive)
            tempScore.style.display = DisplayStyle.Flex;
        else
            tempScore.style.display = DisplayStyle.None;
    }

    public void UpdateScore(int newScore, int newMaxScore)
    {
        score.text = newScore.ToString();
        maxScore.text = newMaxScore.ToString();
        MoveTempScoreToTotal();
    }
    public void UpdateTempScore(int newTempScore)
    {
        if (newTempScore != 0)
            tempScore.style.display = DisplayStyle.Flex;
        else
            tempScore.style.display = DisplayStyle.None;

        tempScore.text = "+ " + newTempScore.ToString();
        tempScore.style.fontSize = 60 + Mathf.Max(0, (newTempScore - 500) / 25);
    }
    private void MoveTempScoreToTotal()
    {
        Sequence sequence = DOTween.Sequence(this);

        float transT = 0;
        sequence.Append(
            DOTween.To(() => transT, x => transT = x, tempScoreTranslateDist, tempScoreMoveUpTime).OnUpdate(() =>
            {
                tempScore.style.translate = new StyleTranslate(new Translate(0, transT));
            }).SetEase(Ease.InBack));

        float alphaT = 1f;
        sequence.Insert(tempScoreFadeDelay,
            DOTween.To(() => alphaT, x => alphaT = x, 0, tempScoreMoveUpTime - tempScoreFadeDelay).OnUpdate(() =>
            {
                tempScore.style.opacity = alphaT;
            }));

        sequence.AppendCallback(() =>
        {
            tempScore.style.display = DisplayStyle.None;
            tempScore.style.opacity = 1f;
            tempScore.style.translate = new StyleTranslate(new Translate(0, 0));
        });

    }
    public void DropTempScore()
    {
        Sequence sequence = DOTween.Sequence(this);

        float transT = 0;
        sequence.Append(
            DOTween.To(() => transT, x => transT = x, -tempScoreTranslateDist, tempScoreMoveUpTime).OnUpdate(() =>
            {
                tempScore.style.translate = new StyleTranslate(new Translate(0, transT));
            }).SetEase(Ease.InQuad));

        float alphaT = 1f;
        sequence.Insert(tempScoreFadeDelay,
            DOTween.To(() => alphaT, x => alphaT = x, 0, tempScoreMoveUpTime - tempScoreFadeDelay).OnUpdate(() =>
            {
                tempScore.style.opacity = alphaT;
            }));

        float randRot = Random.Range(-tempScoreRandRotationSpread, tempScoreRandRotationSpread);
        float rotT = 0;
        sequence.Insert(0,
            DOTween.To(() => rotT, x => rotT = x, randRot, tempScoreMoveUpTime).OnUpdate(() =>
            {
                tempScore.style.rotate = new StyleRotate(new Rotate(rotT));
            }).SetEase(Ease.InQuad));

        sequence.AppendCallback(() =>
        {
            tempScore.style.display = DisplayStyle.None;
            tempScore.style.opacity = 1f;
            tempScore.style.translate = new StyleTranslate(new Translate(0, 0));
            tempScore.style.rotate = new StyleRotate(new Rotate(0));
        });
    }
    private void TempScoreShake(int amplitude) //TODO:
    {
        if (seq_tempScoreShake != null && seq_tempScoreShake.IsPlaying())
            seq_tempScoreShake.Complete();

        if (amplitude == 0)
            return;

        seq_tempScoreShake = DOTween.Sequence(this);

    }


    private void EndTurnClick (ClickEvent click)
    {
        //TODO: 
    }
    private void MenuClick (ClickEvent click)
    {
        //TODO: 
    }


    public class PlayerCard
    {
        private VisualElement root;
        private VisualElement portrait;
        private VisualElement redDot;
        private Label name;
        private VisualElement score;


        public PlayerCard(VisualElement cardRoot)
        {
            root = cardRoot;
            portrait = cardRoot.Q("Portrait");
            redDot = cardRoot.Q("RedDot");
            name = cardRoot.Q("Name") as Label;
            score = cardRoot.Q("Score") as Label;
        }

        public void SetPortraitName(string newName, Texture2D newPortrait)
        {
            portrait.style.backgroundImage = new StyleBackground(newPortrait);
            name.text = newName;
        }
        public void SetName(string newName)
        {
            name.text = newName;
        }
        public void SetActiveRedDot(bool isActive)
        {
            if (isActive)
                redDot.style.display = DisplayStyle.Flex;
            else
                redDot.style.display = DisplayStyle.None;
        }
        public void SetActive(bool isActive)
        {
            if (isActive)
                root.style.display = DisplayStyle.Flex;
            else
                root.style.display = DisplayStyle.None;
        }
    }

}
