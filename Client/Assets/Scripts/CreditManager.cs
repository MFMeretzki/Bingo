using UnityEngine;
using UnityEngine.UI;

public class CreditManager : MonoBehaviour {

    public Text creditText;
    
	void Start ()
    {
        NetworkManager.AddOnCreditChangedListener(OnCreditChanged);
	}
	
	void Update ()
    {
		
	}

    private void OnDestroy ()
    {
        NetworkManager.RemoveOnCreditChangedListener(OnCreditChanged);
    }

    private void OnCreditChanged (ushort credit)
    {
        creditText.text = credit.ToString();
    }
}
