using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{

    private string IP = "192.168.0.164";
    private int PORT = 27015;
    private TcpClient tcpClient = new TcpClient();
    private NetworkStream netStream;
    private NetworkWriter netWriter = null;
    private LockQueue<BaseNetData> readQueue = new LockQueue<BaseNetData>();
    private bool searchingForGame = false;
    private bool connectionError = false;
    private readonly object errorLock = new object();

    private const int BUFFER_SIZE = 1024;
    private byte[] readBuffer = new byte[BUFFER_SIZE];


    private static NetworkManager instance = null;
    public static NetworkManager Instance { get { return instance; } }

    void Awake ()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Update ()
    {
        bool success = false;
        do
        {
            BaseNetData data;
            success = readQueue.TryDequeue(out data);
            if (success) ProcessCommnad(data);
        } while (success);
    }

    public void ConfigureConnection (string IP)
    {
        this.IP = IP;
    }

    public void Disconnect ()
    {
        lock (errorLock)
        {
            connectionError = true;
        }

        searchingForGame = false;

        if (netWriter != null) netWriter.Disconnect();
        tcpClient.Close();
    }

    /// <summary>
    /// Start searching for a new game.
    /// </summary>
    public void SearchGame ()
    {
        if (searchingForGame) return;

        Disconnect();
        connectionError = false;
        try
        {
            Debug.Log("BeggingConnect...");
            searchingForGame = true;
            tcpClient = new TcpClient();
            tcpClient.BeginConnect(IP, PORT, new AsyncCallback(ConnectCallback), null);
        }
        catch (SocketException se)
        {
            Debug.Log("<color=red>BegginConnect error:</color> " + se.ErrorCode);
            ConnectionError();
        }
    }


    #region events

    //**************	EVENTS	******************//

    /// <summary>
    /// Called when the client gets notified that a new game is starting
    /// </summary>
    public delegate void StartingNewGameAction (ushort remainingTime);
    private static event StartingNewGameAction OnStartingNewGame;

    public static void AddOnStartingNewGame (StartingNewGameAction callback)
    {
        OnStartingNewGame += callback;
    }

    public static void RemoveOnStartingNewGame (StartingNewGameAction callback)
    {
        OnStartingNewGame -= callback;
    }

    /// <summary>
    /// Called when the client gets notified that a new game is starting
    /// </summary>
    public delegate void GettingCardsAction (List<Card> cards);
    private static event GettingCardsAction OnGettingCards;

    public static void AddOnGettingCards (GettingCardsAction callback)
    {
        OnGettingCards += callback;
    }

    public static void RemoveOnGettingCards (GettingCardsAction callback)
    {
        OnGettingCards -= callback;
    }

    /// <summary>
    /// Called when an error occurs with the client connection.
    /// </summary>
    public delegate void BallRevealedAction (UShortVector3 vector);
    private static event BallRevealedAction OnBallRevealed;

    public static void AddOnBallRevealedListener (BallRevealedAction callback)
    {
        OnBallRevealed += callback;
    }

    public static void RemoveOnBallRevealedListener (BallRevealedAction callback)
    {
        OnBallRevealed -= callback;
    }

    /// <summary>
    /// Called when an error occurs with the client connection.
    /// </summary>
    public delegate void GameBeganAction ();
    private static event GameBeganAction OnGameBegan;

    public static void AddOnGameBeganListener (GameBeganAction callback)
    {
        OnGameBegan += callback;
    }

    public static void RemoveOnGameBeganListener (GameBeganAction callback)
    {
        OnGameBegan -= callback;
    }

    /// <summary>
    /// Called when credit changed.
    /// </summary>
    public delegate void CreditChangedAction (ushort credit);
    private static event CreditChangedAction OnCreditChanged;

    public static void AddOnCreditChangedListener (CreditChangedAction callback)
    {
        OnCreditChanged += callback;
    }

    public static void RemoveOnCreditChangedListener (CreditChangedAction callback)
    {
        OnCreditChanged -= callback;
    }

    /// <summary>
    /// Called when an error occurs with the client connection.
    /// </summary>
    public delegate void ConnectionErrorAction ();
    private static event ConnectionErrorAction OnConnectionError;

    public static void AddOnConnectionErrorListener (ConnectionErrorAction callback)
    {
        OnConnectionError += callback;
    }

    public static void RemoveOnConnectionErrorListener (ConnectionErrorAction callback)
    {
        OnConnectionError -= callback;
    }

    #endregion

    //**************	PRIVATE METHODS		*************//

    private void ConnectionError ()
    {
        lock (errorLock)
        {
            if (connectionError) return;
            else connectionError = true;
        }

        Disconnect();

        if (OnConnectionError != null)
        {
            BaseNetData errorData = new BaseNetData(0);
            readQueue.Enqueue(errorData);
        }

        Debug.Log("Connection error called");
    }

    private void ConnectCallback (IAsyncResult ar)
    {
        try
        {
            tcpClient.EndConnect(ar);
            Debug.Log("EndConnect: " + tcpClient.Connected);

            netStream = tcpClient.GetStream();
            netWriter = new NetworkWriter(netStream, BUFFER_SIZE);

            //begin the network stream read loop
            Array.Clear(readBuffer, 0, BUFFER_SIZE);
            netStream.BeginRead(readBuffer, 0, BUFFER_SIZE, new AsyncCallback(ReadCallback), null);
        }
        catch (SocketException se)
        {
            Debug.Log("<color=red>Connection error:</color> " + se.ErrorCode);
            ConnectionError();
        }
        catch (Exception ex)
        {
            Debug.Log("<color=red>Network Exception:</color> " + ex.Message);
            ConnectionError();
        }
    }

    private void SendMessage (BaseNetData data)
    {
        try
        {
            netWriter.Send(data);
        }
        catch (Exception ex)
        {
            Debug.Log("<color=red>Network writer exception: " + ex.Message + "</color>");
            ConnectionError();
        }
    }

    private void ReadCallback (IAsyncResult ar)
    {
        try
        {
            int bytesRead = netStream.EndRead(ar);

            if (bytesRead < 1)
            {
                //TODO improve checking, might be called when client disconnects too
                ConnectionError();
                return;
            }
            else
            {
                int offset = 0;
                while (offset < bytesRead)
                {
                    BaseNetData data;
                    int packetLength = Encoder.Decode(readBuffer, offset, out data);

                    if (packetLength == 0)
                    {
                        Debug.Log("<color=yellow>Warning: command " + data.command + " packet length " + packetLength + " and offset " + offset + " and bytes read " + bytesRead + "</color>");
                        break;
                    }

                    readQueue.Enqueue(data);
                    offset += packetLength;
                }
                if (offset != bytesRead)
                {
                    Debug.Log("<color=yellow>Warning: sum of packet lengths " + offset + " != bytes read " + bytesRead + "</color>");
                }

                Array.Clear(readBuffer, 0, BUFFER_SIZE);
                netStream.BeginRead(readBuffer, 0, BUFFER_SIZE, new AsyncCallback(ReadCallback), null);
            }

        }
        catch (Exception ex)
        {
            Debug.Log("<color=red>Network Exception:</color> " + ex.Message);
            ConnectionError();
        }
    }

    private void ProcessCommnad (BaseNetData data)
    {
        switch (data.command)
        {
            case 0:
                if (OnConnectionError != null) OnConnectionError();
                break;
            case ServerCommands.STARTING_NEW_GAME:
                UShortNetData netData = (UShortNetData)data;
                if (OnStartingNewGame != null) OnStartingNewGame(netData.value);
                break;
            case ServerCommands.CARDS_RESPONSE:
                CardsNetData cardsData = (CardsNetData)data;
                if (OnGettingCards != null) OnGettingCards(cardsData.cards);
                break;
            case ServerCommands.BALL_REVEALED:
                UShortVector3NetData ballData = (UShortVector3NetData)data;
                if (OnBallRevealed != null) OnBallRevealed(ballData.vector);
                break;
            case ServerCommands.GAME_BEGAN:
                if (OnGameBegan != null) OnGameBegan();
                break;
            case ServerCommands.CREDIT:
                UShortNetData creditData = (UShortNetData)data;
                if (OnCreditChanged != null) OnCreditChanged(creditData.value);
                break;
            default:
                break;
        }

        Debug.Log("Recived command " + data.command);
    }

    /// <summary>
    /// Inform the server about the desired number of cards.
    /// </summary>
    /// <param name="number">The number of cards</param>
    public void GetCard (ushort nCards)
    {
        UShortNetData data = new UShortNetData(ServerCommands.GET_CARD, nCards);
        SendMessage(data);
    }

}
