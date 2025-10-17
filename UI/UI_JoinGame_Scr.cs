using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_JoinGame_Scr : MonoBehaviour
{
    private UIDocument doc;

    private ScrollView scrollView;
    private Button backBtn;
    private Button searchBtn;

    [SerializeField] private UI_MainMenu_Scr mainMenuUI;
    [SerializeField] private UI_HostGame_Scr hostGameUI;

    [SerializeField] private VisualTreeAsset lobbyVTA;

    Lobby[] foundLobbies;

    private void Awake()
    {
        doc = GetComponent<UIDocument>();

        scrollView = doc.rootVisualElement.Query<ScrollView>();
        searchBtn = doc.rootVisualElement.Query<Button>("Search");
        backBtn = doc.rootVisualElement.Query<Button>("Back");

        searchBtn.RegisterCallback<ClickEvent>(SearchClick); // Search
        backBtn.RegisterCallback<ClickEvent>(BackClick); // Back

        SearchForLobbies();
    }

    public async void SearchForLobbies()
    {
        foundLobbies = await SteamMatchmaking.LobbyList.WithMaxResults(20).RequestAsync();
        if (foundLobbies == null) { Debug.Log("Не нашёл ни одного лобби"); return; }


        ClearUI();
        foreach (Lobby lobby in foundLobbies)
            AddUIElementForLobby(lobby);
    }
    private void AddUIElementForLobby(Lobby lobby)
    {
        TemplateContainer template = lobbyVTA.Instantiate();
        VisualElement visualElement = template.ElementAt(0);
        Debug.Log(lobby.Owner.Name);
        //TODO: add join callback
        visualElement.Q<Button>().RegisterCallback<ClickEvent>(JoinClick);
        visualElement.Q<Label>("Name").text = lobby.GetData("name");
        scrollView.Add(visualElement);
    }
    private void ClearUI()
    {
        Debug.Log("почистил UI лоббов");
        scrollView.Clear();
    }


    private async void JoinClick(ClickEvent click)
    {
        Button btn = click.target as Button;
        int lobbyID = scrollView.IndexOf(btn.parent);

        RoomEnter joinedLobby = await foundLobbies[lobbyID].Join();
        if (joinedLobby != RoomEnter.Success)
        {
            //TODO: добавить какую-то обратную реакцию

            Debug.Log("Failed to join lobby from lobby list");
        }
        else
        {
            NetworkManager_Scr.instance.SetCurrentLobby(foundLobbies[lobbyID]);

            if (!hostGameUI.isActiveAndEnabled)
                hostGameUI.gameObject.SetActive(true);
            else
                hostGameUI.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
            doc.rootVisualElement.style.display = DisplayStyle.None;
            hostGameUI.EnableStartGameBtn(false);
            
            Debug.Log("Joined lobby from lobby");
        }
    }
    private void SearchClick(ClickEvent click)
    {
        SearchForLobbies();
    }
    private void BackClick(ClickEvent click)
    {
        mainMenuUI.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
        doc.rootVisualElement.style.display = DisplayStyle.None;
    }
}
