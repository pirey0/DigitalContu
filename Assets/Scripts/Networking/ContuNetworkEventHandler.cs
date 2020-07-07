using ExitGames.Client.Photon;
using Photon.Realtime;
using System;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class ContuNetworkEventHandler : MonoBehaviour, IOnEventCallback
{
    [SerializeField] ContuConnectionHandler connectionHandler;
    [SerializeField] Transform gameHolder;

    [SerializeField] bool sendSuccesfulActions;
    [SerializeField] bool log;

    LoadBalancingClient client;
    ContuGame game;
    private void Start()
    {
        client = connectionHandler.Client;

        client.AddCallbackTarget(this);

        game = gameHolder.GetComponent<IContuGameOwner>().GetGame();
        if(game != null)
        {
            game.ActionExecuted += OnActionExecuted;
        }

    }

    private void OnActionExecuted(ContuActionData data)
    {
        if (sendSuccesfulActions)
        {
            RaiseContuActionEvent(data);
        }
    }

    public void RaiseEvent(byte eventCode, object customEventContent)
    {
        if (log)
        {
            Debug.Log(eventCode + " " + customEventContent.ToString());
        }

        client.OpRaiseEvent(eventCode, customEventContent, RaiseEventOptions.Default, SendOptions.SendReliable);
    }

    public void RaiseContuActionEvent(ContuActionData actionData)
    {
        RaiseEvent((byte)ContuEventCode.GameAction, actionData.ToByteArray());
    }


    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case (byte)ContuEventCode.GameAction:
                var data = ContuActionData.FromByteArray((byte[])photonEvent.CustomData);
                //Debug.Log("Contu Game Action received: " + data);
                game.TryAction(data, true, false);
                break;
        }

        //if(photonEvent.Code < 200)
           //Debug.Log("Event: " + photonEvent.ToStringFull());
    }
}
