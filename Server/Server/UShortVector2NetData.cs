public class UShortVector2NetData : BaseNetData
{
    public UShortVector2 vector { get; set; }


    public UShortVector2NetData (ushort command, UShortVector2 vector) : base(command)
    {
        this.vector = vector;
    }
}
