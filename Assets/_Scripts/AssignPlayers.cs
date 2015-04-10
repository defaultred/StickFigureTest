using UnityEngine;
using System.Collections;

public class AssignPlayers:Photon.MonoBehaviour
{

	bool grandmaSelected = false;
	bool childSelected = false;
	bool firstPlayerToJoin = false;
	private PhotonView gameView;
	bool inRoom;
	float jump = 10000;
	bool isJumping = false;
	bool facesAssigned = false;

	public GameObject peerFace;
	public GameObject localFace;
	public GameObject NetworkedChild;
	public GameObject NetworkedGrandma;
	public GameObject joinServerButton;
	public GameObject waitingButton;
	public GameObject placeHolderSumo;




	// Use this for initialization
	void Start()
	{
		PhotonNetwork.ConnectUsingSettings("3.0");
		waitingButton.SetActive(false);

	}

	public void loadLevel()
	{
		if(!PhotonNetwork.insideLobby)
		{
			PhotonNetwork.LeaveRoom();
			Application.LoadLevel("StickFigureTest");
		}
	}
	
	void Update()
	{
		if(PhotonNetwork.playerList.Length >= 2)
		{
			waitingButton.SetActive(false);
			joinServerButton.SetActive(false);
		}
		else if(PhotonNetwork.playerList.Length == 1 && inRoom)
		{
			waitingButton.SetActive(true);
			joinServerButton.SetActive(false);
		}

		if(PhotonNetwork.playerList.Length == 1 && childSelected == true) //if host leaves
		{
			PhotonNetwork.LeaveRoom();
			inRoom = false;
			grandmaSelected = false;
			childSelected = false;
			firstPlayerToJoin = false;
			waitingButton.SetActive(false);
			joinServerButton.SetActive(true);
		}

		//if(PhotonNetwork.playerList.Length == 1 && gameHasStarted == true)
		//{
		//	PhotonNetwork.LeaveRoom();
		//	Application.LoadLevel("StickFigureTest");
		//}
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
		RoomOptions options = new RoomOptions()
		{
			isVisible = false,
			isOpen = true,
			cleanupCacheOnLeave = true
		};
		PhotonNetwork.JoinOrCreateRoom("newRoom", options, TypedLobby.Default);
		waitingButton.SetActive(true);
		joinServerButton.SetActive(false);
		inRoom = true;
	}

	void OnCreatedRoom()
	{
		grandmaSelected = true;
		firstPlayerToJoin = true;
		placeHolderSumo.SetActive(false);
		NetworkedGrandma = PhotonNetwork.Instantiate("Granny_Pogo", new Vector3(-6.94f, 10.2f, -.39f), Quaternion.Euler(0, 0, 90), 0);
		NetworkedGrandma.GetComponent<Movement>().enabled = true;
		//localFace.transform.SetParent(NetworkedGrandma.transform);
		//localFace.transform.localPosition = new Vector3(20,0,0);
	}

	void OnJoinedRoom()
	{
		if(firstPlayerToJoin == false)
		{
			childSelected = true;
			placeHolderSumo.SetActive(false);
			NetworkedChild = PhotonNetwork.Instantiate("Kid_Pogo", new Vector3(7.47f, 10.2f, -.49f), Quaternion.Euler(0, 0, 90), 0);
			NetworkedChild.GetComponent<Movement>().enabled = true;
			//localFace.transform.SetParent(NetworkedChild.transform);
			//localFace.transform.localPosition = new Vector3(34, 0, 0);
		}
	}
}