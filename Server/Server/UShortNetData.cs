public class UShortNetData : BaseNetData
{
    public ushort value { get; set; }

    public UShortNetData (ushort command, ushort value) : base(command)
    {
        this.value = value;
    }
}
