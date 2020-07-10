using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ContuMinMaxer
{
    ContuGame game;
    int depth;


    public ContuMinMaxer(ContuGame game, int depth)
    {
        this.game = game;
        this.depth = depth;
    }

    public ResultData Evaluate(int customDepth = -1)
    {
        var res = MinMax(game, customDepth > 0 ? customDepth : depth, game.TurnState == TurnState.Player1);
        res.Actions.Reverse();
        return res;
    }

    private ResultData MinMax(ContuGame game, int depth, bool maximizingPlayer)
    {
        if (depth <= 0) // || node is leaf
        {
            var res = new ResultData();
            res.Value = EvaluateBoard(game.Board);
            return res;
        }

        if (maximizingPlayer)
        {
            float value = float.MinValue;
            ResultData max = null;
            ContuActionData? action = null;
            foreach (var item in game.GetPossibleMoves())
            {
                ContuGame subGame = ContuGame.Clone(game);
                subGame.TryAction(item, false, false);
                var heur = MinMax(subGame, depth - 1, false);
                if(heur.Value > value)
                {
                    value = heur.Value;
                    max = heur;
                    action = item;
                }
            }
            max.Actions.Add(action.Value);
            return max;
        }
        else
        {
            float value = float.MaxValue;
            ResultData min = null;
            ContuActionData? action = null;
            foreach (var item in game.GetPossibleMoves())
            {
                ContuGame subGame = ContuGame.Clone(game);
                subGame.TryAction(item, false, false);
                var heur = MinMax(subGame, depth - 1, true);
                if (heur.Value < value)
                {
                    value = heur.Value;
                    min = heur;
                    action = item;
                }
            }
            min.Actions.Add(action.Value);
            return min;
        }
    }

    public static float EvaluateBoard(ContuBoard board)
    {
        float score = 0;

        for (int y = 0; y < board.Height; y++)
        {
            for (int x = 0; x < board.Width; x++)
            {
                var tile = board.GetTile(x, y);
                score += TileTypeToValue(tile);

            }
        }

        for (int i = 0; i < board.TokenCount; i++)
        {
            score += TokenToValue(board.GetToken(i));
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

    private static float TokenToValue(Token t)
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

    private static float TileTypeToValue(TileType t)
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

    public class ResultData
    {
        public float Value;
        public List<ContuActionData> Actions;

        public ResultData()
        {
            Value = 0;
            Actions = new List<ContuActionData>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Eval: " + Value + " -> ");

            foreach (var a in Actions)
            {
                sb.Append(", " + a.ToString());
            }

            return sb.ToString();
        }
    }
}
