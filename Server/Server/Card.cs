using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                notMarked[i].Add(values[j + ((i - 1) * 5)]);
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

                if (completeLine[1]&& completeLine[2]&& completeLine[3])
                {
                    retValue = 2;
                }
            }
        }

        return retValue;
    }
}
