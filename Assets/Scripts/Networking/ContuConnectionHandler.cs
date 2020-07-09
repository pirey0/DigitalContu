using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class ContuConnectionHandler : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks, ILobbyCallbacks, IInRoomCallbacks
{
    [SerializeField] private AppSettings appSettings = new AppSettings();
    [SerializeField] private ConnectionHandler connectionHandler;


    private LoadBalancingClient client;

    public event System.Action RoomJoined;

    public LoadBalancingClient Client { get => client; } 

    public void Start()
    {
        this.client = new LoadBalancingClient();
        this.client.AddCallbackTarget(this);

        if (!this.client.ConnectUsingSettings(appSettings))
        {
            Debug.LogError("Error while connecting");
        }

        if (this.connectionHandler != null)
        {
            this.connectionHandler.Client = this.client;
            this.connectionHandler.StartFallbackSendAckThread();
        }
    }

    public void Update()
    {
        if (client != null)
        {
            client.Service();
        }
    }

    public void OnConnected()
    {
    }

    public void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster");
        this.client.OpJoinRandomRoom();    // joins any open room (no filter)
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("OnDisconnected(" + cause + ")");
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
        Debug.Log("OnRegionListReceived");
        regionHandler.PingMinimumOfRegions(this.OnRegionPingCompleted, null);
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
    }

    public void OnJoinedLobby()
    {
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
        Debug.Log("Room Joined");
        RoomJoined?.Invoke();
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed");
        this.client.OpCreateRoom(new EnterRoomParams());
    }

    public void OnLeftRoom()
    {
    }

    /// <summary>A callback of the RegionHandler, provided in OnRegionListReceived.</summary>
    /// <param name="regionHandler">The regionHandler wraps up best region and other region relevant info.</param>
    private void OnRegionPingCompleted(RegionHandler regionHandler)
    {
        Debug.Log("RegionPingSummary: " + regionHandler.SummaryToCache);
        Debug.Log("OnRegionPingCompleted " + regionHandler.BestRegion);
        this.client.ConnectToRegionMaster(regionHandler.BestRegion.Code);
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer + " Joined the room");
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log( otherPlayer.NickName + " left the room");
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
}

