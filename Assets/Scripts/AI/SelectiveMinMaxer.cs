using System.Collections.Generic;
using UnityEngine;

public class SelectiveMinMaxer : GameEvaluator
{
    private int moveCount = 2;
    public SelectiveMinMaxer(int moveCount)
    {
        this.moveCount = moveCount;
    }

    protected override GameEvalResult InternalEvaluate(ContuGame game, int depth)
    {
        return SelectiveMinMax(moveCount, game, depth, game.TurnState == TurnState.Player1);
    }

    private GameEvalResult SelectiveMinMax(int movesCount, ContuGame game, int depth, bool maximizingPlayer)
    {
        if (depth <= 0) // || node is leaf
        {
            var res = new GameEvalResult();
            res.Value = boardEvaluator(game.Board);
            return res;
        }

        if (maximizingPlayer)
        {
            float value = float.MinValue;
            GameEvalResult max = null;
            ContuActionData? action = null;
            var moves = GetBestMoves(movesCount, game, maximizingPlayer);
            foreach (var move in moves)
            {
                ContuGame subGame = ContuGame.Clone(game);
                subGame.TryAction(move, false, false);
                var heur = SelectiveMinMax(movesCount, subGame, depth - 1, false);
                if (heur.Value > value)
                {
                    value = heur.Value;
                    max = heur;
                    action = move;
                }
            }
            max.Actions.Add(action.Value);
            return max;
        }
        else
        {
            float value = float.MaxValue;
            GameEvalResult min = null;
            ContuActionData? action = null;
            var moves = GetBestMoves(movesCount, game, maximizingPlayer);
            foreach (var move in moves)
            {
                ContuGame subGame = ContuGame.Clone(game);
                subGame.TryAction(move, false, false);
                var heur = SelectiveMinMax(movesCount, subGame, depth - 1, true);
                if (heur.Value < value)
                {
                    value = heur.Value;
                    min = heur;
                    action = move;
                }
            }
            min.Actions.Add(action.Value);
            return min;
        }
    }

    private ContuActionData[] GetBestMoves(int count, ContuGame game, bool max)
    {
        var enumerator = game.GetPossibleMoves();
        var data = new List<ContuActionData>();
        var evals = new List<float>();

        while (enumerator.MoveNext())
        {
            data.Add(enumerator.Current);

            ContuGame subGame = ContuGame.Clone(game);
            subGame.TryAction(enumerator.Current, false, false);
            evals.Add(boardEvaluator.Invoke(subGame.Board));
        }

        var indexList = new List<int>();

        for (int i = 0; i < data.Count; i++)
        {
            indexList.Add(i);
        }

        if(max)
        indexList.Sort((i1, i2) => evals[i1] > evals[i2]? -1 : 1);
        else
        indexList.Sort((i1, i2) => evals[i1] > evals[i2] ? 1 : -1);
        
        var res = new List<ContuActionData>();
        int locMin = Mathf.Min(data.Count, count);
        for (int i = 0; i < locMin; i++)
        {
            res.Add(data[indexList[i]]);
        }

        return res.ToArray();
    }

    public override int GetPermutations(int depth)
    {
        return (int) Mathf.Pow(moveCount, depth);
    }

}
