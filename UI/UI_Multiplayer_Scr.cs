using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_Multiplayer_Scr : MonoBehaviour
{
    private UIDocument doc;
    private List<Button> buttons;

    [SerializeField] private UI_HostGame_Scr hostGameUI;
    [SerializeField] private UI_JoinGame_Scr joinGameUI;
    [SerializeField] private UI_MainMenu_Scr mainMenuUI;


    private void Awake()
    {
        doc = GetComponent<UIDocument>();

        buttons = doc.rootVisualElement.Query<Button>().ToList();

        /*foreach (Button button in buttons)
            Debug.Log(button.name);*/

        buttons[0].RegisterCallback<ClickEvent>(HostGameClick); // Host
        buttons[1].RegisterCallback<ClickEvent>(JoinGameClick); // Join
        buttons[2].RegisterCallback<ClickEvent>(BackClick); // Back
    }

    private void HostGameClick(ClickEvent click)
    {
        NetworkManager_Scr.instance.StartHost(4);

        if (!hostGameUI.isActiveAndEnabled)
            hostGameUI.gameObject.SetActive(true);
        else
            hostGameUI.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
        doc.rootVisualElement.style.display = DisplayStyle.None;
        hostGameUI.EnableStartGameBtn(true);
    }
    private void JoinGameClick(ClickEvent click)
    {
        if (!joinGameUI.isActiveAndEnabled)
            joinGameUI.gameObject.SetActive(true);
        else
        {
            joinGameUI.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
            joinGameUI.SearchForLobbies();
        }
        doc.rootVisualElement.style.display = DisplayStyle.None;
    }
    private void BackClick(ClickEvent click)
    {
        mainMenuUI.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
        doc.rootVisualElement.style.display = DisplayStyle.None;
    }
}
