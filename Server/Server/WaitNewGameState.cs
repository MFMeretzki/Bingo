using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

public class WaitNewGameState : GameState
{
    private const long WAITING_TIMEOUT = 60000;
    private const long TIME_MARGIN = 100;

    private Timer timer;
    private Stopwatch stopwatch = new Stopwatch();
    private Random rng = new Random();


    public WaitNewGameState (NetworkWriter networkWriter, Dictionary<ulong, ClientConnection> playerDic, ConcurrentDictionary<ulong, ClientConnection> clientList)
        : base(networkWriter, playerDic, clientList)
    {
        actualState = State.WAIT_NEW_GAME;

        Console.WriteLine("WAIT_NEW_GAME");
        //SendStartingNewGameMsg();
        timer = new Timer(StartNewGame, null, 30000, Timeout.Infinite);
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

    public override bool ClientDisconnect (ClientConnection client)
    {
        lock (stateLock)
        {
            players.Remove(client.ID);

            // TO DO remove player cards
        }

        return true;
    }

    private void StartNewGame (object obj)
    {
        timer.Change(Timeout.Infinite, Timeout.Infinite);
        timer.Dispose();
        actualState = State.GAME_STARTED;

        OnChangeState(this, actualState);
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
                tiles[j] = j;
            }

            cardsList.Add(tiles);
        }

        CardsNetData cardsNetData = new CardsNetData(ServerCommands.CARDS_RESPONSE,cardsList);
        netWriter.Send(client, cardsNetData);
    }
}
