using System;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ContuBoard
{
    private int width, height;
    private TileType[,] tiles;
    private Token[] tokens;

    public event System.Action<int, int, TileType> TileChanged;
    public event System.Action TokensUpdated;

    public int Width { get => width; }
    public int Height { get => height; }
    public int TokenCount { get => tokens.Length; }

    public static ContuBoard CreateDefault()
    {
        ContuBoard board = new ContuBoard();
        board.width = 5;
        board.height = 10;
        board.tiles = new TileType[5, 10];
        board.tokens = new Token[4];
        board.tokens[0] = new Token(TokenType.Archer, 2);
        board.tokens[1] = new Token(TokenType.Guard, 2);
        board.tokens[2] = new Token(TokenType.Knight, 2);
        board.tokens[3] = new Token(TokenType.Veteran, 2);

        board.tiles[2, 2] = TileType.Player1;
        board.tiles[2, 7] = TileType.Player2;

        return board;
    }

    public TileType GetTile(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return TileType.Null;

        return tiles[x, y];
    }

    public void SetTile(int x, int y, TileType type, bool onlyIfEmpty = false)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return;

        //Dont allow change of spawn pieces
        if (x == width / 2 && (y == 2 || y == height - 2))
            return;

        if (onlyIfEmpty && tiles[x, y] != TileType.Empty)
            return;

        tiles[x, y] = type;
        TileChanged?.Invoke(x, y, type);
    }

    public bool CanPlaceTile(int x, int y, int playerId)
    {
        if(GetTile(x,y) == TileType.Empty)
        {
            return IsPlayerTileInNeighbours(x, y, playerId);
        }
        return false;
    }

    public bool IsPlayerTileInNeighbours(int x, int y, int playerId)
    {
        TileType type = (playerId == 0 ? TileType.Player1 : TileType.Player2);

        bool res = GetTile(x + 1, y) == type;;
        res = res | GetTile(x-1, y) == type;
        res = res | GetTile(x, y+1) == type;
        res = res | GetTile(x, y-1) == type;

        return res;
    }

    public bool TileIsInPlayersHalf(int x, int y, int playerId)
    {
        //XNOR(P1, y<5)
        return !(playerId == 0 ^ y < height / 2);
    } 

    public Token GetFirstTokenOfType(TokenType type)
    {
        foreach (var token in tokens)
        {
            if (token.Type == type)
                return token;
        }

        return null;
    }

    public int GetTokenCountForUser(int userid)
    {
        int tokencount = 0;

        foreach (var token in tokens)
        {
            if (token.State == (userid == 0 ? TokenState.P1Owned : TokenState.P2Owned))
                tokencount++;
        }

        return tokencount;
    }

    public Token GetToken(int index)
    {
        if (index < 0 || index > tokens.Length)
            return null;

        return tokens[index];
    }

    public BoardState GetBoardState()
    {
        if (GetTile(2, 0) == TileType.Player2)
        {
            return BoardState.P2Won;
        }
        else if (GetTile(2, 9) == TileType.Player1)
        {
            return BoardState.P1Won;
        }

        return BoardState.Playing;
    }

    public void TickTokens()
    {
        bool changed = false;
        foreach (var t in tokens)
        {
            if (t.Tick())
                changed = true;
        }

        if (changed)
            TokensUpdated?.Invoke();
    }

    public bool CheckRuleOf5()
    {
        if(CheckHorizontalRuleOf5())
        {
            return true;
        }
        else
        {
            return CheckVerticalRuleOf5();
        }
    }

    private bool CheckHorizontalRuleOf5()
    {
        for (int y = 0; y < height; y++)
        {
            TileType type = GetTile(0, y);
            if (type != TileType.Player1 && type != TileType.Player2)
                continue;

            bool broken = false;
            for (int x = 1; x < width; x++)
            {
                if (type != GetTile(x, y))
                {
                    broken = true;
                    break;
                }
            }

            if (!broken) //Rule of 5 found
            {
                for (int i = 0; i < width; i++)
                {
                    SetTile(i, y, TileType.Empty);
                }
                return true;
            }
        }

        return false;
    }

    private bool CheckVerticalRuleOf5()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y1 = 0; y1 < height-4; y1++)
            {

                TileType type = GetTile(x, y1);
                if (type != TileType.Player1 && type != TileType.Player2)
                    continue;

                bool broken = false;
                for (int y2 = 0; y2 < 5; y2++)
                {
                    if (type != GetTile(x, y1 + y2))
                    {
                        broken = true;
                        break;
                    }
                }

                if (!broken) //Rule of 5 found
                {
                    for (int y2 = 0; y2 < 5; y2++)
                    {
                        SetTile(x, y1 + y2, TileType.Empty);
                    }
                    return true;
                }
            }
        }

        return false;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                sb.Append(TileTypeToString(tiles[x, y]));
            }

            sb.Append(Environment.NewLine);
        }

        sb.Append(Environment.NewLine);

        sb.Append("Tokens:");

        for (int i = 0; i < tokens.Length; i++)
        {
            sb.Append(Environment.NewLine);
            sb.Append(tokens[i].ToString());
        }

        return sb.ToString();
    }

    private string TileTypeToString(TileType t)
    {
        switch (t)
        {
            case TileType.Empty:
                return "_";
            case TileType.Player2:
                return "□";
            case TileType.Player1:
                return "■";
        }

        return "E";
    }

}

public enum BoardState
{
    Playing,
    P1Won,
    P2Won
}

public enum TileType
{
    Empty,
    Player1,
    Player2,
    Null
}
