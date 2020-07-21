using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class GameEvaluator
{
    protected ContuGame game;
    private System.Func<ContuBoard, float> boardEvaluator;
    protected Stopwatch boardEvalStopWatch, cloneAndMoveStopWatch;
    protected bool measureTime;
    protected int boardsEvaluated;

    protected StateHashTable stateTable;

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

        stateTable = new StateHashTable();
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
            stateTable.ResetCount();
        }

        var res = InternalEvaluate(game, customDepth);
        

        if (measureTime)
        {
            stopwatch.Stop();
            long tot = stopwatch.ElapsedMilliseconds;
            LogStopWatch("Total", stopwatch, tot);
            LogStopWatch("BoardEvals", boardEvalStopWatch, tot);
            LogStopWatch("CloneAndMoveEval", cloneAndMoveStopWatch, tot);
            UnityEngine.Debug.Log("Board Evaluation Count: " + boardsEvaluated);
            UnityEngine.Debug.Log("Used State Table Count: " + stateTable.Count);
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
    public float RunBoardEvaluator(ContuGame locGame)
    {
        if (measureTime)
        {
            boardEvalStopWatch.Start();
            boardsEvaluated++;
        }

        float res = 0;
        var tabRes = stateTable.Get(locGame);

        if (tabRes != null)
        {
            res = tabRes.Value.Value;
        }
        else
        {
            res = boardEvaluator.Invoke(locGame.Board);
            stateTable.TryAdd(locGame, 0, new GameEvalResult(res));
        }

        if (measureTime)
            boardEvalStopWatch.Stop();

        return res;
    }

    public ContuGame CloneAndMove(ContuGame gameToClone, ContuActionData data)
    {
        if (measureTime)
            cloneAndMoveStopWatch.Start();

        var newG = ContuGame.Clone(gameToClone);
        var res = newG.TryAction(data, false, false);
        if (res != ExecutionCheckResult.Success)
        {
            UnityEngine.Debug.LogWarning("AI trying illegal move: " + res + " " + data.ToString());
            return null;
        }

        if (measureTime)
            cloneAndMoveStopWatch.Stop();
        return newG;
    }
}

public struct GameEvalResult
{
    public float Value;
    public string ActionSequence;

    public bool HasAction { get { return ActionSequence != null && ActionSequence.Length > 0; } }

    public ContuActionData GetAction()
    {
        if (!HasAction)
            return default;

        return ContuActionData.FromByteArray(StringToByteArray(ActionSequence));
    }

    private static byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();
    }



    public void AddAction(ContuActionData action)
    {
        ActionSequence = action.ToByteString() + ActionSequence;
    }

    public GameEvalResult(float value)
    {
        Value = value;
        ActionSequence = "";
    }

    public override string ToString()
    {
        return ("Eval: " + Value + " -> " + ActionSequence);
    }
}

public class StateTableData
{
    public int Depth;
    public GameEvalResult Eval;

    public StateTableData(int depth, GameEvalResult eval)
    {
        Depth = depth;
        Eval = eval;
    }
}

public class StateHashTable
{
    private Dictionary<string, StateTableData>  table;
    int useCount;

    public int Count { get => useCount; }

    public StateHashTable()
    {
        table = new Dictionary<string, StateTableData>();
        useCount = 0;

    }

    public void ResetCount()
    {
        useCount = 0;
    }

    public GameEvalResult? Get(ContuGame game, int minDepth =0)
    {
        string str = game.NormalAsString();

        if (table.ContainsKey(str))
        {
            var res = table[str];

            if (res.Depth >= minDepth)
            {
                useCount++;
                return res.Eval;
            }

            return null;
        }
        else
        {
            return null;
        }
    }

    public void TryAdd(ContuGame game, int depth, GameEvalResult eval)
    {
        string str = game.NormalAsString();

        if (table.ContainsKey(str))
        {
            var res = table[str];

            if (res.Depth < depth)
                table[str] = new StateTableData(depth, eval);
        }
        else
        {
            table.Add(str, new StateTableData(depth, eval));
        }

    }

}