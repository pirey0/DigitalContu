using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;



public class ContuMinMaxer : GameEvaluator
{
    protected override GameEvalResult InternalEvaluate(ContuGame game, int depth)
    {
        return MinMax(game, depth, game.TurnState == TurnState.Player1);
    }


    ///Testing indiactes that the current games Permutations follow aproximately this curve: y = 0.00026*x^12
    ///With a depth of 30 this would be aprox 138.174.660.000.000 board states to evaluate....
    ///                50                     63.476.562.500.000.000
    ///                10                     260.000.000
    ///This means that to run the algorithm at depth 10 in 1second we need a board eval to take: 1/260 000 000 = 3,84ns (s^-9)
    ///That would be 1 board eval in 16cpu cyces (4GHZ) ((Impossible)
    ///Current Speeds: depth 5 (57920 permutations) 1382ms -> 1 382 000 000ns for 57920 -> 23 860 ns/boardEval
    /// Current algorithm (Crude MinMax No-opt) is 6200 times slower then desired
    private GameEvalResult MinMax(ContuGame game, int depth, bool maximizingPlayer)
    {
        if (depth <= 0) // || node is leaf
        {
            var res = new GameEvalResult();
            res.Value = RunBoardEvaluator(game);
            return res;
        }

        if (maximizingPlayer)
        {
            float value = float.MinValue;
            GameEvalResult max = null;
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
            GameEvalResult min = null;
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

    public override int GetPermutations(int depth)
    {
        return GetPermutationsAtDepth(game, depth);
    }

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
}
