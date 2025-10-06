using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UI_HostGame_Scr : MonoBehaviour
{
    private UIDocument doc;

    public List<Label> playerNames;
    private List<Button> buttons;

    [SerializeField] private UI_MainMenu_Scr mainMenuUI;

    private void Awake()
    {
        Debug.Log("init");

        doc = GetComponent<UIDocument>();

        buttons = doc.rootVisualElement.Query<Button>().ToList();
        playerNames = doc.rootVisualElement.Query<Label>("Name").ToList();
        Debug.Log(buttons.Count + "   " + playerNames.Count);

        buttons[0].RegisterCallback<ClickEvent>(StartGameClick); // Start
        buttons[1].RegisterCallback<ClickEvent>(BackClick); // Back
    }

    #region Click Events
    private void StartGameClick(ClickEvent click)
    {
        Debug.Log("game started, loading scene");
        NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
    }
    private void BackClick(ClickEvent click)
    {
        //TODO: отрубать лоббу

        mainMenuUI.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
        doc.rootVisualElement.style.display = DisplayStyle.None;
    }
    #endregion
}
