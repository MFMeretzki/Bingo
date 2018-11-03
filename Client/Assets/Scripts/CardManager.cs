using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public Text cardsNumberText;
    public ushort cardsNumber;

    public GameObject connectionButton;
    public GameObject demandPanel;
    public CardPanel[] cardPanel;
    public BallsBoardManager ballsBoardManager;
    public PremiumBoardManager premiumBoarManager;
    private bool gameMode = false;
    private int activePanels = 0;

    void Start ()
    {
        NetworkManager.AddOnStartingNewGame(OnStartingNewGame);
        NetworkManager.AddOnGettingCards(OnGettingCards);
        NetworkManager.AddOnBallRevealedListener(OnBallRevealed);
        NetworkManager.AddOnConnectionErrorListener(OnConnectionError);
        NetworkManager.AddOnGameBeganListener(OnGameBegan);

        cardsNumberText.text = cardsNumber.ToString();
        foreach (CardPanel cp in cardPanel)
        {
            cp.gameObject.SetActive(false);
        }

        premiumBoarManager.ResetBoard();
    }

    void Update () { }

    private void OnDestroy ()
    {
        NetworkManager.RemoveOnStartingNewGame(OnStartingNewGame);
        NetworkManager.RemoveOnGettingCards(OnGettingCards);
        NetworkManager.RemoveOnBallRevealedListener(OnBallRevealed);
        NetworkManager.RemoveOnConnectionErrorListener(OnConnectionError);
        NetworkManager.RemoveOnGameBeganListener(OnGameBegan);
    }

    public void Connect ()
    {
        NetworkManager.Instance.SearchGame();
        connectionButton.SetActive(false);
    }

    public void GetCards ()
    {
        NetworkManager.Instance.GetCard(cardsNumber);
        demandPanel.SetActive(false);
    }

    public void PlusNumber ()
    {
        if (cardsNumber == 4)
        {
            cardsNumber = 1;
        }
        else
        {
            cardsNumber++;
        }

        cardsNumberText.text = cardsNumber.ToString();
    }

    public void MinusNumber ()
    {
        if (cardsNumber == 1)
        {
            cardsNumber = 4;
        }
        else
        {
            cardsNumber--;
        }

        cardsNumberText.text = cardsNumber.ToString();
    }

    private void OnStartingNewGame (ushort timeRemaining)
    {
        demandPanel.SetActive(true);

        for (int i = 0; i < cardsNumber; ++i)
        {
            cardPanel[i].Clear();
            cardPanel[i].gameObject.SetActive(false);
        }
        premiumBoarManager.ResetBoard();
        ballsBoardManager.Clear();
    }

    private void OnGettingCards (List<Card> cards)
    {
        activePanels = cards.Count;

        for (int i = 0; i < cards.Count; ++i)
        {
            cardPanel[i].gameObject.SetActive(true);
            cardPanel[i].SetCardData(cards[i].values);
        }
    }

    private void OnBallRevealed (UShortVector3 vector)
    {
        Debug.Log(vector.x + ", " + vector.y + ", " + vector.z);
        if (gameMode) return;

        ballsBoardManager.SetBall(vector.x);

        for (ushort i=0; i<activePanels; ++i)
        {
            cardPanel[i].OnBallRevealed(vector.x);
        }

        if (vector.y > 0) premiumBoarManager.SetLine(vector.y);
        if (vector.z > 0) premiumBoarManager.SetBingo(vector.z);

    }

    private void OnGameBegan ()
    {
        demandPanel.SetActive(false);
    }

    private void OnConnectionError ()
    {
        connectionButton.SetActive(true);
    }
}
