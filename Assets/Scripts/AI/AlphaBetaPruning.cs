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
    private GameEvalResult AlphaBeta_Rec(ContuGame game, int depth, float alpha, float beta, bool maximizingPlayer)
    {
        //0
        if (depth <= 0) // || node is leaf
        {
            return new GameEvalResult(RunBoardEvaluator(game));
        }

        var lookup = stateTable.Get(game, depth);
        if (lookup != null)
            return lookup.Value;
        

        float value = maximizingPlayer ? float.MinValue : float.MaxValue;
        GameEvalResult localRes = default;
        ContuActionData? action = null;

        var moves = game.GetPossibleMoves();

        //5
        while (moves.MoveNext())
        {
            ContuGame subGame = CloneAndMove(game, moves.Current);
            var heur = AlphaBeta_Rec(subGame, depth - 1, alpha, beta, !maximizingPlayer);

            //1
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

        //2
        if (action == null)
        {
            var res = new GameEvalResult();
            res.Value = RunBoardEvaluator(game);
            res.Value += res.Value > 0 ? -depth * 5 : depth * 5;
            stateTable.TryAdd(game, depth, res);
            return res;
        }
        else
        {
            localRes.AddAction(action.Value);
            stateTable.TryAdd(game, depth, localRes);
            return localRes;
        }
    }

    //https://stackoverflow.com/questions/40027002/how-to-convert-recursion-to-iteration
    private GameEvalResult AlphaBeta_Iter(ContuGame in_game, int in_depth)
    {
        Stack<IterationData> stack = new Stack<IterationData>();
        GameEvalResult returnValue = default;

        IterationData current = new IterationData();
        current.alpha = float.NegativeInfinity;
        current.beta = float.PositiveInfinity;
        current.maximizingPlayer = game.TurnState == TurnState.Player1;
        current.game = in_game;
        current.depth = in_depth;

        int address = 0;
        bool inFirst = true;

        while (stack.Count > 0 || inFirst)
        {
            switch (address)
            {
                case 0: //start

                    if (current.depth <= 0)
                    {
                        returnValue = new GameEvalResult(RunBoardEvaluator(current.game));
                        address = 1; //jump to return;
                        break;
                    }

                    current.value = current.maximizingPlayer ? float.MinValue : float.MaxValue;
                    current.localRes = default(GameEvalResult);
                    current.action = null;
                    current.moves = game.GetPossibleMoves();

                    stack.Push(current);
                    address = 5; //jump to loopIter
                    inFirst = false;
                    break;


                case 5: //loopIter

                    current = stack.Pop();

                    if (current.moves.MoveNext())
                    {
                        var subGame = CloneAndMove(current.game, current.moves.Current);

                        if(subGame == null) //Should not happen
                        {
                            Debug.Log(current.ToString());
                            return default(GameEvalResult);
                        }

                        //push to stack
                        stack.Push(current);

                        //reset parameters
                        current.game = subGame;
                        current.depth = current.depth - 1;
                        current.maximizingPlayer = !current.maximizingPlayer;
                        current.moves = null;

                        address = 0; //jump to start
                    }
                    else
                    {
                        stack.Push(current);
                        address = 2; //jump to else
                    }
                    break;

                case 1: // return

                    current = stack.Pop();
                    current.heur = returnValue;

                    if (current.maximizingPlayer ^ current.heur.Value < current.value)
                    {
                        current.value = current.heur.Value;
                        current.localRes = current.heur;

                        if(current.moves == null)
                        {
                            //?? no clue how i get here
                        }
                        else
                        {
                            current.action = current.moves.Current;
                        }
                    }

                    if (current.maximizingPlayer)
                    {
                        current.alpha = Mathf.Max(current.alpha, current.value);
                        if (current.alpha >= current.beta)
                        {
                            address = 2;
                            break;
                        }
                    }
                    else
                    {
                        current.beta = Mathf.Min(current.beta, current.value);
                        if (current.beta <= current.alpha)
                        {
                            address = 2;
                            break;
                        }
                    }

                    stack.Push(current);
                    address = 5; //jump to loop iter
                    break;

                case 2: //else
                    if (current.action == null) 
                    {
                        Debug.Log("Out of moves");
                        float val = RunBoardEvaluator(game);
                        returnValue = new GameEvalResult(val + val > 0 ? -current.depth * 5 : current.depth * 5);
                        address = 1; //jump to return;
                    }
                    else
                    {
                        current.localRes.AddAction(current.action.Value);
                        returnValue = current.localRes;
                        address = 1; //jump to return;
                    }
                    break;
            }
        }

        return returnValue;
    }

    struct IterationData
    {
        public ContuGame game;
        public float value;
        public GameEvalResult localRes;
        public ContuActionData? action;
        public GameEvalResult heur;
        public float alpha;
        public float beta;
        public bool maximizingPlayer;
        public IEnumerator<ContuActionData> moves;
        public int depth;

        public override string ToString()
        {
            return  depth + "_" + maximizingPlayer + "_" + ((localRes.HasAction)? localRes.ToString() : "");
        }
    }


}
