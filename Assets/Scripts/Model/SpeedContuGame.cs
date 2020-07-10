using UnityEngine;

public class SpeedContuGame : ContuGame
{

    private float playerStartTime;
    private float moveIncrement;

    private float p1TimeLeft;
    private float p2TimeLeft;

    private float lastTimeStamp;
    public static SpeedContuGame NormalGame(float time, float increment)
    {
        SpeedContuGame game = new SpeedContuGame();
        game.board = ContuBoard.CreateDefault();
        game.state = TurnState.Player1;
        game.turnCount = 1;

        game.playerStartTime = time;
        game.moveIncrement = increment;

        game.p1TimeLeft = time;
        game.p2TimeLeft = time;

        return game;
    }

    public float GetTimeLeft(int playerId)
    {
        if(playerId == 0)
        {
            return p1TimeLeft - (TurnState == TurnState.Player1 ? (Time.time - lastTimeStamp) : 0);
        }
        else if(playerId == 1)
        {
            return p2TimeLeft - (TurnState == TurnState.Player2 ? (Time.time - lastTimeStamp) : 0);
        }

        return 0;
    }

    protected override void PassTurn()
    {
        float timePassed = Time.time - lastTimeStamp;
        lastTimeStamp = Time.time;

        if(turnCount > 1)
        {
            if (state == TurnState.Player1)
            {
                p1TimeLeft -= timePassed;
                p1TimeLeft += moveIncrement;
            }
            else
            {
                p2TimeLeft -= timePassed;
                p2TimeLeft += moveIncrement;
            }
        }

        base.PassTurn();
    }

}
