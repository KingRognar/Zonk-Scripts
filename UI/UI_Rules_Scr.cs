using UnityEngine;
using UnityEngine.UIElements;

public class UI_Rules_Scr : MonoBehaviour
{
    private UIDocument doc;

    Button backBtn;

    [SerializeField] private UI_MainMenu_Scr mainUI; 

    private void Awake()
    {
        doc = GetComponent<UIDocument>();

        backBtn = doc.rootVisualElement.Query<Button>("Back");
        backBtn.RegisterCallback<ClickEvent>(BackClick);
    }

    private void BackClick(ClickEvent click)
    {
        mainUI.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
        doc.rootVisualElement.style.display = DisplayStyle.None;
    }
}
