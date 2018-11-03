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

    public struct Player
    {
        public List<Card> cards;
        public ClientConnection clientConnection;

        public Player (List<Card> cards, ClientConnection clientConnection)
        {
            this.cards = cards;
            this.clientConnection = clientConnection;
        }
    }

    protected const ushort MAX_NUMBER = 90;

    public State actualState { get; protected set; }

    protected NetworkWriter netWriter;
    protected Dictionary<ulong, Player> players;
    protected ConcurrentDictionary<ulong, ClientConnection> clientList;
    protected List<ushort> baseNumbersList;
    protected object stateLock = new object();
    protected Random rng = new Random();

    public GameState (NetworkWriter networkWriter, Dictionary<ulong, Player> playerDic, ConcurrentDictionary<ulong, ClientConnection> clientList)
    {
        netWriter = networkWriter;
        players = playerDic;
        this.clientList = clientList;
        baseNumbersList = new List<ushort>();
        for (ushort i=1; i<=MAX_NUMBER; ++i)
        {
            baseNumbersList.Add(i);
        }
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
    /// Do the proper action when a player enters the game.
    /// </summary>
    /// <param name="client">The incoming player connection</param>
    public abstract void ClientConnect (ClientConnection client);

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
            netWriter.Send(p.Value.clientConnection, data);
        }
    }

    protected void SendToOtherPlayers (ulong id, BaseNetData data)
    {
        foreach (var p in players)
        {
            if (p.Value.clientConnection.ID != id) netWriter.Send(p.Value.clientConnection, data);
        }
    }

    protected void RemovePlayersActiveGame ()
    {
        foreach (var p in players)
        {
            p.Value.clientConnection.ActiveGameID = 0;
        }
    }

    protected void OnChangeState (object sender, State state)
    {
        EventHandler<State> e = ChangeState;
        if (e != null) e(sender, state);
    }
}


