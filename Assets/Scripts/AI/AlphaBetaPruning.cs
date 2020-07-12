using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AlphaBetaPruning : GameEvaluator
{
    protected override GameEvalResult InternalEvaluate(ContuGame game, int depth)
    {
        return AlphaBeta_Rec(game, depth, float.NegativeInfinity, float.PositiveInfinity, game.TurnState == TurnState.Player1);
    }

    //https://en.wikipedia.org/wiki/Alpha%E2%80%93beta_pruning
    private GameEvalResult AlphaBeta_Rec( ContuGame game, int depth, float alpha, float beta, bool maximizingPlayer)
    {
        if (depth <= 0) // || node is leaf
        {
            var res = new GameEvalResult();
            res.Value = boardEvaluator(game.Board);
            return res;
        }

        float value = maximizingPlayer ? float.MinValue : float.MaxValue;
        GameEvalResult localRes = null;
        ContuActionData? action = null;
        var moves = game.GetPossibleMoves();
        while(moves.MoveNext())
        {
            ContuGame subGame = ContuGame.Clone(game);
            subGame.TryAction(moves.Current, false, false);
            var heur = AlphaBeta_Rec( subGame, depth - 1, alpha, beta, !maximizingPlayer);
            if (maximizingPlayer ^ heur.Value < value)
            {
                value = heur.Value;
                localRes = heur;
                action = moves.Current;
            }
            if (maximizingPlayer)
            {
                alpha = Mathf.Max(alpha, value);
                if (alpha >= beta)
                    break;
            }
            else
            {
                beta = Mathf.Min(beta, value);
                if (beta <= alpha)
                    break;
            }                
        }
        if (action == null)
        {
            var res = new GameEvalResult();
            res.Value = boardEvaluator(game.Board);
            res.Value += res.Value > 0 ? -depth*5 : depth*5;
            return res;
        }
        else
        {
            localRes.Actions.Add(action.Value);
            return localRes;
        }
    }

    private GameEvalResult AlphaBeta_Iter(ContuGame game, int depth, float alpha, float beta, bool maximizingPlayer)
    {
        int address = 10;
        Stack<int> stack = new Stack<int>();

        stack.Push(1); //inital return address

        while (stack.Count > 0)
        {

        }

        if (depth <= 0) // || node is leaf
        {
            var res = new GameEvalResult();
            res.Value = boardEvaluator(game.Board);
            return res;
        }

        float value = maximizingPlayer ? float.MinValue : float.MaxValue;
        GameEvalResult localRes = null;
        ContuActionData? action = null;
        var moves = game.GetPossibleMoves();
        while (moves.MoveNext())
        {
            ContuGame subGame = ContuGame.Clone(game);
            subGame.TryAction(moves.Current, false, false);
            var heur = AlphaBeta_Iter(subGame, depth - 1, alpha, beta, !maximizingPlayer);
            if (maximizingPlayer ^ heur.Value < value)
            {
                value = heur.Value;
                localRes = heur;
                action = moves.Current;
            }
            if (maximizingPlayer)
            {
                alpha = Mathf.Max(alpha, value);
                if (alpha >= beta)
                    break;
            }
            else
            {
                beta = Mathf.Min(beta, value);
                if (beta <= alpha)
                    break;
            }
        }
        if (action == null)
        {
            var res = new GameEvalResult();
            res.Value = boardEvaluator(game.Board);
            return res;
        }
        else
        {
            localRes.Actions.Add(action.Value);
            return localRes;
        }
    }
}
