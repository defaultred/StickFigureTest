using UnityEngine;
using System.Collections;

public class NetworkCharacter1 : Photon.MonoBehaviour 
{
	private Vector3 correctPlayerPos;
	//private Quaternion correctPlayerRot;


	void start()
	{
		photonView.observed = this;
	}
	// Update is called once per frame
	void Update()
	{
		if (!photonView.isMine)
		{
			transform.position = Vector3.MoveTowards(transform.localPosition, this.correctPlayerPos, Time.deltaTime * 5);
			//transform.rotation = Quaternion.Lerp(transform.localRotation, this.correctPlayerRot, Time.deltaTime * 5);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			// We own this player: send the others our data
			stream.SendNext(transform.localPosition);
			//stream.SendNext(transform.localRotation);
		}
		else
		{
			// Network player, receive data
			correctPlayerPos = (Vector3) stream.ReceiveNext();
			//this.transform.localRotation = (Quaternion) stream.ReceiveNext();
		}
	}
}
