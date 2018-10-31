using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class GameStartedState : GameState
{
    public GameStartedState (NetworkWriter networkWriter, Dictionary<ulong, ClientConnection> playerDic, ConcurrentDictionary<ulong, ClientConnection> clientList)
        : base(networkWriter, playerDic, clientList)
    {
        actualState = State.GAME_STARTED;

        Console.WriteLine("GAME_STARTED");
    }

    public override void ProcessCommand (ClientConnection client, BaseNetData data)
    {
        lock (stateLock)
        {
            // TO DO process command

        }
    }

    public override bool ClientDisconnect (ClientConnection client)
    {
        lock (stateLock)
        {

            // TO DO remove player


        }

        return true;
    }
}
