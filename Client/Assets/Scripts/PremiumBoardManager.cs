using UnityEngine;
using UnityEngine.UI;

public class PremiumBoardManager : MonoBehaviour {

    private const string LINE = "Line      ";
    private const string BINGO = "Bingo   ";

    private bool lineSet = false;
    private bool bingoSet = false;

    public Text lineText;
    public Text bingoText;

    void Start ()
    {
		
	}
	

	void Update ()
    {
		
	}

    public void ResetBoard ()
    {
        lineText.text = "";
        bingoText.text = "";
        lineSet = false;
        bingoSet = false;
    }

    public void SetLine (ushort value)
    {
        if (!lineSet) lineText.text = LINE + value.ToString();
    }

    public void SetBingo (ushort value)
    {
        if (!bingoSet) bingoText.text = BINGO + value.ToString();
    }
}
