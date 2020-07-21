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
        return SelectiveMinMax_Rec(moveCount, game, depth, game.TurnState == TurnState.Player1);
    }

    private GameEvalResult SelectiveMinMax_Rec(int movesCount, ContuGame game, int depth, bool maximizingPlayer)
    {
        if (depth <= 0) // || node is leaf
        {
            var res = new GameEvalResult();
            res.Value = RunBoardEvaluator(game);
            return res;
        }

        float value = maximizingPlayer ? float.MinValue : float.MaxValue;
        GameEvalResult localRes = null;
        ContuActionData? action = null;
        var moves = GetBestMoves(movesCount, game, maximizingPlayer);
        foreach (var move in moves)
        {
            ContuGame subGame = ContuGame.Clone(game);
            subGame.TryAction(move, false, false);
            var heur = SelectiveMinMax_Rec(movesCount, subGame, depth - 1, !maximizingPlayer);
            if (maximizingPlayer ^ heur.Value < value)
            {
                value = heur.Value;
                localRes = heur;
                action = move;
            }
        }
        if (action == null)
        {
            var res = new GameEvalResult();
            res.Value = RunBoardEvaluator(game);
            if (res.Value > 10000)
                res.Value -= depth;
            else if (res.Value < -10000)
                res.Value += depth;

            return res;
        }
        else
        {
            localRes.Actions.Add(action.Value);
            return localRes;
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
            evals.Add(RunBoardEvaluator(subGame));
        }

        var indexList = new List<int>();

        for (int i = 0; i < data.Count; i++)
        {
            indexList.Add(i);
        }

        if (max)
            indexList.Sort((i1, i2) => evals[i1] > evals[i2] ? -1 : 1);
        else
            indexList.Sort((i1, i2) => evals[i1] < evals[i2] ? -1 : 1);

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
        return (int)Mathf.Pow(moveCount, depth);
    }

}
