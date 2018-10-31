
public class UShortBoolNetData : BaseNetData
{
    public ushort num { get; set; }
    public bool value { get; set; }

    public UShortBoolNetData (ushort command, ushort num, bool value) : base(command)
    {
        this.num = num;
        this.value = value;
    }
}
