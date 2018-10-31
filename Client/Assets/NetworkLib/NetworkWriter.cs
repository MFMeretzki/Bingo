using System.Net.Sockets;
using System.Threading;

public class NetworkWriter
{
    private NetworkStream netStream;
    private LockQueue<BaseNetData> writeQueue = new LockQueue<BaseNetData>();
    private Thread writeThread;
    private ManualResetEvent queueEvent = new ManualResetEvent(false);
    private bool stopThread;
    private System.Exception exception = null;

    private readonly int READ_BUFFER_SIZE;
    private byte[] buffer;


    public NetworkWriter (NetworkStream stream, int bufferSize)
    {
        netStream = stream;
        READ_BUFFER_SIZE = bufferSize;
        buffer = new byte[READ_BUFFER_SIZE];

        writeThread = new Thread(this.ThreadWork);
        writeThread.Start();
    }


    public void Send (BaseNetData data)
    {
        if (exception != null) throw exception;

        writeQueue.Enqueue(data);
        queueEvent.Set();
    }

    public void Disconnect ()
    {
        stopThread = true;
        queueEvent.Set();
    }


    private void ThreadWork ()
    {
        stopThread = false;
        BaseNetData bData;
        while (!stopThread)
        {
            bool success = writeQueue.TryDequeue(out bData);
            if (success)
            {
                try
                {
                    int packetLenght = Encoder.Encode(buffer, bData);
                    netStream.Write(buffer, 0, packetLenght);
                }
                catch (System.Exception ex)
                {
                    exception = ex;
                }
            }
            else
            {
                queueEvent.WaitOne();
                queueEvent.Reset();
            }
        }
    }

}
