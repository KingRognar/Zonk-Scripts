using Netcode.Transports.Facepunch;
using NUnit.Framework;
using Steamworks;
using Steamworks.Data;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkManager_Scr : MonoBehaviour
{
    [HideInInspector] public PlayerData_Scr playerData;
    public UI_HostGame_Scr hostGameUI;

    public static NetworkManager_Scr instance { get; private set; } = null;

    private FacepunchTransport transport = null;

    public Lobby? currentLobby { get; private set; } = null;

    public ulong hostId;

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        transport = GetComponent<FacepunchTransport>();
        playerData = GetComponent<PlayerData_Scr>();

        SteamMatchmaking.OnLobbyCreated += SteamMatchmaking_OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += SteamMatchmaking_OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmaking_OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmaking_OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += SteamMatchmaking_OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated += SteamMatchmaking_OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += SteamFriends_OnGameLobbyJoinRequested;

    }

    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= SteamMatchmaking_OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= SteamMatchmaking_OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= SteamMatchmaking_OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= SteamMatchmaking_OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= SteamMatchmaking_OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated -= SteamMatchmaking_OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested -= SteamFriends_OnGameLobbyJoinRequested;

        if (NetworkManager.Singleton == null)
        {
            return;
        }
        NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;

    }
    private void OnApplicationQuit()
    {
        Disconnected();
    }

    //when you accept the invite or Join on a friend
    private async void SteamFriends_OnGameLobbyJoinRequested(Lobby _lobby, SteamId _steamId)
    {
        RoomEnter joinedLobby = await _lobby.Join();
        if (joinedLobby != RoomEnter.Success)
        {
            Debug.Log("Failed to join lobby from Friends list");
        }
        else
        {
            currentLobby = _lobby;
            Debug.Log("Joined lobby from friends list");
        }
    }
    private void SteamMatchmaking_OnLobbyGameCreated(Lobby _lobby, uint _ip, ushort _port, SteamId _steamId)
    {
        hostGameUI.playerNames[0].text = _lobby.Owner.Name;
        Debug.Log("A game server has been associated with the lobby");
    }
    //friend send you an steam invite
    private void SteamMatchmaking_OnLobbyInvite(Friend _steamFriend, Lobby _lobby)
    {
        Debug.Log($"Invite from {_steamFriend.Name}");
    }
    private void SteamMatchmaking_OnLobbyMemberLeave(Lobby _lobby, Friend _steamFriend)
    {
        Debug.Log(_steamFriend.Name + " left lobby");
    }
    private void SteamMatchmaking_OnLobbyMemberJoined(Lobby _lobby, Friend _steamFriend)
    {
        Debug.Log(_steamFriend.Name + " joined lobby");
        int id = NetworkManager.Singleton.ConnectedClientsIds.Count;
        hostGameUI.playerNames[id].text = _steamFriend.Name;
        //clientNameTMP.text = "client: " + _steamFriend.Name;
    }
    private void SteamMatchmaking_OnLobbyEntered(Lobby _lobby)
    {
        Debug.Log("You've joined " + _lobby.Owner.Name + "'s lobby");
        Debug.Log(hostGameUI.playerNames[0].text);
        hostGameUI.playerNames[0].text = _lobby.Owner.Name;

        RPCManager_Scr.instance.AddPlayerToDictionaryServerRPC(NetworkManager.Singleton.LocalClientId, SteamClient.SteamId, SteamClient.Name);

        if (NetworkManager.Singleton.IsHost)
        {
            return;
        }

        //hostNameTMP.text = "Host: " + _lobby.Owner.Name;

        StartClient(currentLobby.Value.Owner.Id);


    }
    private void SteamMatchmaking_OnLobbyCreated(Result _result, Lobby _lobby)
    {
        if (_result != Result.OK)
        {
            Debug.Log("lobby was not created");
            return;
        }
        Debug.Log("lobby was created");
        _lobby.SetPublic();
        _lobby.SetJoinable(true);
        _lobby.SetGameServer(_lobby.Owner.Id);
        hostGameUI.playerNames[0].text = _lobby.Owner.Name;
        //hostNameTMP.text = "Host: " + _lobby.Owner.Name;
        Debug.Log($"{_lobby.Owner.Name} created lobby");
    }


    public async void StartHost(int _maxMembers)
    {
        NetworkManager.Singleton.OnServerStarted += Singleton_OnServerStarted;
        NetworkManager.Singleton.StartHost();
        currentLobby = await SteamMatchmaking.CreateLobbyAsync(_maxMembers);
    }
    public void StartClient(SteamId _sId)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
        transport.targetSteamId = _sId;
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client has started");
        }
    }
    public void Disconnected()
    {
        currentLobby?.Leave();
        if (NetworkManager.Singleton == null)
        {
            return;
        }
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
        }
        else
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
        }
        NetworkManager.Singleton.Shutdown(true);
        Debug.Log("disconnected");
    }

    private void Singleton_OnClientDisconnectCallback(ulong _cliendId)
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
        if (_cliendId == 0)
        {
            Disconnected();
        }
    }
    private void Singleton_OnClientConnectedCallback(ulong _cliendId)
    {
        /*NetworkTransmission.instance.AddMeToDictionaryServerRPC(SteamClient.SteamId, SteamClient.Name, _cliendId);
        GameManager.instance.myClientId = _cliendId;
        NetworkTransmission.instance.IsTheClientReadyServerRPC(false, _cliendId);*/
        Debug.Log($"Client has connected : AnotherFakeSteamName");
    }
    private void Singleton_OnServerStarted()
    {
        Debug.Log("Host started");
    }
}
