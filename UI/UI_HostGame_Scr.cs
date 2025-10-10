using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UI_HostGame_Scr : MonoBehaviour
{
    private UIDocument doc;


/*    public NetworkVariable<FixedString128Bytes> player1Name = new NetworkVariable<FixedString128Bytes>();
    public NetworkVariable<FixedString128Bytes> player2Name = new NetworkVariable<FixedString128Bytes>();
    public NetworkVariable<FixedString128Bytes> player3Name = new NetworkVariable<FixedString128Bytes>();
    public NetworkVariable<FixedString128Bytes> player4Name = new NetworkVariable<FixedString128Bytes>();*/

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

        if (NetworkManager.Singleton.IsClient)
            Sync();
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

    public void Sync()
    {
        for (int i = 0; i < 4; i++)
        {
            playerNames[i].text = RPCManager_Scr.instance.playerNamesNV[i].Value.ToString();
        }
    }
}
