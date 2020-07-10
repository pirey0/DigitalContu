using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] Text t_state, t_inLobbyInfo;
    [SerializeField] GameObject w_createLobby, w_joinRoom, w_main, w_viewRoom;
    [SerializeField] GameObject prefab_joinRoomView;
    [SerializeField] InputField i_createLobbyName;

    [SerializeField] Transform t_roomListParent;
    [SerializeField] Button b_beginGame;

    private void Start()
    {
        EnterMain();
        ContuConnectionHandler.Instance.Client.NickName = "UnnamedGuest" + UnityEngine.Random.Range(1000, 10000);
    }

    private void OnEnable()
    {
        ContuConnectionHandler.Instance.StateChanged += OnStateChanged;
        ContuConnectionHandler.Instance.RoomListUpdate += OnRoomListUpdated;
        ContuConnectionHandler.Instance.PlayerEnteredRoom += OnPlayerEnteredRoom;
        ContuConnectionHandler.Instance.PlayerLeftRoom += OnPlayerLeftRoom;
        ContuConnectionHandler.Instance.RoomJoined += OnRoomJoined;
    }

    private void OnDisable()
    {
        ContuConnectionHandler.Instance.StateChanged -= OnStateChanged;
        ContuConnectionHandler.Instance.RoomListUpdate -= OnRoomListUpdated;
    }

    private void OnRoomListUpdated(List<RoomInfo> rooms)
    {
        for (int i = 0; i < t_roomListParent.childCount; i++)
        {
            Destroy(t_roomListParent.GetChild(i).gameObject);
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            var roomview = Instantiate(prefab_joinRoomView, new Vector3(0, -40 * i, 0), Quaternion.identity, t_roomListParent).GetComponent<RoomViewElement>();
            var room = rooms[i];
            roomview.Setup(room.Name, room.IsOpen);
        }
    }

    private void OnStateChanged()
    {
        t_state.text = ContuConnectionHandler.Instance.CurrentState.ToString();
    }

    private void OnPlayerLeftRoom(Player obj)
    {
        UpdateInRoomView();
    }

    private void OnPlayerEnteredRoom(Player obj)
    {
        UpdateInRoomView();
    }
    private void OnRoomJoined()
    {
        UpdateInRoomView();
    }

    private void UpdateInRoomView()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in ContuConnectionHandler.Instance.Client.CurrentRoom.Players)
        {
            sb.Append(item.Key + ": " + item.Value.NickName + Environment.NewLine);
        }

        t_inLobbyInfo.text = sb.ToString();
        b_beginGame.interactable = ContuConnectionHandler.Instance.Client.LocalPlayer.IsMasterClient && ContuConnectionHandler.Instance.Client.CurrentRoom.PlayerCount > 1;
    }

    public void EnterCreateRoom()
    {
        w_createLobby.SetActive(true);
        w_joinRoom.SetActive(false);
        w_main.SetActive(false);
        w_viewRoom.SetActive(false);
    }
    
    public void EnterJoinRoom()
    {
        w_createLobby.SetActive(false);
        w_joinRoom.SetActive(true);
        w_main.SetActive(false);
        w_viewRoom.SetActive(false);
    }

    public void EnterMain()
    {
        w_createLobby.SetActive(false);
        w_joinRoom.SetActive(false);
        w_main.SetActive(true);
        w_viewRoom.SetActive(false);
    }

    public void EnterRoomView()
    {
        w_createLobby.SetActive(false);
        w_joinRoom.SetActive(false);
        w_main.SetActive(false);
        w_viewRoom.SetActive(true);
    }

    public void CreateRoom()
    {
        string lobbyName = i_createLobbyName.text;
        if (lobbyName.IsNullOrEmpty())
            lobbyName = "newRoom" + UnityEngine.Random.Range(1000, 9999);

        if (ContuConnectionHandler.Instance.TryCreateRoom(lobbyName))
        {
            EnterRoomView();
        }
    }

    public void LeaveRoom()
    {
        ContuConnectionHandler.Instance.Client.OpLeaveRoom(false);
        EnterMain();
    }

    public void SetMyName(string name)
    {
        ContuConnectionHandler.Instance.Client.NickName = name;
        Debug.Log("Updated Nickname to " + name);
    }

    public void StartGame()
    {
        int SCENE_ID = 1;
        ContuConnectionHandler.Instance.Client.CurrentRoom.IsOpen = false;
        ContuConnectionHandler.Instance.Client.OpRaiseEvent((byte)ContuEventCode.LoadScene, SCENE_ID, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
        ContuConnectionHandler.Instance.EventDrivenLoadScene(SCENE_ID);
    }
}
