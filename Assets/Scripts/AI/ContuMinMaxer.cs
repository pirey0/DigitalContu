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

    public ResultData Evaluate(int customDepth = -1, bool logSpeed = false)
    {
        System.Diagnostics.Stopwatch stopwatch = null;
        if (logSpeed)
        {
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
        }
        
        var res = MinMax(game, customDepth > 0 ? customDepth : depth, game.TurnState == TurnState.Player1);
        res.Actions.Reverse();

        if (logSpeed)
        {
            stopwatch.Stop();
            Debug.Log("MinMax took " + stopwatch.ElapsedMilliseconds.ToString() + "ms ");
        }
        
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
            var enumerator = game.GetPossibleMoves();
            while(enumerator.MoveNext())
            { 
                ContuGame subGame = ContuGame.Clone(game);
                subGame.TryAction(enumerator.Current, false, false);
                var heur = MinMax(subGame, depth - 1, false);
                if(heur.Value > value)
                {
                    value = heur.Value;
                    max = heur;
                    action = enumerator.Current;
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
            var enumerator = game.GetPossibleMoves();
            while (enumerator.MoveNext())
            {
                ContuGame subGame = ContuGame.Clone(game);
                subGame.TryAction(enumerator.Current, false, false);
                var heur = MinMax(subGame, depth - 1, true);
                if (heur.Value < value)
                {
                    value = heur.Value;
                    min = heur;
                    action = enumerator.Current;
                }
            }
            min.Actions.Add(action.Value);
            return min;
        }
    }

    public int GetPermutations(int depth)
    {
        return GetPermutationsAtDepth(game, depth);
    }


    ///Testing indiactes that the current games Permutations follow aproximately this curve: y = 0.00026*x^12
    ///With a depth of 30 this would be aprox 138.174.660.000.000 board states to evaluate....
    ///                50                     63.476.562.500.000.000
    ///                10                     260.000.000
    ///This means that to run the algorithm at depth 10 in 1second we need a board eval to take: 1/260 000 000 = 3,84ns (s^-9)
    ///That would be 1 board eval in 16cpu cyces (4GHZ) ((Impossible)
    ///Current Speeds: depth 5 (57920 permutations) 1382ms -> 1 382 000 000ns for 57920 -> 23 860 ns/boardEval
    /// Current algorithm (Crude MinMax No-opt) is 6200 times slower then desired


    private int GetPermutationsAtDepth(ContuGame game, int depth)
    {
        if (depth <= 0) // || node is leaf
            return 1;

        int count = 0;
        var enumerator = game.GetPossibleMoves();
        while (enumerator.MoveNext())
        {
            ContuGame subGame = ContuGame.Clone(game);
            subGame.TryAction(enumerator.Current, false, false);
            count += GetPermutationsAtDepth(subGame, depth - 1);
        }
        return count;
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
