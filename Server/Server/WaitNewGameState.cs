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
    private List<string> generatedCards;


    public WaitNewGameState (NetworkWriter networkWriter, Dictionary<ulong, Player> playerDic, ConcurrentDictionary<ulong, ClientConnection> clientList)
        : base(networkWriter, playerDic, clientList)
    {
        actualState = State.WAIT_NEW_GAME;
        players.Clear();
        generatedCards = new List<string>();

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
        List<Card> cards = new List<Card>();
        for (ushort i=0;i<nCards;++i)
        {
            cards.Add(GenerateCard());
        }

        Player player = new Player(cards,client);
        players.Add(client.ID, player);

        CardsNetData cardsNetData = new CardsNetData(ServerCommands.CARDS_RESPONSE,cards);
        netWriter.Send(client, cardsNetData);
    }

    private Card GenerateCard ()
    {
        Card card;

        bool ok = false;
        int randNum;
        ushort[] cardData;
        List<ushort> valuesList;
        do
        {
            cardData = new ushort[15];
            valuesList = new List<ushort>(baseNumbersList);
            for (int i=0; i<15; ++i)
            {
                randNum = rng.Next(0, MAX_NUMBER - i)+1;
                cardData[i] = valuesList[randNum];
                valuesList.RemoveAt(randNum);
            }

            card = new Card(cardData);
            string stringCode = card.StringCode();
            if (!generatedCards.Contains(stringCode))
            {
                ok = true;
                generatedCards.Add(stringCode);
            }

        } while (!ok);

        return card;
    }
}
