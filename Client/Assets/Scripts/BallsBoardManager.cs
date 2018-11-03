using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallsBoardManager : MonoBehaviour {

    public GameObject BallPrefab;
    public GameObject[] row;
    private List<GameObject>[] balls;

    private void Awake ()
    {
        balls = new List<GameObject>[3];
        for (int i=0; i<3; ++i)
        {
            balls[i] = new List<GameObject>();
        }
    }

    void Start ()
    {
		
	}
	
	void Update ()
    {
		
	}

    public void SetBall (ushort ball)
    {
        for (int i=0; i<3; ++i)
        {
            if (balls[i].Count >= 30) continue;
            GameObject go = Instantiate(BallPrefab, row[i].transform);
            go.GetComponentInChildren<Text>().text = ball.ToString();
            balls[i].Add(go);
            break;
        }
    }

    public void Clear ()
    {
        foreach (List<GameObject> l in balls)
        {
            foreach (GameObject go in l)
            {
                Destroy(go);
            }
            l.Clear();
        }
    }
}
