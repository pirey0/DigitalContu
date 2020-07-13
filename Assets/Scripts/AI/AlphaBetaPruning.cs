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
        Stack<object> stack = new Stack<object>();

        stack.Push(10); //inital return address
        stack.Push(game);
        stack.Push(depth);
        stack.Push(alpha);
        stack.Push(beta);
        stack.Push(maximizingPlayer);
        stack.Push(null);
        stack.Push(1); //start call

        while (stack.Count > 0)
        {
            int address = (int)stack.Pop();

            switch (address)
            {
                case 1:
                    GameEvalResult lastRes = (GameEvalResult)stack.Pop();
                    bool tempMaximizing = (bool)stack.Pop();
                    float tempBeta = (float)stack.Pop();
                    float tempAlpha = (float)stack.Pop();
                    int tempDepth = (int)stack.Pop();
                    ContuGame tempGame = (ContuGame)stack.Pop();

                    if (tempDepth <= 0)
                    {
                        int tempAddress = (int)stack.Pop();

                        var res = new GameEvalResult();
                        res.Value = boardEvaluator(tempGame.Board);
                        stack.Push(tempGame);
                        stack.Push(tempDepth);
                        stack.Push(tempAlpha);
                        stack.Push(tempBeta);
                        stack.Push(!tempMaximizing);
                        stack.Push(res);
                        stack.Push(tempAddress);
                        continue;
                    }

                    var moves = game.GetPossibleMoves();

                    stack.Push(moves);
                    stack.Push(tempMaximizing ? float.MinValue : float.MaxValue);
                    stack.Push(tempGame);
                    stack.Push(tempDepth - 1);
                    stack.Push(tempAlpha);
                    stack.Push(tempBeta);
                    stack.Push(!tempMaximizing);
                    stack.Push(lastRes);
                    stack.Push(6); //broken loop
                    break;

                case 6:
                    lastRes = (GameEvalResult)stack.Pop();
                    tempMaximizing = (bool)stack.Pop();
                    tempBeta = (float)stack.Pop();
                    tempAlpha = (float)stack.Pop();
                    tempDepth = (int)stack.Pop();
                    tempGame = (ContuGame)stack.Pop();
                    float tempValue = (float)stack.Pop();
                    moves = (IEnumerator<ContuActionData>)stack.Pop();

                    bool skip = false;

                    if (tempMaximizing)
                    {
                        tempAlpha = Mathf.Max(tempAlpha, tempValue);
                        if (tempAlpha >= tempBeta)
                        {
                            skip = true;
                        }
                    }
                    else
                    {
                        tempBeta = Mathf.Min(tempBeta, tempValue);
                        if (tempBeta <= tempAlpha)
                        {
                            skip = true;
                        }
                    }

                    if (moves.MoveNext() && !skip)
                    {
                        if (lastRes != null)
                        {
                            if (tempMaximizing ^ lastRes.Value < tempValue)
                            {
                                tempValue = lastRes.Value;
                            }      
                        }

                        ContuGame subGame = ContuGame.Clone(tempGame);
                        subGame.TryAction(moves.Current, false, false);
                        
                        stack.Push(moves);
                        stack.Push(tempValue);
                        stack.Push(6); //return adress
                        stack.Push(subGame);
                        stack.Push(tempDepth);
                        stack.Push(tempAlpha);
                        stack.Push(tempBeta);
                        stack.Push(tempMaximizing);
                        stack.Push(lastRes);
                        stack.Push(1); //call address;
                    }
                    else
                    {
                        int tempAddress = (int)stack.Pop();
                        stack.Push(tempGame);
                        stack.Push(tempDepth);
                        stack.Push(tempAlpha);
                        stack.Push(tempBeta);
                        stack.Push(tempMaximizing);
                        stack.Push(lastRes);
                        stack.Push(tempAddress);
                    }
                    break;

                case 10: //final return
                    GameEvalResult output = (GameEvalResult)stack.Pop();
                    return output;
            }
        }

        return null;
    }
}
