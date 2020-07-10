using UnityEngine;
using NaughtyAttributes;

public interface IContuGameOwner
{
    ContuGame GetGame();
}
public class ModelTester : MonoBehaviour, IContuGameOwner
{
    [SerializeField] bool draw;
    [SerializeField] bool testMinMax;
    [SerializeField] int minMaxDepth;

    ContuGame game;
    ContuMinMaxer minMaxer;

    private void Awake()
    {
        game = ContuGame.NormalGame();

        if (testMinMax)
            minMaxer = new ContuMinMaxer(game, minMaxDepth);
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
    private void EvaluateMinMax()
    {
        if(minMaxer != null)
        {
            Debug.Log("MinMax Result: " + minMaxer.Evaluate(minMaxDepth).ToString());
        }
    }

    [Button]
    private void PlayOutMinMax()
    {
        var r = minMaxer.Evaluate(minMaxDepth);

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
