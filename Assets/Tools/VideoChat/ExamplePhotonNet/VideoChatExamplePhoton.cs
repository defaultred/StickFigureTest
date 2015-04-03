using UnityEngine;
using System.Collections;

//Photon (available free on the Asset Store is required to use this example
//Comment this class out when you do and use the one below!

//Note (Old version): Photon seems to work well with chunksPerFrame = 4, packetsPerFrame = 4, and audioPacketSize = 1000

//Update
//The latest version of Photon can be set to use TCP, with this connections are more reliable and packets can be larger
//this opens up the ability to use a chunkSize of 64 so higher resolution video chat is possible (256 x 256) on devices with a
//capable GPU (iPhone 5+)

public class VideoChatExamplePhoton:Photon.MonoBehaviour
{

	public bool testMode;
	public bool pngCompression = false;

	//Optional, VideoChat will create these for you if you do not assign them
	public GameObject videoObject;
	public AudioSource audioIn;
	public AudioSource audioOut;
	public bool audioOut3D;

	//Adjust these as you wish for the custom needs of your particular application and platform
	public int chunksPerFrame = 4;
	public int packetsPerFrame = 4;
	public float pixelDeltaMagnitude = 0.002f;
	public int audioFrequency = 5000;
	public int audioPacketSize = 1000;
	public float audioThreshold = 0.1f; //Remove echo for conversations without headphones
	public bool acousticEchoCancellation = false;
	public int width = 64;
	public int height = 64;
	public int chunkSize = 16;

	private string stringWidth = "64";
	private string stringHeight = "64";
	private string stringChunkSize = "16";

	//Unity Networking stuff
	private float clearTime;

	//Two network views for access to audio and video network groups separately
	private PhotonView audioView;
	private PhotonView videoView;

	// Use this for initialization
	IEnumerator Start()
	{
		audioView = gameObject.AddComponent<PhotonView>();
		audioView.synchronization = ViewSynchronization.Off;
		audioView.observed = this;
		audioView.viewID = 1;

		videoView = gameObject.AddComponent<PhotonView>();
		videoView.synchronization = ViewSynchronization.Off;
		videoView.observed = this;
		videoView.viewID = 2;

		PhotonNetwork.ConnectUsingSettings("1.0");

		if(Application.isWebPlayer)
		{
			yield return Application.RequestUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone);
		}

		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		stringWidth = width + "";
		stringHeight = height + "";
		stringChunkSize = chunkSize + "";

		VideoChat.pngCompression = pngCompression;
		VideoChat.requestedWidth = width;
		VideoChat.requestedHeight = height;
		VideoChat.audioPacketSize = audioPacketSize;
		VideoChat.audioOut3D = audioOut3D;
		VideoChat.acousticEchoCancellation = acousticEchoCancellation;
		VideoChat.audioFrequency = audioFrequency;

		//Initialize to set base parameters such as the actual WebCamTexture height and width
		VideoChat.Init();

		//Add was created in case we need to defer the assignment of a videoObject until after it has been Network instantiated
		//In this example we are not doing network instantiation but if we were, this would come in handy
		VideoChat.Add(videoObject, audioIn, audioOut);

		//Make some adjustments to the default video chat quad object for this demo, this assumes a Main Camera at the origin
		if(!videoObject)
		{
			VideoChat.vcObject.transform.localScale *= 1.5f;
			VideoChat.vcObject.transform.position = new Vector3(0, -1.4f, 5);
		}
	}

	void Restart()
	{
		if(PhotonNetwork.room != null)
			PhotonNetwork.Disconnect();

		Resources.UnloadUnusedAssets();
		Application.LoadLevel(Application.loadedLevel);
	}

	void OnGUI()
	{
		if(!VideoChat.tempImage)
			return;

		GUI.Label(new Rect(0, 0, Screen.width, Screen.height), VideoChat.log);
		if(PhotonNetwork.room == null)
		{
			bool oldTestMode = testMode;
			testMode = GUI.Toggle(new Rect(0, 20, Screen.width / 3, 40), testMode, "Test Mode");
			if(testMode == true && oldTestMode == false)
			{
				VideoChat.chunkSize = (stringChunkSize == "" ? 0 : System.Convert.ToInt32(stringChunkSize));
				if(width >= VideoChat.chunkSize && height >= VideoChat.chunkSize)
				{
					VideoChat.requestedWidth = width;
					VideoChat.requestedHeight = height;
				}
				VideoChat.deviceIndex++;
			}

			GUI.Label(new Rect(0, 40, Screen.width / 3, 20), "Width");
			GUI.Label(new Rect(Screen.width / 3, 40, Screen.width / 3, 20), "Height");
			GUI.Label(new Rect(2 * Screen.width / 3, 40, Screen.width / 3, 20), "Chunk Size");
			stringWidth = GUI.TextField(new Rect(0, 60, Screen.width / 3, 40), stringWidth);
			stringHeight = GUI.TextField(new Rect(Screen.width / 3, 60, Screen.width / 3, 40), stringHeight);
			stringChunkSize = GUI.TextField(new Rect(2 * Screen.width / 3, 60, Screen.width / 3, 40), stringChunkSize);

			width = (stringWidth == "" ? 64 : System.Convert.ToInt32(stringWidth));
			height = (stringHeight == "" ? 64 : System.Convert.ToInt32(stringHeight));

			if(testMode)
			{
				if(GUI.Button(new Rect(0, 100, Screen.width, 40), "Change Camera"))
				{
					VideoChat.requestedWidth = width;
					VideoChat.requestedHeight = height;
					VideoChat.deviceIndex++;
				}
				//Added some sliders to toy with performance settings in real time
				GUI.Label(new Rect(0, 160, Screen.width, 40), "Chunks per Frame " + chunksPerFrame);
				chunksPerFrame = System.Convert.ToInt16(GUI.HorizontalSlider(new Rect(0, 140, Screen.width, 40), chunksPerFrame, 0.0f, 512));
				GUI.Label(new Rect(0, 220, Screen.width, 40), "Packets per Frame " + packetsPerFrame);
				packetsPerFrame = System.Convert.ToInt16(GUI.HorizontalSlider(new Rect(0, 200, Screen.width, 40), packetsPerFrame, 0.0f, 512));
				GUI.Label(new Rect(0, 280, Screen.width, 40), "Pixel Delta Magnitude " + pixelDeltaMagnitude);
				pixelDeltaMagnitude = GUI.HorizontalSlider(new Rect(0, 260, Screen.width, 40), pixelDeltaMagnitude, 0.0f, 0.01f);
				GUI.Label(new Rect(0, 320, Screen.width, 40), "Audio Threshold " + audioThreshold);
				audioThreshold = GUI.HorizontalSlider(new Rect(0, 340, Screen.width, 40), audioThreshold, 0.0f, 1.0f);
				VideoChat.acousticEchoCancellation = GUI.Toggle(new Rect(0, 380, Screen.width / 3, 40), VideoChat.acousticEchoCancellation, "Echo Cancellation");
				return;
			}
			if(GUI.Button(new Rect(0, 140, Screen.width, 40), "Start"))
			{
				VideoChat.chunkSize = (stringChunkSize == "" ? 0 : System.Convert.ToInt32(stringChunkSize));
				if(width < VideoChat.chunkSize)
					width = VideoChat.chunkSize;
				if(height < VideoChat.chunkSize)
					height = VideoChat.chunkSize;
				VideoChat.requestedWidth = width;
				VideoChat.requestedHeight = height;
				VideoChat.deviceIndex++;
				PhotonNetwork.CreateRoom("MidnightVideoChat");
			}

			if(GUI.Button(new Rect(0, 180, Screen.width, 40), "Join"))
			{
				VideoChat.chunkSize = (stringChunkSize == "" ? 0 : System.Convert.ToInt32(stringChunkSize));
				if(width < VideoChat.chunkSize)
					width = VideoChat.chunkSize;
				if(height < VideoChat.chunkSize)
					height = VideoChat.chunkSize;
				VideoChat.requestedWidth = width;
				VideoChat.requestedHeight = height;
				VideoChat.deviceIndex++;
				PhotonNetwork.JoinRoom("MidnightVideoChat");
			}
			if(GUI.Button(new Rect(0, 220, Screen.width, 40), "Refresh Master Server"))
			{
				PhotonNetwork.ConnectUsingSettings("1.0");
			}
		}
		else
		{
			if(GUI.Button(new Rect(0, 20, Screen.width, 40), "Disconnect"))
				Restart();
			if(GUI.Button(new Rect(0, 60, Screen.width, 40), "Change Camera"))
			{
				VideoChat.chunkSize = (stringChunkSize == "" ? 0 : System.Convert.ToInt32(stringChunkSize));

				if(width < VideoChat.chunkSize)
					width = VideoChat.chunkSize;
				if(height < VideoChat.chunkSize)
					height = VideoChat.chunkSize;

				VideoChat.requestedWidth = width;
				VideoChat.requestedHeight = height;

				VideoChat.deviceIndex++;
			}
			//Added some sliders to toy with performance settings in real time
			GUI.Label(new Rect(0, 120, Screen.width, 40), "Chunks per Frame " + chunksPerFrame);
			chunksPerFrame = System.Convert.ToInt16(GUI.HorizontalSlider(new Rect(0, 140, Screen.width, 40), chunksPerFrame, 0.0f, 100));
			GUI.Label(new Rect(0, 160, Screen.width, 40), "Packets per Frame " + packetsPerFrame);
			packetsPerFrame = System.Convert.ToInt16(GUI.HorizontalSlider(new Rect(0, 180, Screen.width, 40), packetsPerFrame, 0.0f, 100));
			GUI.Label(new Rect(0, 200, Screen.width, 40), "Pixel Delta Magnitude " + pixelDeltaMagnitude);
			pixelDeltaMagnitude = GUI.HorizontalSlider(new Rect(0, 220, Screen.width, 40), pixelDeltaMagnitude, 0.0f, 0.01f);
			GUI.Label(new Rect(0, 240, Screen.width, 40), "Audio Threshold " + audioThreshold);
			audioThreshold = GUI.HorizontalSlider(new Rect(0, 260, Screen.width, 40), audioThreshold, 0.0f, 1.0f);
			VideoChat.acousticEchoCancellation = GUI.Toggle(new Rect(0, 300, Screen.width / 3, 40), VideoChat.acousticEchoCancellation, "Echo Cancellation");
		}
	}

	void Update()
	{
		if(Input.GetKey(KeyCode.Escape))
			Application.Quit();

		//This is new in version 1.004, initializes things early for thumbnail
		VideoChat.PreVideo();

		if((!testMode && PhotonNetwork.room == null) || (PhotonNetwork.room != null && PhotonNetwork.otherPlayers.Length < 1))
			return;

		#region AUDIO
		VideoChat.audioThreshold = audioThreshold;

		//Collect source audio, this will create a new AudioPacket and add it to the audioPackets list in the VideoChat static class
		VideoChat.FromAudio();

		//Send the latest VideoChat audio packet for a local test or your networking library of choice, in this case Unity Networking
		int numPackets = VideoChat.audioPackets.Count;
		AudioPacket[] tempAudioPackets = new AudioPacket[numPackets];
		VideoChat.audioPackets.CopyTo(tempAudioPackets);

		for(int i = 0; i < numPackets; i++)
		{
			AudioPacket currentPacket = tempAudioPackets[i];

			if(testMode)
				ReceiveAudio(currentPacket.position, currentPacket.length, currentPacket.data); //Test mode just plays back on one machine
			else
				audioView.RPC("ReceiveAudio", PhotonTargets.Others, currentPacket.position, currentPacket.length, currentPacket.data); //Unity Networking

			VideoChat.audioPackets.Remove(tempAudioPackets[i]);
		}
		#endregion


		#region VIDEO
		VideoChat.chunksPerFrame = chunksPerFrame;

		PhotonNetwork.sendRate = (int)(packetsPerFrame + ((1 / Time.fixedDeltaTime) / 10));
		VideoChat.packetsPerFrame = packetsPerFrame;

		VideoChat.pixelDeltaMagnitude = pixelDeltaMagnitude;

		//Collect source video, this will create a new VideoPacket(s) and add it(them) to the videoPackets list in the VideoChat static class
		VideoChat.FromVideo();

		numPackets = VideoChat.videoPackets.Count > VideoChat.packetsPerFrame ? VideoChat.packetsPerFrame : VideoChat.videoPackets.Count;
		VideoPacket[] tempVideoPackets = new VideoPacket[VideoChat.videoPackets.Count];
		VideoChat.videoPackets.CopyTo(tempVideoPackets);

		//Send the latest VideoChat video packets for a local test or your networking library of choice, in this case Unity Networking
		for(int i = 0; i < numPackets; i++)
		{
			VideoPacket currentPacket = tempVideoPackets[i];

			if(testMode)
				ReceiveVideo(currentPacket.x, currentPacket.y, currentPacket.data); //Test mode just displays on one machine
			else
				videoView.RPC("ReceiveVideo", PhotonTargets.Others, currentPacket.x, currentPacket.y, currentPacket.data); //Unity Networking

			VideoChat.videoPackets.Remove(tempVideoPackets[i]);
		}
		#endregion
	}

	[RPC]
	void ReceiveVideo(int x, int y, byte[] videoData)
	{
		VideoChat.ToVideo(x, y, videoData);
	}

	[RPC]
	void ReceiveAudio(int micPosition, int length, byte[] audioData)
	{
		VideoChat.ToAudio(micPosition, length, audioData);
	}

	void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		VideoChat.deviceIndex = VideoChat.deviceIndex; //This resets the camera to prepare for a new connection
	}

	void OnPhotonPlayerConnected(PhotonPlayer player)
	{
		VideoChat.deviceIndex = VideoChat.deviceIndex; //This resets the camera to prepare for a new connection
	}

	void OnDisconnectedFromPhoton()
	{
		Restart();
	}
}


