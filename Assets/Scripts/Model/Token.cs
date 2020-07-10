using System;
using UnityEngine;

public class Token
{
    private readonly static int[,] STATECHANGE_VALIDITY = new int[,]
    {
        {0,1,1,0,0},
        {0,0,0,1,0},
        {0,0,0,0,1},
        {1,0,1,0,0},
        {1,1,0,0,0}
    };

    private TokenType type;
    private TokenState state;
    private int exhaustDuration;
    private int exhaustCount;

    public TokenType Type { get => type; }
    public TokenState State { get => state; }

    public Token(TokenType type, int exhaustDuration)
    {
        this.type = type;
        this.state = TokenState.Free;
        this.exhaustCount = 0;
        this.exhaustDuration = exhaustDuration;
    }

    public bool Tick()
    {
        if(exhaustCount > 0)
        {
            exhaustCount -= 1;

            if(exhaustCount == 0)
            {
                return TryChangeState(TokenState.Free);
            }
        }

        return false;
    }

    public bool TryChangeState(TokenState newState)
    {
        if (CanChangeStateTo(newState))
        {
            state = newState;

            if (state == TokenState.P1Exausted || state == TokenState.P2Exausted)
                exhaustCount = exhaustDuration;

            return true;
        }
        return false;
    }

    public bool BelongsTo(int userId)
    {
        switch (state)
        {
            case TokenState.P1Owned:
                if (userId == 0)
                    return true;
                break;

            case TokenState.P2Exausted:
                if (userId == 1)
                    return true;
                break;
        }
        return false;
    }

    public bool CanChangeStateTo(TokenState tokenState)
    {
        return STATECHANGE_VALIDITY[(int)state, (int)tokenState] > 0;
    }

    public override string ToString()
    {
        return type.ToString() + " (" + state.ToString() + ")" + (exhaustCount>0? "[" + exhaustCount.ToString() + "]" : "");
    }

    internal static Token Clone(Token token)
    {
        Token newTok = new Token(token.type, token.exhaustDuration);
        newTok.exhaustCount = token.exhaustCount;
        newTok.state = token.state;
        return newTok;
    }
}

public enum TokenState
{
    Free = 0,
    P1Owned = 1,
    P2Owned = 2,
    P1Exausted = 3,
    P2Exausted = 4
}

public enum TokenType
{
    Guard,
    Archer,
    Knight,
    Veteran
}
