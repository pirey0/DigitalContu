using UnityEngine;
using NaughtyAttributes;
using System.Text;
using System.Collections;

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
    [SerializeField] bool saveMoves;


    SpeedContuGame game;
    GameEvaluator evaluator;

    private void Awake()
    {
        game = SpeedContuGame.NormalGame(playerTime, increment);

        if (createEvaluator)
        {
            evaluator = new AlphaBetaPruning();
            evaluator.Setup(game, BoardEvaluators.DirectionProportionalEvaluation, measureTime: true);
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
            Debug.Log("Evaluator Result: " + evaluator.Evaluate(evaluatorDepth).ToString());
        }
    }

    [Button]
    private void EvaluateBoardState()
    {
        Debug.Log("BoardEval: " + BoardEvaluators.DirectionProportionalEvaluation(game.Board));
        
    }

    [Button]
    private void PlayOutGame()
    {
        StartCoroutine(PlayGameCoroutine());
    }

    [Button]
    private void PlayGameAsBlack()
    {
        StartCoroutine(PlayBlackCoroutine());
    }

    private IEnumerator PlayBlackCoroutine()
    {
        while (true)
        {
            if(game.TurnState == TurnState.Player1)
            {
                yield return null;
            }
            else
            {
                yield return null;
                var r = evaluator.Evaluate(evaluatorDepth);
                Debug.Log("Evaluator Result: " + r.ToString());
                if (r.Actions.Count > 0)
                    game.TryAction(r.Actions[0], false, false);
                else
                    yield break;

                yield return null;
            }
        }
    }

    private IEnumerator PlayGameCoroutine()
    {
        while (true)
        {
            var r = evaluator.Evaluate(evaluatorDepth);
            Debug.Log("Evaluator Result: " + r.ToString());
            if (r.Actions.Count > 0)
                game.TryAction(r.Actions[0], false, false);
            else
                yield break;

            yield return new WaitForSeconds(0.1f);
        }
    }

    [Button]
    private void PlayOutSingleStep()
    {
        var r = evaluator.Evaluate(evaluatorDepth);
        Debug.Log("Evaluator Result: " + r.ToString());
        if(r.Actions.Count>0)
            game.TryAction(r.Actions[0], false, false);
    }

    [Button]
    private void LogPossibleMoves()
    {
        var moves = game.GetPossibleMoves();
        StringBuilder sb = new StringBuilder();
        sb.Append("Moves: ");
        
        while (moves.MoveNext())
        {
            sb.Append(moves.Current.ToString() + ", ");
        }
        Debug.Log(sb.ToString());
    }

    [Button]
    private void Run1MillionBoardEvals()
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        for (int i = 0; i < 1000000; i++)
        {
            evaluator.RunBoardEvaluator(game.Board);
        }
        stopwatch.Stop();

        Debug.Log("1Million Board Eval time: " + stopwatch.ElapsedMilliseconds + "ms");
    }

    [Button]
    private void Run1MillionCloning()
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        for (int i = 0; i < 1000000; i++)
        {
            var move = new ContuActionData(0, ActionType.Place, 0, 0);
            ContuGame subGame = ContuGame.Clone(game);
            subGame.TryAction(move, false, false);
        }
        stopwatch.Stop();

        Debug.Log("1 Million Cloning time: " + stopwatch.ElapsedMilliseconds + "ms");
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
