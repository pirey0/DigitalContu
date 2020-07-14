using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public abstract class GameEvaluator
{
    protected ContuGame game;
    private System.Func<ContuBoard, float> boardEvaluator;
    protected Stopwatch boardEvalStopWatch, cloneAndMoveStopWatch;
    protected bool measureTime;
    protected int boardsEvaluated;
    public void Setup(ContuGame game, Func<ContuBoard, float> func = null, bool measureTime = false)
    {
        this.game = game;
        if(func == null)
            boardEvaluator = BoardEvaluators.NaiveEvaluate;
        else
            boardEvaluator = func;

        this.measureTime = measureTime;
        if (measureTime)
        {
            boardEvalStopWatch = new Stopwatch();
            cloneAndMoveStopWatch = new Stopwatch();
        }
    }

    public GameEvalResult Evaluate(int customDepth)
    {
        System.Diagnostics.Stopwatch stopwatch = null;
        if (measureTime)
        {
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            boardEvalStopWatch.Reset();
            cloneAndMoveStopWatch.Reset();
            boardsEvaluated = 0;
        }

        var res = InternalEvaluate(game, customDepth);
        res.Actions.Reverse();

        if (measureTime)
        {
            stopwatch.Stop();
            long tot = stopwatch.ElapsedMilliseconds;
            LogStopWatch("Total", stopwatch, tot);
            LogStopWatch("BoardEvals", boardEvalStopWatch, tot);
            LogStopWatch("CloneAndMoveEval", cloneAndMoveStopWatch, tot);
            UnityEngine.Debug.Log("Board Evaluation Count: " + boardsEvaluated);
        }

        return res;
    }

    private void LogStopWatch(string name, Stopwatch sw, long total)
    {
        UnityEngine.Debug.Log(name + " took " + sw.ElapsedMilliseconds + "ms " + (int)(((double)sw.ElapsedMilliseconds / total) * 100) + "%");
    }

    protected abstract GameEvalResult InternalEvaluate(ContuGame game, int depth);

    public virtual int GetPermutations(int depth)
    {
        throw new System.NotImplementedException();
    }
    public float RunBoardEvaluator(ContuBoard board)
    {
        if (measureTime)
        {
            boardEvalStopWatch.Start();
            boardsEvaluated++;
        }

        float res = boardEvaluator.Invoke(board);

        if (measureTime)
            boardEvalStopWatch.Stop();

        return res;
    }

    public ContuGame CloneAndMove(ContuGame gameToClone, ContuActionData data)
    {
        if (measureTime)
            cloneAndMoveStopWatch.Start();

        var newG = ContuGame.Clone(gameToClone);
        if (newG.TryAction(data, false, false) != ExecutionCheckResult.Success)
            UnityEngine.Debug.LogError("AI trying illegal move:" + data.ToString());

        if (measureTime)
            cloneAndMoveStopWatch.Stop();
        return newG;
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

    public GameEvalResult(float value)
    {
        Value = value;
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