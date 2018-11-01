using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour {

    public Text cardsNumberText;
    public ushort cardsNumber;

    public CardPanel[] cardPanel;
    private bool gameMode = false;

	void Start () {
        NetworkManager.AddOnGettingCards(OnGettingCards);
        cardsNumberText.text = cardsNumber.ToString();
        foreach(CardPanel cp in cardPanel)
        {
            cp.gameObject.SetActive(false);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnDestroy ()
    {
        NetworkManager.RemoveOnGettingCards(OnGettingCards);
    }

    public void GetCards ()
    {
        NetworkManager.Instance.GetCard(cardsNumber);
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

    private void OnStartingNewGame(ushort timeRemaining)
    {

    }

    private void OnGettingCards (List<ushort[]> cards)
    {
        int nCards = cards.Count;

        for (int i = 0; i < cards.Count; ++i)
        {
            cardPanel[i].gameObject.SetActive(true);
            cardPanel[i].SetCardData(cards[i]);
        }
    }
}
