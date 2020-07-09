using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChatSystem : MonoBehaviour
{
    [SerializeField] Transform owner;
    [SerializeField] ContuNetworkEventHandler network;
    [SerializeField] Text logText;
    [SerializeField] AudioClip[] clips;

    ContuGame game;
    private void Start()
    {
        game = owner.GetComponent<IContuGameOwner>().GetGame();
        if(game == null)
        {
            Debug.LogError("No Game referenced");
            Destroy(this);
        }
        else
        {
            Application.logMessageReceived += OnLog;
        }
    }

    private void OnLog(string condition, string stackTrace, LogType type)
    {
        if (logText == null)
            return;

        Print(type.ToString() + ": " + condition);
    }

    public void Print(string msg)
    {
        logText.text += msg + Environment.NewLine;

        if (logText.text.Length > 300)
        {
            logText.text = logText.text.Substring(logText.text.Length - 300);
        }
    }

    public bool PlaySound(int index)
    {
        if (index < 0 || index >= clips.Length)
            return false;

        AudioSource.PlayClipAtPoint(clips[index], Vector3.zero);
        return true;
    }

    public void OnCommandSent(string content)
    {
        if(content.StartsWith("/do "))
        {
            content = content.Substring(4);
            var input = content.Split(' ');

            if (input.Length < 2)
            {
                Debug.Log("Command: not enough paramenters");
                return;
            }

            var res = game.TryAction(network.LocalPlayerId, (ActionType)int.Parse(input[0]), true, false, GetParams(input));
            Debug.Log("Command: " + res);
        }
        else if( content.StartsWith("/say "))
        {
            content = content.Substring(5);
            int res;

            if(int.TryParse(content, out res))
            {

                if(PlaySound(res))
                {
                    network.RaiseEvent((int)ContuEventCode.ChatSoundMessage, res);
                    Print("Said: " + res);
                }
            }
        }
        else
        {
            Print("Me: " + content);
            network.RaiseEvent((byte)ContuEventCode.Chat, content);
        }

    }

    private int[] GetParams(string[] input)
    {
        int[] res = new int[input.Length - 1];

        for (int i = 0; i < res.Length; i++)
        {
            res[i] = int.Parse(input[i + 1]);
        }
        return res;
    }

}
