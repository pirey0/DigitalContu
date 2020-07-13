using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualToken3D : MonoBehaviour
{
    Token token;
    Image image;

    [SerializeField] TokenState state;

    VisualBoard3D board;

    public Token Source { get => token; }

    public void SetupToken(Token _token, Sprite _sprite, VisualBoard3D _board)
    {
        token = _token;

        board = _board;

        image = GetComponent<Image>();

        if (image != null)
            image.sprite = _sprite;
    }

    public void OnClick() {
        if (token != null) {
            board.InteractWithToken(this);
        }
    }
}
