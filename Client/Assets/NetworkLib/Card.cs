using System.Collections.Generic;

public class Card
{
    public ushort ID { get; protected set; }
    public ushort[] values;
    public List<ushort>[] notMarked;
    public List<ushort>[] marked;
    public bool[] completeLine;

    public Card (ushort [] values)
    {
        this.values = values;
        notMarked = new List<ushort>[3];
        marked = new List<ushort>[3];
        completeLine = new bool[3];

        for (int i = 0; i < 3; ++i)
        {
            notMarked[i] = new List<ushort>();
            marked[i] = new List<ushort>();
            completeLine[i] = false;

            for (int j = 0; j < 5; ++j)
            {
                notMarked[i].Add(values[j + (i * 5)]);
            }
        }
    }

    public ushort EvaluateBall (ushort ball)
    {
        ushort retValue = 0;        // 0 -> nothing special, 1 -> LINE!, 2 -> BINGO!!
        for (int i = 0; i < 3; ++i)
        {
            if (notMarked[i].Contains(ball))
            {
                notMarked[i].Remove(ball);
                marked[i].Add(ball);
                if (marked[i].Count == 5)
                {
                    completeLine[i] = true;
                    retValue = 1;
                }

                if (completeLine[0] && completeLine[1] && completeLine[2])
                {
                    retValue = 2;
                }
            }
        }

        return retValue;
    }

    public string StringCode ()
    {
        string code = values[0].ToString();
        
        for (int i=1; i<15; ++i)
        {
            code += ("-" + values[i].ToString());
        }

        return code;
    }
}
