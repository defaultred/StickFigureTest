using UnityEngine;
using System.Collections;

public class AssignPlayers : Photon.MonoBehaviour {

	public GameObject Grandma;
	public GameObject Child;
	bool grandmaSelected = false;
	bool childSelected = false;
	bool firstPlayerToJoin = false;
	private PhotonView gameView;
	public GameObject joinServerButton;
	public GameObject waitingButton;
	bool inRoom;
	float jump = 10000;
	bool isJumping = false;
	public GameObject NetworkedChild;
	public GameObject NetworkedGrandma;
	bool gameHasStarted;



	// Use this for initialization
	void Start () 
	{
		PhotonNetwork.ConnectUsingSettings ("1.0");
		waitingButton.SetActive (false);
		Grandma.GetComponent<Movement> ().enabled = false;
		Child.GetComponent<Movement> ().enabled = false;

	}

	public void loadLevel()
	{
		if (!PhotonNetwork.insideLobby) {
			PhotonNetwork.LeaveRoom ();
			Application.LoadLevel ("StickFigureTest");
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (PhotonNetwork.playerList.Length >= 2) 
		{
			waitingButton.SetActive (false);
			joinServerButton.SetActive (false);
			gameHasStarted = true;
		} 
		else if (PhotonNetwork.playerList.Length == 1 && inRoom) 
		{
			waitingButton.SetActive (true);
			joinServerButton.SetActive (false);
		}

		if(PhotonNetwork.playerList.Length == 1 && childSelected == true) //if host leaves
		{
			PhotonNetwork.LeaveRoom();
			inRoom = false;
			grandmaSelected = false;
			childSelected = false;
			firstPlayerToJoin = false;
			waitingButton.SetActive (false);
			joinServerButton.SetActive (true);
		}

		if (PhotonNetwork.playerList.Length == 1 && gameHasStarted == true)
		{
			PhotonNetwork.LeaveRoom ();
			gameHasStarted = false;
			Application.LoadLevel ("StickFigureTest");
		}

		/*if (NetworkedChild.GetComponent<PhotonView> ().isMine) 
		{
			NetworkedChild.GetComponent<Movement>().enabled = true;
		}
		else
		{
			NetworkedChild.GetComponent<Movement>().enabled = false;
		}


		if (NetworkedGrandma.GetComponent<PhotonView> ().isMine) 
		{
			NetworkedGrandma.GetComponent<Movement> ().enabled = true;
		} 
		else 
		{
			NetworkedGrandma.GetComponent<Movement> ().enabled = false;
		}
		
		if (grandmaSelected) {
			Grandma.GetComponent<Movement>().enabled = true;
		} else {Grandma.GetComponent<Movement> ().enabled = false;
		}
		if (childSelected && Child != null) {
			Child.GetComponent<Movement> ().enabled = true;
		} else {Child.GetComponent<Movement> ().enabled = false;
		}*/
	}

	void OnLeftRoom()
	{
		inRoom = false;
		grandmaSelected = false;
		childSelected = false;
		firstPlayerToJoin = false;
	}

	public void StartChat()
	{
		RoomOptions options = new RoomOptions ()
		{ isVisible = false, isOpen = true, cleanupCacheOnLeave = true };
		PhotonNetwork.JoinOrCreateRoom ("newRoom", options, TypedLobby.Default);
		waitingButton.SetActive (true);
		joinServerButton.SetActive (false);
		inRoom = true;
	}

	void OnCreatedRoom()
	{
		grandmaSelected = true;
		firstPlayerToJoin = true;
		Child.SetActive (false);
		Grandma.SetActive (false);
		NetworkedGrandma = PhotonNetwork.Instantiate("Granny_Pogo" , new Vector3(-6.94f, -.2f, -.39f), Quaternion.Euler(0, 0, 90), 0);
		NetworkedGrandma.GetComponent<Movement> ().enabled = true;
		//NetworkedGrandma.GetComponent<Movement> ().enabled = true;
	}

	void OnJoinedRoom()
	{
		if (firstPlayerToJoin == false) 
		{
			childSelected = true;
			Grandma.SetActive (false);
			Child.SetActive (false);
			NetworkedChild = PhotonNetwork.Instantiate("Kid_Pogo", new Vector3(7.47f, -.2f, -.49f), Quaternion.Euler(0, 0, 90), 0);
			NetworkedChild.GetComponent<Movement>().enabled = true;
			//NetworkedKid.GetComponent<Movement> ().enabled = true;
		}
	}
}
