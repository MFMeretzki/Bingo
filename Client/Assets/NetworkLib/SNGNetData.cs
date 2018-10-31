public class SNGNetData : BaseNetData
{
    public ulong remainingTime { get; set; }

    public SNGNetData (ushort command, ulong remainingTime) : base(command)
    {
        this.remainingTime = remainingTime;
    }
}
