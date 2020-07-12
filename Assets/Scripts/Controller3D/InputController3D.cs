using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(200)]
public class InputController3D : MonoBehaviour
{
    [SerializeField] GameObject gameHolder;
    [SerializeField] Camera camera;
    [SerializeField] ContuNetworkEventHandler eventHandler;
    [SerializeField] ContuConnectionHandler connectionHandler;

    [SerializeField] Text stateText, turnText, interactionStateText, idText;
    [SerializeField] Image currentTurnPlayerImage;

    [SerializeField] Transform[] freeLocations, p1Locations, p2Locations, p1ExLocations, p2ExLocations;

    [SerializeField] Transform tileHighlighter;

    ContuGame game;
    VisualToken[] visualTokens;

    InteractionState interactionState;

    VisualToken tokenSelected;
    Vector2Int tokenUse1, tokenUse2;

    private void Start()
    {
        game = gameHolder.GetComponent<IContuGameOwner>().GetGame();
        interactionState = InteractionState.Selecting;

        if(interactionStateText)
        interactionStateText.text = interactionState.ToString();


        if(connectionHandler)
        connectionHandler.RoomJoined += OnRoomJoined;

        game.TurnChanged += OnTurnChanged;
    }

    private void OnGameStateChanged(BoardState obj)
    {
        stateText.text = "State: " + game.Board.GetBoardState();
    }

    private void OnRoomJoined()
    {
        idText.text = "You are Player " + (GetPlayerId()+1);
    }

    private void OnTurnChanged()
    {

        interactionState = InteractionState.Selecting;
        if(interactionStateText != null)
            interactionStateText.text = interactionState.ToString();

        if(turnText!=null)
        turnText.text = "Turn: " + game.Turn + " " + game.TurnState;

        if (currentTurnPlayerImage != null)
            currentTurnPlayerImage.color = game.TurnState == TurnState.Player1 ? Color.red : Color.blue;

    }

    private void Update()
    {
        //do the raycast
        int layerMask = LayerMask.GetMask("Ground");
        Camera mainCam = Camera.main;
        RaycastHit hit;
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.yellow, 5);

            if (hit.transform != null && tileHighlighter != null)
            {
                tileHighlighter.transform.position = Round(hit.point);

                if (Input.GetMouseButtonDown(0))
                {
                    int x = Mathf.RoundToInt(hit.point.x);
                    int y = Mathf.RoundToInt(hit.point.z);

                    if (game.Board.GetTile(x, y) != TileType.Null)
                    {
                        PlayerClicked(x, y);
                    }
                }
            }
        }
    }

    private Vector3 Round(Vector3 input)
    {
        return new Vector3(Mathf.RoundToInt(input.x), Mathf.RoundToInt(input.y), Mathf.RoundToInt(input.z));
    }

    private void UpdateTokenView()
    {
        Dictionary<TokenState, int> indexes = new Dictionary<TokenState, int>();
        indexes.Add(TokenState.Free, 0);
        indexes.Add(TokenState.P1Exausted, 0);
        indexes.Add(TokenState.P2Exausted, 0);
        indexes.Add(TokenState.P1Owned, 0);
        indexes.Add(TokenState.P2Owned, 0);

        foreach (var token in visualTokens)
        {
            var state = token.Source.State;
            token.transform.position = GetTokenLocation(state, indexes[state]);
            indexes[state] += 1;
        }
    }

    private Vector3 GetTokenLocation(TokenState state, int index)
    {
        switch (state)
        {
            case TokenState.Free:
                return freeLocations[index].position;
            case TokenState.P1Exausted:
                return p1ExLocations[index].position;
            case TokenState.P2Exausted:
                return p2ExLocations[index].position;
            case TokenState.P1Owned:
                return p1Locations[index].position;
            case TokenState.P2Owned:
                return p2Locations[index].position;
        }

        return Vector3.zero;
    }

    private void PlayerClicked(int x, int y)
    {
        switch (interactionState)
        {
            case InteractionState.Selecting:
                PlacePiece(x, y);
                break;

            case InteractionState.UsingTokenTarget1:
                tokenUse1 = new Vector2Int(x, y);
                interactionState = InteractionState.UsingTokenTarget2;
                //interactionStateText.text = interactionState.ToString();
                break;

            case InteractionState.UsingTokenTarget2:
                tokenUse2 = new Vector2Int(x, y);
                UseToken();
                break;
        }
    }

    private void InteractWithToken(VisualToken token)
    {
        if(GetPlayerId() != (int)game.TurnState)
        {
            Debug.Log("Not your turn");
            return;
        }

        switch (token.Source.State)
        {
            case TokenState.Free:
                TakeToken(token.Source.Type);
                break;

            case TokenState.P1Owned:
                if(GetPlayerId() == 0)
                    StartTokenUsage(token);
                break;

            case TokenState.P2Owned:
                if (GetPlayerId() == 1)
                    StartTokenUsage(token);
                break;

            case TokenState.P1Exausted:
                if (GetPlayerId() == 1)
                    TakeToken(token.Source.Type);
                break;
            case TokenState.P2Exausted:
                if (GetPlayerId() == 0)
                    TakeToken(token.Source.Type);
                break;
        }
    }

    private void StartTokenUsage(VisualToken token)
    {
        tokenSelected = token;
        interactionState = InteractionState.UsingTokenTarget1;
        //interactionStateText.text = interactionState.ToString();
        Debug.Log("Started using Token");
    }

    private void TakeToken(TokenType type)
    {
        var res = game.TryAction(GetPlayerId(), ActionType.TakeToken, true, false, (int)type);
        Debug.Log("Tried Taking a Token: " + res);
    }

    private void UseToken()
    {
        var res = game.TryAction(GetPlayerId(), ActionType.UseToken, true, false, (int)tokenSelected.Source.Type, tokenUse1.x, tokenUse1.y, tokenUse2.x, tokenUse2.y);
        ResetSelection();
        Debug.Log("Tried using Token: " +res);
    }

    private void ResetSelection()
    {
        interactionState = InteractionState.Selecting;
        interactionStateText.text = interactionState.ToString();
    }

    private void PlacePiece(int x, int y)
    {
        var res = game.TryAction(GetPlayerId(), ActionType.Place, true, false, x, y);
        Debug.Log("Tried Placing: " + res);
    }

    private int GetPlayerId()
    {
        if (ContuConnectionHandler.Instance != null) 
        {
        return ContuConnectionHandler.Instance.Client.LocalPlayer.ActorNumber - 1;
        }
        else
        {
            return ((int)game.TurnState);
        }
    }

    private void SpawnTokens()
    {
        /*
        visualTokens = new VisualToken[game.Board.TokenCount];

        for (int i = 0; i < game.Board.TokenCount; i++)
        {
            var t = game.Board.GetToken(i);
            visualTokens[i] = Instantiate(tokenPrefab).GetComponent<VisualToken>();
            visualTokens[i].Setup(t, InteractWithToken);
        }
        */
    }

    public enum InteractionState
    {
        Selecting,
        UsingTokenTarget1,
        UsingTokenTarget2
    }

}