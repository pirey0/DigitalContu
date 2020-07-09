using UnityEngine;
using UnityEngine.Tilemaps;

public static class TokenEffects
{
    /*
    Guard: 5 parameters 2 for piece to remove, 2 for piece to place
    Archer: 5 parameters, 2 for own piece, 2 for target piece
    Knight: 5 parameters, 2 for first step, 2 for second step
    Veteran: 5 parameters, 2 for own piece, 2 for target piece
     */

    public static bool CanUseToken(ContuBoard board, int userId, params int[] parameters)
    {
        if (parameters.Length != 5)
            return false;

        switch ((TokenType)parameters[0])
        {
            case TokenType.Guard:
                return CanUseGuard(board, userId, parameters);
            case TokenType.Archer:
                return CanUseArcher(board, userId, parameters);
            case TokenType.Knight:
                return CanUseKnight(board, userId, parameters);
            case TokenType.Veteran:
                return CanUseVeteran(board, userId, parameters);
        }

        return false;
    }

    private static bool CanUseGuard(ContuBoard board, int userId, params int[] parameters)
    {
        if (!board.TileIsInPlayersHalf(parameters[1], parameters[2], userId) || !board.TileIsInPlayersHalf(parameters[3], parameters[4], userId))
            return false;

        if(DoesNotBelongToPlayer(board, parameters[1], parameters[2], userId, oppositeCheck: true))
            return false;   
        
        if(!board.CanPlaceTile(parameters[3], parameters[4], userId))
            return false;

        return true;
    }

    private static bool CanUseArcher(ContuBoard board, int userId, params int[] parameters)
    {
        if (DoesNotBelongToPlayer(board, parameters[1], parameters[2], userId))
            return false;

        if (DirectionBetween2Tiles(parameters[1], parameters[2], parameters[3], parameters[4]) == Direction.Error)
            return false;

        return true;
    }

    private static bool CanUseKnight(ContuBoard board, int userId, params int[] parameters)
    {
        if (!board.IsPlayerTileInNeighbours(parameters[1], parameters[2], userId))
            return false;

        if (!board.CanPlaceTile(parameters[1], parameters[2], userId))
            return false;

        return true;
    }

    private static bool CanUseVeteran(ContuBoard board, int userId, params int[] parameters)
    {
        if (DoesNotBelongToPlayer(board, parameters[1], parameters[2], userId))
            return false;

        if (DoesNotBelongToPlayer(board, parameters[3], parameters[4], userId, oppositeCheck: true))
            return false;

        if (DirectionBetween2Tiles(parameters[1], parameters[2], parameters[3], parameters[4]) == Direction.Error)
            return false;

        return true;
    }

    private static bool DoesNotBelongToPlayer(ContuBoard board, int x, int y, int userId, bool oppositeCheck = false)
    {
        return board.GetTile(x, y) != UserIdToTileType(userId, oppositeCheck);
    }

    public static void UseToken(ContuBoard board, int userId, params int[] parameters)
    {
        TokenType type = (TokenType)parameters[0];

        switch (type)
        {
            case TokenType.Guard: //remove enemy in your half, place one of yours in your half
                board.SetTile(parameters[1], parameters[2], TileType.Empty);
                board.SetTile(parameters[3], parameters[4], UserIdToTileType(userId));
                break;

            case TokenType.Archer: // 3 in a line are deleted that are attached to you
                var tiles = GetTilesAffectedByArcher(parameters);
                board.SetTile(tiles[0], tiles[1], TileType.Empty);
                board.SetTile(tiles[2], tiles[3], TileType.Empty);
                board.SetTile(tiles[4], tiles[5], TileType.Empty);
                break;

            case TokenType.Knight: //row of 3
                TileType target = UserIdToTileType(userId);
                var lastTile = GetLastKnightTile(parameters);
                board.SetTile(parameters[1], parameters[2], target, onlyIfEmpty: true);
                board.SetTile(parameters[3], parameters[4], target, onlyIfEmpty: true);
                board.SetTile(lastTile.x, lastTile.y, target, onlyIfEmpty: true);
                break;

            case TokenType.Veteran: //replace enemy piece, leaves empty behind
                board.SetTile(parameters[1], parameters[2], TileType.Empty);
                board.SetTile(parameters[3], parameters[4], UserIdToTileType(userId));
                break;
        }
    }

    private static TileType UserIdToTileType(int userId, bool opponent = false)
    {
        if (opponent)
        {
            return userId == 1 ? TileType.Player1 : TileType.Player2;
        }
        else
        {
            return userId == 0 ? TileType.Player1 : TileType.Player2;

        }
    }

    private static int[] GetTilesAffectedByArcher(int[] parameters)
    {
        var res = new int[6];

        res[0] = parameters[3];
        res[1] = parameters[4];

        var dir = DirectionBetween2Tiles(parameters[1], parameters[2], parameters[3], parameters[4]);

        var p2 = dir.Rotate(1).AddTo(res[0], res[1]);
        var p3 = dir.Rotate(3).AddTo(res[0], res[1]);

        res[2] = p2.x;
        res[3] = p2.y;
        res[4] = p3.x;
        res[5] = p3.y;

        return res;
    }

    private static Vector2Int GetLastKnightTile(int[] parameters)
    {
        var res = new int[2];

        var dir = DirectionBetween2Tiles(parameters[1], parameters[2], parameters[3], parameters[4]);

        return dir.AddTo(parameters[3], parameters[4]);
    }

    public static Direction DirectionBetween2Tiles(int x1, int y1, int x2, int y2)
    {
        if(Mathf.Abs(x1-x2) + Mathf.Abs(y1-y2) != 1)
        {
            return Direction.Error;
        }

        if (x2 > x1)
            return Direction.Right;
        else if (x2 < x1)
            return Direction.Left;
        else if (y2 > y1)
            return Direction.Up;
        else
            return Direction.Down;
    }

}

public enum Direction
{
    Up, Right, Down, Left, Error
}

public static class DirectionExtension
{
    public static Direction Rotate(this Direction dir, int stepsIn90Deg)
    {
        return (Direction)(((int)dir+stepsIn90Deg)%4);
    }

    public static Vector2Int AddTo(this Direction dir, int x, int y)
    {
        return new Vector2Int(x, y) + dir.ToVector();
    }

    public static Vector2Int ToVector(this Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                return new Vector2Int(0, 1);
            case Direction.Right:
                return new Vector2Int(1, 0);
            case Direction.Down:
                return new Vector2Int(0, -1);
            case Direction.Left:
                return new Vector2Int(-1, 0);

            default:
                return Vector2Int.zero;
        }
    }
}
