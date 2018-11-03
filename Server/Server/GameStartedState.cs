using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

public class GameStartedState : GameState
{
    private const long BALLS_TIME_RATE = 1800;
    private Timer timer;
    private Queue<ushort> balls;
    private bool lineReached = false;
    private bool bingoReached = false;

    public GameStartedState (NetworkWriter networkWriter, Dictionary<ulong, Player> playerDic, ConcurrentDictionary<ulong, ClientData> clientList)
        : base(networkWriter, playerDic, clientList)
    {
        actualState = State.GAME_STARTED;
        InitBalls();

        SendToAllPlayers(new BaseNetData(ServerCommands.GAME_BEGAN));
        Console.WriteLine("GAME_STARTED");
        timer = new Timer(BallRevealed, null, BALLS_TIME_RATE, Timeout.Infinite);
    }

    private void InitBalls()
    {
        List<ushort> ballsList = new List<ushort>(baseNumbersList);
        ballsList.Shuffle();
        balls = new Queue<ushort>(ballsList);
    }

    public override void ProcessCommand (ClientData client, BaseNetData data) { }
    public override void ClientConnect (ClientData client) { }

    public override bool ClientDisconnect (ClientData client)
    {
        lock (stateLock)
        {
            players.Remove(client.clientConnection.ID);
        }

        return true;
    }

    private void BallRevealed (object obj)
    {
        if (bingoReached || balls.Count == 0)
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            timer.Dispose();
            actualState = State.WAIT_NEW_GAME;

            OnChangeState(this, actualState);
        }
        else
        {
            List<Player> winners = new List<Player>();
            ushort ball = balls.Dequeue();

            ushort lines = 0, bingos = 0, result;
            foreach (KeyValuePair<ulong, Player> p in players)
            {
                foreach (Card c in p.Value.cards)
                {
                    result = c.EvaluateBall(ball);

                    switch (result)
                    {
                        case 1:
                            winners.Add(p.Value);
                            lines++;
                            break;
                        case 2:
                            winners.Add(p.Value);
                            bingos++;
                            break;
                        default:
                            break;
                    }
                }
            }

            UShortVector3 vector;
            if (bingos > 0 && !bingoReached && winners.Count > 0)
            {
                vector = new UShortVector3(ball, 0, bingos);

                ushort credit = (ushort)(GameLogic.BINGO_REWARD / winners.Count);
                foreach (Player p in winners)
                {
                    p.clientData.EarnCredit(credit, netWriter);
                }
                bingoReached = true;
            }
            else if (lines > 0 && !lineReached && winners.Count > 0)
            {
                vector = new UShortVector3(ball, lines, 0);

                ushort credit = (ushort)(GameLogic.LINE_REWARD / winners.Count);
                foreach (Player p in winners)
                {
                    p.clientData.EarnCredit(credit, netWriter);
                }
                lineReached = true;
            }
            else
            {
                vector = new UShortVector3(ball, 0, 0);
            }

            UShortVector3NetData netData = new UShortVector3NetData(ServerCommands.BALL_REVEALED, vector);
            SendToAllPlayers(netData);

            timer.Change(BALLS_TIME_RATE, Timeout.Infinite);
        }
    }
}
