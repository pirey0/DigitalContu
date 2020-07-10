using UnityEngine;

public class VisualTile : MonoBehaviour
{
    [SerializeField] Sprite[] stateSprites;

    SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SwitchTo(TileType type)
    {
        spriteRenderer.sprite = stateSprites[(int)type];
    }
}
