using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UI_IngameMaster_Scr : MonoBehaviour
{
    [SerializeField] private UIDocument mainDoc;
    [SerializeField] private UIDocument rulesDoc;

    private List<Button> mainBtns;
    private Button rulesBackBtn;

    private bool isUiEnabled = false; 
    
    
    //TODO: переделать чтоб выходил из разных менюшек в основную а затем закрывал
    //TODO: выключать интеракции

    private void Awake()
    {
        mainDoc.rootVisualElement.style.display = DisplayStyle.None;
        rulesDoc.rootVisualElement.style.display = DisplayStyle.None;


        mainBtns = mainDoc.rootVisualElement.Query<Button>().ToList();

        mainBtns[0].RegisterCallback<ClickEvent>(ContinueClick);// continue
        mainBtns[1].RegisterCallback<ClickEvent>(OptionsClick);// options
        mainBtns[2].RegisterCallback<ClickEvent>(RulesClick);// rules
        mainBtns[3].RegisterCallback<ClickEvent>(ExitMenuClick);// exit to main menu
        mainBtns[4].RegisterCallback<ClickEvent>(ExitGameClick);// exit game

        rulesBackBtn = rulesDoc.rootVisualElement.Query<Button>("Back");
        rulesBackBtn.RegisterCallback<ClickEvent>(RulesBackClick);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isUiEnabled)
                mainDoc.rootVisualElement.style.display = DisplayStyle.None;
            else
                mainDoc.rootVisualElement.style.display = DisplayStyle.Flex;
            isUiEnabled = !isUiEnabled;
        }
    }

    private void ContinueClick(ClickEvent click)
    {
        mainDoc.rootVisualElement.style.display = DisplayStyle.None;
        isUiEnabled = !isUiEnabled;
    }
    private void OptionsClick(ClickEvent click)
    {
        //TODO:
    }
    private void RulesClick(ClickEvent click)
    {
        rulesDoc.rootVisualElement.style.display = DisplayStyle.Flex;
        mainDoc.rootVisualElement.style.display = DisplayStyle.None;
    }
    private void ExitMenuClick(ClickEvent click)
    {
        //TODO: надо перед этим дисконектится
        NetworkManager.Singleton.SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
    }
    private void ExitGameClick(ClickEvent click)
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void RulesBackClick(ClickEvent click)
    {
        mainDoc.rootVisualElement.style.display = DisplayStyle.Flex;
        rulesDoc.rootVisualElement.style.display = DisplayStyle.None;
    }
}
