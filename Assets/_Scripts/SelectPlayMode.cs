using UnityEngine;
using System.Collections;

public class SelectPlayMode : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void quitGame()
	{
		Application.Quit();
	}

	public void loadOnlinePlay()
	{
		Application.LoadLevel ("SumoOnline");
	}

	public void loadLocalPlay()
	{
		Application.LoadLevel ("SumoLocal");
	}
}
