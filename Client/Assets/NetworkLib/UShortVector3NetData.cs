public class UShortVector3NetData : BaseNetData
{
    public UShortVector3 vector { get; set; }


    public UShortVector3NetData (ushort command, UShortVector3 vector) : base(command)
    {
        this.vector = vector;
    }
}
