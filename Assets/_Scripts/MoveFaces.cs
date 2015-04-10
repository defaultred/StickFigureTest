using UnityEngine;
using System.Collections;

public class MoveFaces : MonoBehaviour 
{

	public GameObject peerFace;
	public GameObject localFace;
	
	void Start () 
	{
		peerFace = GameObject.Find ("PeerFace_LeftCorner");
		localFace = GameObject.Find ("LocalFace_RightCorner");

		if (gameObject.GetComponent<PhotonView> ().isMine == true) 
		{
			localFace.transform.SetParent (gameObject.transform);
			localFace.transform.localPosition = new Vector3(30,0,0);

		} 
		else 
		{
			peerFace.transform.SetParent (gameObject.transform);
			peerFace.transform.localPosition = new Vector3(30,0,0);
		}
	}
}
