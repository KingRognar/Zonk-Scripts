using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_DiceColoring_Scr : MonoBehaviour
{
    private UIDocument doc;

    private Button backBtn;
    private List<Button> setButtons = new List<Button>();

    [SerializeField] private UI_MainMenu_Scr mainUI;

    [SerializeField] private VisualTreeAsset coloringOptionBtn;
    


    [SerializeField] private List<DiceMaterialSetSO_Scr> diceMaterialSets;

    private void Awake()
    {
        doc = GetComponent<UIDocument>();

        backBtn = doc.rootVisualElement.Query<Button>("Back");
        backBtn.RegisterCallback<ClickEvent>(BackClick);

        InstantiateAllColoringOptions();
    }

    private void InstantiateAllColoringOptions()
    {
        VisualElement setsContainer = doc.rootVisualElement.Q("OptionsContainer");
        foreach (DiceMaterialSetSO_Scr set in diceMaterialSets)
        {
            TemplateContainer templateContainer = coloringOptionBtn.Instantiate();
            templateContainer.Q<Button>().RegisterCallback<ClickEvent>(MaterialSetClick);
            setButtons.Add(templateContainer.Q<Button>());
            templateContainer.Q("Image").style.backgroundImage = new StyleBackground(set.image);
            setsContainer.Add(templateContainer);
        }
    }

    private void BackClick(ClickEvent click)
    {
        mainUI.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
        doc.rootVisualElement.style.display = DisplayStyle.None;
    }
    private void MaterialSetClick(ClickEvent click)
    {
        Debug.Log("clicked");
        foreach (Button btn in setButtons)
        {
            btn.style.backgroundColor = new Color(0, 0, 0, 0);
        }
        VisualElement target = click.target as VisualElement;
        target.style.backgroundColor = new Color(1, 0.796f, 0, 1);
    }
}
