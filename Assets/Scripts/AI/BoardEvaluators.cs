using UnityEngine;

public static class BoardEvaluators 
{
    public static float NaiveEvaluate(ContuBoard board)
    {
        float score = 0;

        for (int y = 0; y < board.Height; y++)
        {
            for (int x = 0; x < board.Width; x++)
            {
                var tile = board.GetTile(x, y);
                score += NaiveTileTypeToValue(tile);

            }
        }

        for (int i = 0; i < board.TokenCount; i++)
        {
            score += NaiveTokenToValue(board.GetToken(i));
        }


        switch (board.GetBoardState())
        {
            case BoardState.P1Won:
                score = float.MaxValue;
                break;

            case BoardState.P2Won:
                score = float.MinValue;
                break;
        }

        return score;
    }

    private static float NaiveTokenToValue(Token t)
    {
        switch (t.State)
        {
            case TokenState.P1Owned:
                return 2;

            case TokenState.P2Owned:
                return -2;
        }

        return 0;
    }

    private static float NaiveTileTypeToValue(TileType t)
    {
        switch (t)
        {
            case TileType.Player1:
                return 1;

            case TileType.Player2:
                return -1;
        }

        return 0;
    }
}
