using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using Sequence = DG.Tweening.Sequence;

public class UI_MainMenu_Scr : MonoBehaviour
{
    private UIDocument doc;

    private List<Button> buttons;
    private VisualElement zonkLabel;

    [SerializeField] private float zonkRotationAmplitude, zonkMoveAmplitude, zonkRotTime, zonkMovTime;

    [SerializeField] private UI_HostGame_Scr hostGameUI;
    [SerializeField] private UI_DiceColoring_Scr diceColoringUI;
    [SerializeField] private UI_Rules_Scr rulesUI;

    private void Awake()
    {
        doc = GetComponent<UIDocument>();

        buttons = doc.rootVisualElement.Query<Button>().ToList();
        zonkLabel = doc.rootVisualElement.Q("Zonk");

        /*foreach (Button button in buttons)
            Debug.Log(button.name);*/

        buttons[0].RegisterCallback<ClickEvent>(HostGameClick); // Host
        buttons[1].RegisterCallback<ClickEvent>(JoinGameClick); // Join
        buttons[2].RegisterCallback<ClickEvent>(DicesClick); // Dices
        buttons[3].RegisterCallback<ClickEvent>(OptionsClick); // Options
        buttons[4].RegisterCallback<ClickEvent>(RulesClick); // Rules
        buttons[5].RegisterCallback<ClickEvent>(ExitClick); // Exit
    }
    private void Start()
    {
        AnimateZonk();
        DisableOtherMenus();
    }

    #region Click Events
    private void HostGameClick(ClickEvent click)
    {
        NetworkManager_Scr.instance.StartHost(4);

        if (!hostGameUI.isActiveAndEnabled)
            hostGameUI.gameObject.SetActive(true);
        else
            hostGameUI.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
        doc.rootVisualElement.style.display = DisplayStyle.None;
    }
    private void JoinGameClick(ClickEvent click)
    {

    }
    private void DicesClick(ClickEvent click)
    {
        if (!diceColoringUI.isActiveAndEnabled)
            diceColoringUI.gameObject.SetActive(true);
        else
            diceColoringUI.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
        doc.rootVisualElement.style.display = DisplayStyle.None;
    }
    private void OptionsClick(ClickEvent click)
    {

    }
    private void RulesClick(ClickEvent click)
    {
        if (!rulesUI.isActiveAndEnabled)
            rulesUI.gameObject.SetActive(true);
        else
            rulesUI.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
        doc.rootVisualElement.style.display = DisplayStyle.None;
    }
    private void ExitClick(ClickEvent click)
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    #endregion

    private void AnimateZonk()
    {
        float tempRot = 0;
        DOTween.To(() => tempRot, x => tempRot = x, zonkRotationAmplitude, zonkRotTime/2).OnUpdate(() => 
        {
            zonkLabel.style.rotate = new StyleRotate(new Rotate(new Angle(tempRot, AngleUnit.Degree)));
        }).SetEase(Ease.OutSine).OnComplete(() => 
        {
            DOTween.To(() => tempRot, x => tempRot = x, -zonkRotationAmplitude, zonkRotTime).OnUpdate(() =>
            {
                zonkLabel.style.rotate = new StyleRotate(new Rotate(new Angle(tempRot, AngleUnit.Degree)));
            }).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        });

        float tempMov = 0;
        DOTween.To(() => tempMov, x => tempMov = x, zonkMoveAmplitude, zonkMovTime / 2).OnUpdate(() =>
        {
            zonkLabel.style.translate = new StyleTranslate(new Translate(tempMov, 0));
        }).SetEase(Ease.OutSine).OnComplete(() =>
        {
            DOTween.To(() => tempMov, x => tempMov = x, -zonkMoveAmplitude, zonkMovTime).OnUpdate(() =>
            {
                zonkLabel.style.translate = new StyleTranslate(new Translate(tempMov, 0));
            }).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        });
    }
    private void DisableOtherMenus()
    {
        hostGameUI.gameObject.SetActive(false);
    }
}
