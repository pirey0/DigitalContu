using UnityEngine;

[DefaultExecutionOrder(200)]
public class VisualBoard : MonoBehaviour
{
    [SerializeField] GameObject tilePrefab, tokenPrefab;
    [SerializeField] GameObject gameHolder;

    ContuGame game;

    VisualTile[,] visualTiles;
    VisualToken[] visualTokens;

    private void Start()
    {
        game = gameHolder.GetComponent<IContuGameOwner>().GetGame();
        SpawnBoard();
    }

    private void SpawnBoard()
    {
        visualTiles = new VisualTile[game.Board.Width, game.Board.Height];

        for (int y = 0; y < visualTiles.GetLength(1); y++)
        {
            for (int x = 0; x < visualTiles.GetLength(0); x++)
            {
                visualTiles[x, y] = Instantiate(tilePrefab, new Vector3(x +0.5f, y + 0.5f, 0), Quaternion.identity).GetComponent<VisualTile>();
                visualTiles[x, y].SwitchTo(game.Board.GetTile(x, y));
            }
        }
    }


}
