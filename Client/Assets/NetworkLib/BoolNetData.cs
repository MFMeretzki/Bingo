public class BoolNetData : BaseNetData
{
    public bool value { get; set; }


    public BoolNetData (ushort command, bool value) : base(command)
    {
        this.value = value;
    }
}
