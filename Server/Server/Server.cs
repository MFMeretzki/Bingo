using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

class Server
{

    public const int BUFFER_SIZE = 1024;

    private readonly string IP = "192.168.0.164";
    private const int PORT = 27015;
    private TcpListener tcpListener;
    private NetworkWriter networkWriter;
    private ConcurrentDictionary<ulong, ClientConnection> clientList = new ConcurrentDictionary<ulong, ClientConnection>();
    private GameLogic gameLogic;
    private ulong nextID = 0;


    public Server ()
    {
        networkWriter = new NetworkWriter(BUFFER_SIZE);
    }


    public void Start ()
    {
        Start(IP);
    }
    public void Start (string IP)
    {
        gameLogic = new GameLogic(networkWriter, clientList);
        OnClientConnection += gameLogic.ClientConnect;

        IPAddress ipAddress = IPAddress.Parse(IP);
        try
        {
            tcpListener = new TcpListener(ipAddress, PORT);
            tcpListener.Start();

            while (true)
            {
                TcpClient tcpClient = tcpListener.AcceptTcpClient();

                ulong id = GetNextID();
                ClientConnection client = new ClientConnection(id, tcpClient);
                clientList.TryAdd(id, client);

                //subscribe to the client message event
                client.ClientMessage += ProcessCommand;
                client.ConnectionError += HandleClientConnectionError;
                client.StartRead();
                if (OnClientConnection != null) OnClientConnection(this, client);
            }
        }
        catch (SocketException se)
        {
            Console.WriteLine("Error accepting client connection: " + se.ErrorCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error starting the server: " + ex.Message);
        }
    }

    public void Stop ()
    {
        OnClientConnection -= gameLogic.ClientConnect;
        tcpListener.Stop();

        networkWriter.Disconnect();
        foreach (ClientConnection client in clientList.Values)
        {
            client.Disconnect();
        }
    }


    private void ProcessCommand (Object sender, BaseNetData data)
    {
        ClientConnection client = (ClientConnection)sender;
        gameLogic.ProcessCommand(client, data);
    }

    private void HandleClientConnectionError (Object sender, string error)
    {
        ClientConnection client = (ClientConnection)sender;
        if (clientList.TryRemove(client.ID, out client))
        {
            Console.WriteLine("Client " + client.ID + " disconnected with error: " + error);
            client.Disconnect();
            gameLogic.ClientDisconnect(client);
        }
    }

    private ulong GetNextID ()
    {
        if (nextID >= ulong.MaxValue)
        {
            for (ulong i = 1; i < ulong.MaxValue; ++i)
            {
                if (!clientList.ContainsKey(i)) return i;
            }
            return 0;
        }
        else return ++nextID;
    }

    public event EventHandler<ClientConnection> OnClientConnection;
}
