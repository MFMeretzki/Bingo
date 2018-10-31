using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

public class NetworkWriter
{

    struct MsgInfo
    {
        public ClientConnection sender;
        public BaseNetData data;
    }

    private readonly int THREAD_COUNT = 4;
    private readonly int BUFFER_SIZE;
    private ConcurrentQueue<MsgInfo> writeQueue = new ConcurrentQueue<MsgInfo>();
    private List<Thread> threadList = new List<Thread>();
    private ManualResetEvent queueEvent = new ManualResetEvent(false);
    private bool stopThread = false;

    public NetworkWriter (int bufferSize)
    {
        BUFFER_SIZE = bufferSize;
        for (int i = 0; i < THREAD_COUNT; ++i)
        {
            Thread t = new Thread(this.ThreadWork);
            t.Start();
            threadList.Add(t);
        }
    }

    public void Disconnect ()
    {
        stopThread = true;
        queueEvent.Set();
    }

    public void Send (ClientConnection clienConnection, BaseNetData data)
    {
        MsgInfo msgInfo = new MsgInfo();
        msgInfo.sender = clienConnection;
        msgInfo.data = data;
        writeQueue.Enqueue(msgInfo);
        queueEvent.Set();
    }

    public void SendAll (List<ClientConnection> clients, BaseNetData data)
    {
        foreach (ClientConnection c in clients)
        {
            MsgInfo msgInfo = new MsgInfo();
            msgInfo.sender = c;
            msgInfo.data = data;
            writeQueue.Enqueue(msgInfo);
            queueEvent.Set();
        }
    }

    private void ThreadWork ()
    {
        byte[] buffer = new byte[BUFFER_SIZE];
        MsgInfo msgInfo;
        while (!stopThread)
        {
            bool success = writeQueue.TryDequeue(out msgInfo);
            if (success)
            {
                try
                {
                    int packetLength = Encoder.Encode(buffer, msgInfo.data);
                    msgInfo.sender.Write(buffer, packetLength);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("NetworkWriter exception: " + ex.Message);
                }
            }
            else
            {
                queueEvent.Reset();
                queueEvent.WaitOne();
            }
        }
    }
}
