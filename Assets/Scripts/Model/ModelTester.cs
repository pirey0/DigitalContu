using UnityEngine;
using NaughtyAttributes;

public interface IContuGameOwner
{
    ContuGame GetGame();
}
public class ModelTester : MonoBehaviour, IContuGameOwner
{
    [SerializeField] bool draw;
    [SerializeField] bool createEvaluator;
    [SerializeField] int evaluatorDepth;
    [SerializeField] float playerTime, increment;

    SpeedContuGame game;
    IGameEvaluator evaluator;

    private void Awake()
    {
        game = SpeedContuGame.NormalGame(playerTime, increment);

        if (createEvaluator)
        {
            evaluator = new SelectiveMinMaxer(2);
            evaluator.Setup(game, BoardEvaluators.DirectionProportionalEvaluation);
        }
    }

    [Button]
    private void LogPermutationsAtDepth()
    {
        Debug.Log("Permuations: " + evaluatorDepth + " -> " + evaluator.GetPermutations(evaluatorDepth));
    }

    [Button]
    private void LogSpeedTimeLeft()
    {
        Debug.Log("Clock: " + ((int)game.GetTimeLeft(0)) + " to " + ((int) game.GetTimeLeft(1)));
    }

    [Button]
    private void TestRandomTrashInput()
    {
        for (int i = 0; i < 10000; i++)
        {
            RandomAction(log: false);
        }
    }

    [Button]
    private void RandomNext()
    {
        ExecutionCheckResult result;
        do
        {
            result = RandomAction(log: true);
        } while (result != ExecutionCheckResult.Success);
    }

    [Button]
    private void EvaluateEvaluator()
    {
        if(evaluator != null)
        {
            Debug.Log("Evaluator Result: " + evaluator.Evaluate(evaluatorDepth, logSpeed: true).ToString());
        }
    }

    [Button]
    private void EvaluateBoardState()
    {
        Debug.Log("BoardEval: " + BoardEvaluators.DirectionProportionalEvaluation(game.Board));
        
    }

    [Button]
    private void PlayOutEvaluator()
    {
        var r = evaluator.Evaluate(evaluatorDepth);

        foreach (var action in r.Actions)
        {
            game.TryAction(action, false, false);
        }
    }

    private ExecutionCheckResult RandomAction(bool log)
    {
        return game.TryAction(Random.Range(0, 10), (ActionType)Random.Range(0, 6), log: log, networkCalled: false, GetRandomParams());
    }

    private int[] GetRandomParams()
    {
        int[] arr = new int[5];

        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = Random.Range(0, 10);
        }

    return arr;
    }

    private void OnGUI()
    {
        if(draw)
        GUI.Label(new Rect(200, 200, 300, 300), game.ToString());
    }

    public ContuGame GetGame()
    {
        return game;
    }
}
