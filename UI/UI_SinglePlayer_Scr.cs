using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UI_SinglePlayer_Scr : MonoBehaviour
{
    private UIDocument doc;
    private List<Button> buttons;

    [SerializeField] private UI_MainMenu_Scr mainMenuUI;


    private void Awake()
    {
        doc = GetComponent<UIDocument>();

        buttons = doc.rootVisualElement.Query<Button>().ToList();

        /*foreach (Button button in buttons)
            Debug.Log(button.name);*/

        buttons[0].RegisterCallback<ClickEvent>(BaseGameClick); // Base Game
        buttons[1].RegisterCallback<ClickEvent>(TournamentClick); // Tournament
        buttons[2].RegisterCallback<ClickEvent>(RogueGameClick); // Rogue Game
        buttons[3].RegisterCallback<ClickEvent>(BackClick); // Back
    }

    private void BaseGameClick(ClickEvent click)
    {
        Debug.Log("SP Game started, loading scene");
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
    }
    private void TournamentClick(ClickEvent click)
    {
        //TODO: 
    }
    private void RogueGameClick(ClickEvent click)
    {
        //TODO: 
    }
    private void BackClick(ClickEvent click)
    {
        mainMenuUI.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
        doc.rootVisualElement.style.display = DisplayStyle.None;
    }
}
