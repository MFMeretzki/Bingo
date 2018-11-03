using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ClientData
{
    public  ClientConnection clientConnection;
    private ushort credit = 50;

    public ClientData (ClientConnection clientConnection, NetworkWriter netWriter)
    {
        this.clientConnection = clientConnection;

        UShortNetData netData = new UShortNetData(ServerCommands.CREDIT, credit);
        netWriter.Send(clientConnection, netData);
    }

    public bool SpendCredit (ushort value, NetworkWriter netWriter)
    {
        bool ok = false;

        if (value <= credit)
        {
            credit -= value;

            UShortNetData netData = new UShortNetData(ServerCommands.CREDIT, credit);
            netWriter.Send(clientConnection, netData);
            ok = true;
        }

        return ok;
    }

    public void EarnCredit (ushort value, NetworkWriter netWriter)
    {
        credit += value;

        UShortNetData netData = new UShortNetData(ServerCommands.CREDIT, credit);
        netWriter.Send(clientConnection, netData);
    }
}
