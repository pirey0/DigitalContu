using UnityEngine;

public class SpeedContuGame : ContuGame
{

    private float playerStartTime;
    private float moveIncrement;

    private float p1Time;
    private float p2Time;

    private float lastTimeStamp;
    public static SpeedContuGame NormalGame(float time, float increment)
    {
        SpeedContuGame game = new SpeedContuGame();
        game.board = ContuBoard.CreateDefault();
        game.state = TurnState.Player1;
        game.turnCount = 1;

        game.playerStartTime = time;
        game.moveIncrement = increment;

        game.p1Time = time;
        game.p2Time = time;

        return game;
    }

    protected override void PassTurn()
    {
        float timePassed = Time.time - lastTimeStamp;
        lastTimeStamp = Time.time;

        if(state == TurnState.Player1)
        {
            p1Time -= timePassed;
        }
        else
        {
            p2Time -= timePassed;
        }

        base.PassTurn();
    }

}
