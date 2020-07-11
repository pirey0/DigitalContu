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
            float time = p1TimeLeft;

            if(TurnState == TurnState.Player1 && turnCount>1)
            {
                 time -= Time.time - lastTimeStamp;
            }

            return time;
        }
        else if(playerId == 1)
        {
            float time = p2TimeLeft;

            if (TurnState == TurnState.Player2 && turnCount > 1)
            {
                time -= Time.time - lastTimeStamp;
            }

            return time;
        }

        return 0;
    }

    public void UpdateTimes()
    {
        float t = GetTimeLeft((int)TurnState);

        if(t < 0)
        {
            FinishGame(TurnState == TurnState.Player1 ? BoardState.P2Won : BoardState.P1Won);
        }
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
