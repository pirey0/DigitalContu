using UnityEngine;
using NaughtyAttributes;

public interface IContuGameOwner
{
    ContuGame GetGame();
}
public class ModelTester : MonoBehaviour, IContuGameOwner
{
    ContuGame game;

    private void Awake()
    {
        game = ContuGame.NormalGame();
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

    private ExecutionCheckResult RandomAction(bool log)
    {
        return game.TryAction(Random.Range(0, 10), (ActionType)Random.Range(0, 6), log: log, userCaused: true, GetRandomParams());
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
        GUI.Label(new Rect(200, 200, 300, 300), game.ToString());
    }

    public ContuGame GetGame()
    {
        return game;
    }
}
