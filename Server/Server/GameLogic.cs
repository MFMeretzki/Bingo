using System.Collections.Generic;
using System.Collections.Concurrent;

public class GameLogic
{
    public const ushort CARD_PRICE = 5;
    public const ushort LINE_REWARD = 10;
    public const ushort BINGO_REWARD = 20;

    private NetworkWriter netWriter;
    private GameState gameState;
    private Dictionary<ulong, GameState.Player> players;
    private ConcurrentDictionary<ulong, ClientData> clientList;


    public GameLogic (NetworkWriter networkWriter, ConcurrentDictionary<ulong, ClientData> clientList)
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


    public void ProcessCommand (ClientData client, BaseNetData data)
    {
        gameState.ProcessCommand(client, data);
    }

    /// <summary>
    /// Connect a player to the game.
    /// </summary>
    /// <param name="client">The client connection of the player</param>
    public void ClientConnect (object sender, ClientData client)
    {
        gameState.ClientConnect(client);
    }

    /// <summary>
    /// Disconnect a player from the game.
    /// </summary>
    /// <param name="client">The client connection of the player</param>
    /// <returns><code>true</code> if the game should be removed from active games, <code>false</code> otherwise</returns>
    public bool ClientDisconnect (ClientData client)
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
