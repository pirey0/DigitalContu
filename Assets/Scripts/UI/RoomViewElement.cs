using UnityEngine;
using UnityEngine.UI;

public class RoomViewElement : MonoBehaviour
{
    [SerializeField] Text t_name;
    [SerializeField] Button b_join;

    public void Setup(string name, bool joinable)
    {
        t_name.text = name;
        b_join.interactable = joinable;
    }
}
