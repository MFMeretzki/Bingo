using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

public abstract class GameState
{
    public enum State
    {
        WAIT_NEW_GAME,
        GAME_STARTED
    }

    public State actualState { get; protected set; }

    protected NetworkWriter netWriter;
    protected Dictionary<ulong, ClientConnection> players;
    protected ConcurrentDictionary<ulong, ClientConnection> clientList;
    protected object stateLock = new object();

    public GameState (NetworkWriter networkWriter, Dictionary<ulong,ClientConnection> playerDic, ConcurrentDictionary<ulong, ClientConnection> clientList)
    {
        netWriter = networkWriter;
        players = playerDic;
        this.clientList = clientList;
    }


    /// <summary>
    /// Fired when the game state changes
    /// </summary>
    public event EventHandler<State> ChangeState;


    /// <summary>
    /// Process the command recived from the client.
    /// </summary>
    /// <param name="client">The connection of the client that sent the command</param>
    /// <param name="data">The related data to the command</param>
    public abstract void ProcessCommand (ClientConnection client, BaseNetData data);

    /// <summary>
    /// Do the proper action when a player leaves the game.
    /// </summary>
    /// <param name="client">The disconnected player connection</param>
    /// <returns><code>true</code> if the game should be removed from active games, <code>false</code> otherwise</returns>
    public abstract bool ClientDisconnect (ClientConnection client);


    protected void SendToAllPlayers (BaseNetData data)
    {
        foreach (var p in players)
        {
            netWriter.Send(p.Value, data);
        }
    }

    protected void SendToOtherPlayers (ulong id, BaseNetData data)
    {
        foreach (var p in players)
        {
            if (p.Value.ID != id) netWriter.Send(p.Value, data);
        }
    }

    protected void RemovePlayersActiveGame ()
    {
        foreach (var p in players)
        {
            p.Value.ActiveGameID = 0;
        }
    }

    protected void OnChangeState (object sender, State state)
    {
        EventHandler<State> e = ChangeState;
        if (e != null) e(sender, state);
    }
}


