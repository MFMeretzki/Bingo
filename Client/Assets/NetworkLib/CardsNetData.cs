using System.Collections.Generic;

public class CardsNetData : BaseNetData
{
    public List<Card> cards { get; set; }

    public CardsNetData (ushort command, List<Card> cards) : base(command)
    {
        this.cards = cards;
    }
}
