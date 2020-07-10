using ExitGames.Client.Photon;
using Photon.Realtime;
using System;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class ContuNetworkEventHandler : MonoBehaviour, IOnEventCallback
{
    [SerializeField] ChatSystem chatSystem;
    [SerializeField] Transform gameHolder;

    [SerializeField] bool sendSuccesfulActions;
    [SerializeField] bool log;

    ContuGame game;

    public int LocalPlayerId { get => ContuConnectionHandler.Instance.Client.LocalPlayer.ActorNumber - 1; }
    private void Start()
    {
        if(ContuConnectionHandler.Instance == null)
        {
            Debug.Log("No ContuConnectionHandler, disabling networking");
            Destroy(gameObject);
            return;
        }

        ContuConnectionHandler.Instance.Client.AddCallbackTarget(this);

        game = gameHolder.GetComponent<IContuGameOwner>().GetGame();
        if(game != null)
        {
            game.NetworkActionCall += OnActionExecuted;
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

        ContuConnectionHandler.Instance.Client.OpRaiseEvent(eventCode, customEventContent, RaiseEventOptions.Default, SendOptions.SendReliable);
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
                game.TryAction(data, true, networkCalled: true);
                break;

            case (byte)ContuEventCode.Chat:
                if(chatSystem!= null)
                    chatSystem.Print(photonEvent.Sender + ": " + (string)photonEvent.CustomData);
                break;

            case (byte)ContuEventCode.ChatSoundMessage:
                if(chatSystem!= null)
                    chatSystem.PlaySound((int)photonEvent.CustomData);
                break;
        }

        //if(photonEvent.Code < 200)
           //Debug.Log("Event: " + photonEvent.ToStringFull());
    }
}
