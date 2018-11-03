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

    public GameStartedState (NetworkWriter networkWriter, Dictionary<ulong, Player> playerDic, ConcurrentDictionary<ulong, ClientConnection> clientList)
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

    public override void ProcessCommand (ClientConnection client, BaseNetData data) { }
    public override void ClientConnect (ClientConnection client) { }

    public override bool ClientDisconnect (ClientConnection client)
    {
        lock (stateLock)
        {
            players.Remove(client.ID);
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
                            lines++;
                            break;
                        case 2:
                            bingos++;
                            break;
                        default:
                            break;
                    }
                }
            }

            UShortVector3 vector;
            if (bingos > 0 && !bingoReached)
            {
                vector = new UShortVector3(ball, 0, bingos);
                bingoReached = true;
            }
            else if (lines > 0 && !lineReached)
            {
                vector = new UShortVector3(ball, lines, 0);
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
