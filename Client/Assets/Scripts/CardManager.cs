using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour {

    public CardPanel[] cardPanel;
    private bool gameMode = false;

	void Start () {
        NetworkManager.AddOnGettingCards(OnGettingCards);

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnGettingCards(List<ushort[]> cards)
    {
        foreach (ushort[] card in cards)
        {
            Debug.Log("Card");
        }
    }
}
