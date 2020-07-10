using System;
using System.Collections.Generic;
using UnityEngine;

public class ContuGame 
{
    ContuBoard board;
    TurnState state;
    int turnCount;
    bool gameFinished;

    public event System.Action<BoardState> BoardStateChanged;
    public event System.Action TurnChanged;
    public event System.Action<ContuActionData> ActionExecuted;
    public event System.Action<ContuActionData> NetworkActionCall;


    public ContuBoard Board { get => board; }
    public TurnState TurnState { get => state; }
    public int Turn { get => turnCount; }

    public static ContuGame NormalGame()
    {
        ContuGame game = new ContuGame();
        game.board = ContuBoard.CreateDefault();
        game.state = TurnState.Player1;
        game.turnCount = 1;

        return game;
    }

    public static ContuGame Clone(ContuGame g1)
    {
        ContuGame g2 = new ContuGame();
        g2.board = ContuBoard.Clone(g1.board);
        g2.state = g1.state;
        g2.turnCount = g1.turnCount;
        g2.gameFinished = g1.gameFinished;

        return g2;
    }

    public ExecutionCheckResult TryAction(ContuActionData data, bool log, bool networkCalled)
    {
        return TryAction(data.UserId, data.Action, log, networkCalled, data.Parameters);
    }

    public ExecutionCheckResult TryAction(int userId, ActionType actionType, bool log, bool networkCalled, params int[] parameters)
    {
        var validResult = ActionIsValid(userId, actionType, parameters);

        if (validResult != ExecutionCheckResult.Success)
        {
            //if(log)
            //Debug.Log("Invalid Action: " + actionType.ToString());
            return validResult;
        }

        switch (actionType)
        {
            case ActionType.Place:
                Place(userId, parameters[0], parameters[1]);
                break;

            case ActionType.TakeToken:
                TakeToken(userId, (TokenType)parameters[0]);
                break;

            case ActionType.UseToken:
                TokenEffects.UseToken(board, userId, parameters);
                var exaused = board.GetFirstTokenOfType((TokenType)parameters[0]).TryChangeState(userId == 0 ? TokenState.P1Exausted : TokenState.P2Exausted);
                if (!exaused)
                    Debug.LogError("Could not exaust token");
                break;
        }

        if (log)
            Debug.Log("Action: " + userId + ") " + actionType.ToString());

        if(!networkCalled)
            NetworkActionCall?.Invoke(new ContuActionData(userId, actionType, parameters));

        ActionExecuted?.Invoke(new ContuActionData(userId, actionType, parameters));

        PassTurn();
        return ExecutionCheckResult.Success;
    }

    public ExecutionCheckResult ActionIsValid(int userId, ActionType action, params int[] parameters)
    {
        if (gameFinished)
            return ExecutionCheckResult.GameEnded;

        if((int)state != userId)
            return ExecutionCheckResult.NotYourTurn;

        switch (action)
        {
            case ActionType.Place:
                if (parameters.Length < 2)
                    return ExecutionCheckResult.BadParameters;

                return board.CanPlaceTile(parameters[0], parameters[1], userId) ? ExecutionCheckResult.Success : ExecutionCheckResult.CannotPlaceThere;

            case ActionType.TakeToken:
                if (parameters.Length < 1)
                    return ExecutionCheckResult.BadParameters;

                if (board.GetTokenCountForUser(userId) > 1)
                    return ExecutionCheckResult.BadParameters;

                var token = board.GetFirstTokenOfType((TokenType)parameters[0]);

                if (token == null)
                    return ExecutionCheckResult.BadParameters;

                return token.CanChangeStateTo(userId == 0 ? TokenState.P1Owned : TokenState.P2Owned) ? ExecutionCheckResult.Success : ExecutionCheckResult.UnusableToken;

            case ActionType.UseToken:
                if (parameters.Length < 1)
                    return ExecutionCheckResult.BadParameters;

                token = board.GetFirstTokenOfType((TokenType)parameters[0]);

                if (token != null && ((token.State == TokenState.P1Owned && userId == 0) || (token.State == TokenState.P2Owned && userId == 1)))
                {
                    return TokenEffects.CanUseToken(board, userId, parameters) ? ExecutionCheckResult.Success : ExecutionCheckResult.UnusableToken;
                }
                else
                {
                    return ExecutionCheckResult.BadParameters;
                }
        }

        return ExecutionCheckResult.BadParameters;
    }

    private void Place(int userId, int x, int y)
    {
        board.SetTile(x, y, userId == 0 ? TileType.Player1 : TileType.Player2);
        board.CheckRuleOf5(); //rule of 5
    }

    private void TakeToken(int userId, TokenType type)
    {
        var token = board.GetFirstTokenOfType(type);
        token.TryChangeState(userId==0 ? TokenState.P1Owned : TokenState.P2Owned);
    }

    private void PassTurn()
    {
        if (state == TurnState.Player1)
        {
            state = TurnState.Player2;
        }
        else
        {
            state = TurnState.Player1;
            turnCount++;
            TickTokens();
        }


        var boardState = board.GetBoardState();
        if (boardState != BoardState.Playing)
        {
            gameFinished = true;
            BoardStateChanged?.Invoke(boardState);
        }
        else
        {
            TurnChanged?.Invoke();
        }

    }

    public IEnumerator<ContuActionData> GetPossibleMoves()
    {
        ContuActionData attempt;

        for (int i = 0; i < board.TokenCount; i++)
        {
            attempt = new ContuActionData((int)TurnState, ActionType.TakeToken, i);
            if(ActionIsValid(attempt.UserId, attempt.Action, attempt.Parameters) == ExecutionCheckResult.Success)
            {
                yield return attempt;
            }

            if (board.GetToken(i).BelongsTo((int)TurnState))
            {
                for (int y1 = 0; y1 < board.Height; y1++)
                {
                    for (int x1 = 0; x1 < board.Width; x1++)
                    {
                        for (int y2 = 0; y2 < board.Height; y2++)
                        {
                            for (int x2 = 0; x2 < board.Width; x2++)
                            {
                                attempt = new ContuActionData((int)TurnState, ActionType.UseToken, i, x1, y1, x2, y2);
                                if (ActionIsValid(attempt.UserId, attempt.Action, attempt.Parameters) == ExecutionCheckResult.Success)
                                {
                                    yield return attempt;
                                }
                            }
                        }
                    }
                }
            }
        }

        for (int y = 0; y < board.Height; y++)
        {
            for (int x = 0; x < board.Width; x++)
            {
                attempt = new ContuActionData((int)TurnState, ActionType.Place, x, y);
                if (ActionIsValid(attempt.UserId, attempt.Action, attempt.Parameters) == ExecutionCheckResult.Success)
                {
                    yield return attempt;
                }
            }
        }

    }

    private void TickTokens()
    {
        board.TickTokens();
    }

    public override string ToString()
    {
        return "Game " + board.GetBoardState().ToString() + " Turn " + turnCount + " " + state + Environment.NewLine + board.ToString();
    }

}

public enum ActionType
{
    Place,
    TakeToken,
    UseToken
}

public enum TurnState
{
    Player1,
    Player2
}

public enum ExecutionCheckResult
{
    Success,
    GameEnded,
    NotYourTurn,
    BadParameters,
    UnusableToken,
    CannotPlaceThere
}