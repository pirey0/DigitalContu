using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class ContuConnectionHandler : ConnectionHandler, IConnectionCallbacks, IMatchmakingCallbacks, ILobbyCallbacks, IInRoomCallbacks, IOnEventCallback
{
    private static ContuConnectionHandler instance;

    [SerializeField] private AppSettings appSettings = new AppSettings();

    State currentState = State.ExpectingRegionList;

    public event System.Action RoomJoined;
    public event System.Action<Player> PlayerEnteredRoom, PlayerLeftRoom;
    public event System.Action StateChanged;
    public event System.Action<List<RoomInfo>> RoomListUpdate;

    public State CurrentState { get => currentState; }
    public static ContuConnectionHandler Instance { get => instance; }

    protected override void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        base.Awake();
    }
    public void Start()
    {
        Client = new LoadBalancingClient(ConnectionProtocol.Udp);

        Client.AddCallbackTarget(this);

        if (!this.Client.ConnectUsingSettings(appSettings))
        {
            Debug.LogError("Error while connecting");
        }

        StartFallbackSendAckThread();
    }

    public void Update()
    {
         Client.Service();
    }

    public void SetState(State newState)
    {
        currentState = newState;
        StateChanged?.Invoke();
    }

    public bool TryCreateRoom(string name)
    {
        if (CurrentState != State.ReadyForRoom)
            return false;

        EnterRoomParams roomParams = new EnterRoomParams();
        roomParams.RoomName = name;
        return Client.OpCreateRoom(roomParams);
    }

    public void RequestRooms()
    {
        
    }

    public void OnConnected()
    {
    }

    public void OnConnectedToMaster()
    {
        Debug.Log("Connected To Master");
        SetState(State.ReadyForRoom);
        Client.OpJoinLobby(TypedLobby.Default);
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log(" Disconnected (" + cause + ")");
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
        Debug.Log("Region List Received");
        SetState(State.ConnectingToMaster);
        regionHandler.PingMinimumOfRegions(this.OnRegionPingCompleted, null);
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room List Update: " + roomList.Count + " rooms");
        RoomListUpdate?.Invoke(roomList);
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
    }

    public void OnJoinedLobby()
    {
        Debug.Log("In Lobby");
    }

    public void OnLeftLobby()
    {
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
    }

    public void OnCreatedRoom()
    {
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
    }

    public void OnJoinedRoom()
    {
        SetState(State.InRoom);
        RoomJoined?.Invoke();
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
    }

    public void OnLeftRoom()
    {
        SetState(State.ReadyForRoom);
    }

    /// <summary>A callback of the RegionHandler, provided in OnRegionListReceived.</summary>
    /// <param name="regionHandler">The regionHandler wraps up best region and other region relevant info.</param>
    private void OnRegionPingCompleted(RegionHandler regionHandler)
    {
        Debug.Log("RegionPingSummary: " + regionHandler.SummaryToCache);
        Debug.Log("OnRegionPingCompleted " + regionHandler.BestRegion);
        Client.ConnectToRegionMaster(regionHandler.BestRegion.Code);
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer + " Joined the room");
        PlayerEnteredRoom?.Invoke(newPlayer);
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log( otherPlayer.NickName + " left the room");
        PlayerLeftRoom?.Invoke(otherPlayer);
    }

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("MasterClient Switched");
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case (byte)ContuEventCode.LoadScene:
                EventDrivenLoadScene((int)photonEvent.CustomData);
                break;
        }
    }

    public void EventDrivenLoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    public enum State
    {
        ExpectingRegionList,
        ConnectingToMaster,
        ReadyForRoom,
        InRoom,
        InGames
    }
}