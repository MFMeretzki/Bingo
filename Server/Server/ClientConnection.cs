using System;
using System.Net.Sockets;

public class ClientConnection
{
    /// <summary>
    /// Client connection identifier within the server
    /// </summary>
    public ulong ID { get; }
    public ulong ActiveGameID { get; set; } = 0;

    private TcpClient tcpClient;
    private NetworkStream netStream;
    private byte[] readBuffer = new byte[Server.BUFFER_SIZE];
    private object writeStreamLock = new object();

    public ClientConnection (ulong identifier, TcpClient client)
    {
        ID = identifier;
        tcpClient = client;
        netStream = tcpClient.GetStream();
    }


    /// <summary>
    /// This event will be fired when the client recives a new message.
    /// </summary>
    public event EventHandler<BaseNetData> ClientMessage;

    /// <summary>
    /// This event will be fired when a connection error happens in the client.
    /// </summary>
    public event EventHandler<string> ConnectionError;


    public void StartRead ()
    {
        try
        {
            Array.Clear(readBuffer, 0, Server.BUFFER_SIZE);
            netStream.BeginRead(readBuffer, 0, Server.BUFFER_SIZE, new AsyncCallback(ReadCallback), null);
        }
        catch (Exception ex)
        {
            OnConnectionError(ex.Message);
        }
    }

    public void Disconnect ()
    {
        tcpClient.Close();
    }


    public void Write (byte[] buffer, int packetLength)
    {
        lock (writeStreamLock)
        {
            netStream.Write(buffer, 0, packetLength);
        }
    }

    private void ReadCallback (IAsyncResult ar)
    {
        try
        {
            int bytesRead = netStream.EndRead(ar);
            if (bytesRead < 1)
            {
                OnConnectionError(bytesRead + " bytes read from stream");
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
                        Console.WriteLine("Warning: packetLength " + packetLength + " and offset " + offset + " and bytesread " + bytesRead);
                        break;
                    }

                    OnClientMessage(data);
                    offset += packetLength;
                }
                if (offset != bytesRead)
                {
                    Console.WriteLine("Warning: sum of packet lengths " + offset + " != bytes read " + bytesRead);
                }

                Array.Clear(readBuffer, 0, Server.BUFFER_SIZE);
                netStream.BeginRead(readBuffer, 0, Server.BUFFER_SIZE, new AsyncCallback(ReadCallback), null);
            }
        }
        catch (Exception ex)
        {
            OnConnectionError(ex.Message);
        }
    }

    private void OnClientMessage (BaseNetData data)
    {
        var e = ClientMessage;
        if (e != null) e(this, data);
    }

    private void OnConnectionError (string error)
    {
        var e = ConnectionError;
        if (e != null) e(this, error);
    }
}
