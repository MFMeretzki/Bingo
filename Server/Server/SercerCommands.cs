public class ServerCommands
{

    //0 is the null command, dont use for a real command
    //public const ushort CONNECT = 0;

    /// <summary>
    /// client -> server client connected
    /// </summary>
    public const ushort CLIENT_CONNECTED = 1;

    /// <summary>
    /// client -> server get bingo cards
    /// </summary>
    public const ushort GET_CARD = 2;

    /// <summary>
    /// server -> client starting new game
    /// </summary>
    public const ushort STARTING_NEW_GAME = 10;

    /// <summary>
    /// server -> client starting new game
    /// </summary>
    public const ushort CARDS_RESPONSE = 11;
}
