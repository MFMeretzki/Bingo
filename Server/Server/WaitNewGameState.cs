using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

public class WaitNewGameState : GameState
{
    private const long WAITING_TIMEOUT = 15000;
    private const long TIME_MARGIN = 100;

    private long startTime;
    private Timer timer;


    public WaitNewGameState (NetworkWriter networkWriter, Dictionary<ulong, Player> playerDic, ConcurrentDictionary<ulong, ClientConnection> clientList)
        : base(networkWriter, playerDic, clientList)
    {
        actualState = State.WAIT_NEW_GAME;
        players.Clear();

        Console.WriteLine("WAIT_NEW_GAME");
        startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        SendStartingNewGameMsg();
        timer = new Timer(StartNewGame, null, WAITING_TIMEOUT, Timeout.Infinite);
    }

    public override void ProcessCommand (ClientConnection client, BaseNetData data)
    {
        lock (stateLock)
        {
            switch (data.command)
            {
                case ServerCommands.CLIENT_CONNECTED:
                    SNGNetData netData = new SNGNetData(ServerCommands.STARTING_NEW_GAME, WAITING_TIMEOUT);
                    netWriter.Send(client, netData);
                    break;
                case ServerCommands.GET_CARD:
                    UShortNetData getCardData = (UShortNetData)data;
                    SendCards(client, getCardData.value);
                    break;
                default:
                    break;
            }
        }
    }

    public override void ClientConnect (ClientConnection client)
    {
        lock (stateLock) {
            Console.WriteLine("Client connected: " + client.ID);
            long elapsedTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime;
            ushort remainingTime = (ushort)((WAITING_TIMEOUT - elapsedTime) / 1000);
            UShortNetData netData = new UShortNetData(ServerCommands.STARTING_NEW_GAME, remainingTime);
            netWriter.Send(client, netData);
        }
    }

    public override bool ClientDisconnect (ClientConnection client)
    {
        lock (stateLock)
        {
            players.Remove(client.ID);
        }

        return true;
    }

    private void StartNewGame (object obj)
    {
        if (players.Count == 0)
        {
            Console.WriteLine("WAIT_NEW_GAME");
            startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            SendStartingNewGameMsg();
            timer.Change(WAITING_TIMEOUT, Timeout.Infinite);
        }
        else
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            timer.Dispose();
            actualState = State.GAME_STARTED;

            OnChangeState(this, actualState);
        }
    }

    private void SendStartingNewGameMsg ()
    {
        UShortNetData netData = new UShortNetData(ServerCommands.STARTING_NEW_GAME, (ushort)(WAITING_TIMEOUT/1000));
        foreach (var p in clientList)
        {
            netWriter.Send(p.Value, netData);
        }
    }

    private void SendCards (ClientConnection client, ushort nCards)
    {
        List<ushort[]> cardsList = new List<ushort[]>();
        for (ushort i=0;i<nCards;++i)
        {
            ushort[] tiles = new ushort[15];
            for (ushort j=0;j<15;++j)
            {
                tiles[j] = (ushort)(j+1);
            }

            cardsList.Add(tiles);
        }

        Player player = new Player(cardsList,client);
        players.Add(client.ID, player);

        CardsNetData cardsNetData = new CardsNetData(ServerCommands.CARDS_RESPONSE,cardsList);
        netWriter.Send(client, cardsNetData);
    }
}
