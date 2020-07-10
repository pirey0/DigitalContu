using UnityEngine;
using UnityEngine.UI;

public class VisualToken : MonoBehaviour
{
    [SerializeField] Sprite[] typeSprites;
    Token sourceToken;
    System.Action<VisualToken> onClickCallback;

    public Token Source { get => sourceToken; }
    public void Setup(Token source, System.Action<VisualToken> onClickDelegate)
    {
        sourceToken = source;
        onClickCallback = onClickDelegate;
        GetComponent<SpriteRenderer>().sprite = typeSprites[(int)source.Type];
    }

    private void OnMouseDown()
    {
        onClickCallback.Invoke(this);
    }

    private void OnMouseEnter()
    {
        //Debug.Log("Hovering: " + Source.Type);
        //Enable description;
    }

    private void OnMouseExit()
    {
        //Disable description;
    }
}
