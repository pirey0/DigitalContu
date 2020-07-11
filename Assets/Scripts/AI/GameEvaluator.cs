using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
public interface IGameEvaluator
{
    GameEvalResult Evaluate(int depth, bool logSpeed = false);
    int GetPermutations(int depth);

    void Setup(ContuGame game, System.Func<ContuBoard, float> func = null);
}

public abstract class GameEvaluator : IGameEvaluator
{
    protected ContuGame game;
    protected System.Func<ContuBoard, float> boardEvaluator;

    public void Setup(ContuGame game, Func<ContuBoard, float> func = null)
    {
        this.game = game;
        if(func == null)
            boardEvaluator = BoardEvaluators.NaiveEvaluate;
        else
            boardEvaluator = func;
    }

    public GameEvalResult Evaluate(int customDepth, bool logSpeed = false)
    {
        System.Diagnostics.Stopwatch stopwatch = null;
        if (logSpeed)
        {
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
        }

        var res = InternalEvaluate(game, customDepth);
        res.Actions.Reverse();

        if (logSpeed)
        {
            stopwatch.Stop();
            Debug.Log("Evaluate took " + stopwatch.ElapsedMilliseconds.ToString() + "ms ");
        }

        return res;
    }

    protected abstract GameEvalResult InternalEvaluate(ContuGame game, int depth);

    public virtual int GetPermutations(int depth)
    {
        throw new System.NotImplementedException();
    }

}

public class GameEvalResult
{
    public float Value;
    public List<ContuActionData> Actions;

    public GameEvalResult()
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