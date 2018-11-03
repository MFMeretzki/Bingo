using UnityEngine;
using UnityEngine.UI;

public class CardPanel : MonoBehaviour {

    private readonly Color grey = new Color (0.196f, 0.196f, 0.196f);
    private readonly Color red = new Color(0.815f, 0f, 0f);

    public GameObject[] tiles;
    public ushort[] data;

	void Start () {
		
	}
	
	void Update () {
		
	}

    public void SetCardData(ushort[] cardData)
    {
        data = cardData;

        for (int i=0; i<15; ++i)
        {
            tiles[i].GetComponentInChildren<Text>().text = data[i].ToString();
        }
    }

    public void Clear ()
    {
        foreach(GameObject t in tiles)
        {
            t.GetComponentInChildren<Text>().color = grey;
        }
    }

    public void OnBallRevealed (ushort ball)
    {
        bool ok = false;
        ushort i = 0;
        do
        {
            if (ball == data[i])
            {
                tiles[i].GetComponentInChildren<Text>().color = red;
                ok = true;
            }
            i++;
        } while (i < 15 && !ok);
    }
}
