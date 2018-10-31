using System.Collections.Generic;

public class LockQueue<T>
{

    private Queue<T> queue;
    private readonly object qLock;

    public LockQueue ()
    {

        queue = new Queue<T>();
        qLock = new object();

    }

    /// 
    /// Try to dequeue the object at the beggining of the queue. True if an object was dequeued, false otherwise.
    /// 
    public bool TryDequeue (out T value)
    {
        value = default(T);

        lock (qLock)
        {
            if (queue.Count == 0) return false;
            value = queue.Dequeue();
            return true;
        }
    }

    public void Enqueue (T item)
    {
        lock (qLock)
        {
            queue.Enqueue(item);
        }
    }

}
