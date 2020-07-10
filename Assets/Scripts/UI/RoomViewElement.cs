using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class RoomViewElement : MonoBehaviour
{
    [SerializeField] Text t_name;
    [SerializeField] Button b_join;

    private RoomInfo room;


    public void Setup(RoomInfo room, System.Action<RoomInfo> callback)
    {
        t_name.text = room.Name;
        b_join.interactable = room.IsOpen;
        this.room = room;

        b_join.onClick.AddListener(delegate { callback.Invoke(room); });
    }
}
