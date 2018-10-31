using System.Collections.Generic;

public class CardsNetData : BaseNetData
{
    public List<ushort[]> cards { get; set; }

    public CardsNetData (ushort command, List<ushort[]> cards) : base(command)
    {
        this.cards = cards;
    }
}
