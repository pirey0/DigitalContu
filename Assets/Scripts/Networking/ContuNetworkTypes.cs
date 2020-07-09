using Photon.Pun;
using UnityEngine;

public enum ContuEventCode
{
    GameAction = 100,
    Chat = 110
}

public struct ContuActionData
{
    public int UserId;
    public ActionType Action;
    public int[] Parameters;

    public ContuActionData(int userId, ActionType action, params int[] parameters)
    {
        this.UserId = userId;
        this.Action = action;
        this.Parameters = parameters;
    }


    public byte[] ToByteArray()
    {
        byte[] data = new byte[2 + Parameters.Length];
        data[0] = (byte)UserId;
        data[1] = (byte)Action;

        for (int i = 0; i < Parameters.Length; i++)
        {
            data[i + 2] = (byte)Parameters[i];
        }

        return data;
    }

    public static ContuActionData FromByteArray(byte[] data)
    {
        int userId = data[0];
        ActionType actionType = (ActionType)data[1];
        int[] parameters = new int[data.Length - 2];

        for (int i = 0; i < parameters.Length; i++)
        {
            parameters[i] = data[i + 2];
        }

        return new ContuActionData(userId, actionType, parameters);
    }


}
