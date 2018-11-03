using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CountDownTimer : MonoBehaviour {

    private Text timeText;
    private ushort remainingTime = 60;
    
	void Start () {
        timeText = GetComponent<Text>();
        NetworkManager.AddOnStartingNewGame(OnStartingNewGame);
    }
	
	void Update () {
		
	}

    private void OnDestroy ()
    {
        NetworkManager.RemoveOnStartingNewGame(OnStartingNewGame);
    }

    private void OnStartingNewGame (ushort remainingTime)
    {
        this.remainingTime = remainingTime;
        StartCoroutine("CountDown");
    }

    private IEnumerator CountDown ()
    {
        while (remainingTime != 0)
        {
            timeText.text = remainingTime.ToString();
            remainingTime--;
            yield return new WaitForSeconds(1.0f);
        }

        timeText.text = "";
    }
}
