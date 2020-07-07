using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DebugView : MonoBehaviour
{
    [SerializeField] Transform owner;
    [SerializeField] ContuConnectionHandler contuConnection;
    [SerializeField] Text logText;

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

        logText.text += type.ToString() + ": " + condition + Environment.NewLine;

        if(logText.text.Length > 300)
        {
            logText.text = logText.text.Substring(logText.text.Length - 300);
        }
    }

    public void OnCommandSent(string content)
    {

        var input = content.Split(' ');

        if(input.Length < 2)
        {
            Debug.Log("Command: not enough paramenters");
            return;
        }


        var res = game.TryAction(contuConnection.Client.LocalPlayer.ActorNumber-1, (ActionType)int.Parse(input[0]), true, true, GetParams(input));
        Debug.Log("Command: " + res);
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
