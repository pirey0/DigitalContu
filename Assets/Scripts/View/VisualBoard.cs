using UnityEngine;

public class VisualBoard : MonoBehaviour
{
    [SerializeField] GameObject tilePrefab, tokenPrefab;
    [SerializeField] GameObject gameHolder;

    ContuGame game;

    VisualTile[,] visualTiles;
    VisualToken visualTokens;

    private void Start()
    {
        game = gameHolder.GetComponent<IContuGameOwner>().GetGame();
    }

    private void SpawnBoard()
    {
        visualTiles = new VisualTile[game.Board.Width, game.Board.Height];

        for (int y = 0; y < visualTiles.GetLength(1); y++)
        {
            for (int x = 0; x < visualTiles.GetLength(0); x++)
            {
                visualTiles[x, y] = Instantiate(tilePrefab, new Vector3(x * 30, 0, y * 30), Quaternion.identity).GetComponent<VisualTile>();
            }
        }
    }

}
