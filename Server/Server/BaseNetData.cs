using System;

public class BaseNetData : EventArgs
{
    public ushort command { get; private set; }


    public BaseNetData (ushort command)
    {
        this.command = command;
    }
}
