using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

class Server
{

    public const int BUFFER_SIZE = 1024;

    private readonly string IP = "192.168.1.34";
    private const int PORT = 27015;
    private TcpListener tcpListener;
    private NetworkWriter networkWriter;
    private ConcurrentDictionary<ulong, ClientData> clientList = new ConcurrentDictionary<ulong, ClientData>();
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
                ClientData clientData = new ClientData(client, networkWriter);
                clientList.TryAdd(id, clientData);

                //subscribe to the client message event
                client.ClientMessage += ProcessCommand;
                client.ConnectionError += HandleClientConnectionError;
                client.StartRead();
                if (OnClientConnection != null) OnClientConnection(this, clientData);
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
        foreach (ClientData client in clientList.Values)
        {
            client.clientConnection.Disconnect();
        }
    }


    private void ProcessCommand (Object sender, BaseNetData data)
    {
        ClientConnection client = (ClientConnection)sender;
        ClientData clientData;
        if (clientList.TryGetValue(client.ID, out clientData))
        {
            gameLogic.ProcessCommand(clientData, data);
        }
    }

    private void HandleClientConnectionError (Object sender, string error)
    {
        ClientConnection client = (ClientConnection)sender;
        ClientData clientData;
        if (clientList.TryGetValue(client.ID,out clientData)) {
            Console.WriteLine("Client " + clientData.clientConnection.ID + " disconnected with error: " + error);
            clientData.clientConnection.Disconnect();
            gameLogic.ClientDisconnect(clientData);
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

    public event EventHandler<ClientData> OnClientConnection;
}
