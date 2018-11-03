using System.Collections.Generic;
using System.Collections.Concurrent;

public class GameLogic
{
    private NetworkWriter netWriter;
    private GameState gameState;
    private Dictionary<ulong, GameState.Player> players;
    private ConcurrentDictionary<ulong, ClientConnection> clientList;


    public GameLogic (NetworkWriter networkWriter, ConcurrentDictionary<ulong, ClientConnection> clientList)
    {
        netWriter = networkWriter;
        this.clientList = clientList;
        players = new Dictionary<ulong, GameState.Player>();

        gameState = new WaitNewGameState(networkWriter, players, clientList);
        gameState.ChangeState += OnChangeGameState;
    }


    public void StartGame ()
    {


    }


    public void ProcessCommand (ClientConnection client, BaseNetData data)
    {
        gameState.ProcessCommand(client, data);
    }

    /// <summary>
    /// Connect a player to the game.
    /// </summary>
    /// <param name="client">The client connection of the player</param>
    public void ClientConnect (object sender, ClientConnection client)
    {
        gameState.ClientConnect(client);
    }

    /// <summary>
    /// Disconnect a player from the game.
    /// </summary>
    /// <param name="client">The client connection of the player</param>
    /// <returns><code>true</code> if the game should be removed from active games, <code>false</code> otherwise</returns>
    public bool ClientDisconnect (ClientConnection client)
    {
        return gameState.ClientDisconnect(client);
    }


    private void OnChangeGameState (object sender, GameState.State state)
    {
        gameState.ChangeState -= OnChangeGameState;

        switch (state)
        {
            case GameState.State.WAIT_NEW_GAME:
                gameState = new WaitNewGameState(netWriter,players,clientList);
                gameState.ChangeState += OnChangeGameState;
                break;
            case GameState.State.GAME_STARTED:
                gameState = new GameStartedState(netWriter,players,clientList);
                gameState.ChangeState += OnChangeGameState;
                break;
            default:
                break;
        }
    }

}
