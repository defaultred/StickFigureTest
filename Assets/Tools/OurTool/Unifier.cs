using UnityEngine;
using System.Collections;
using OpenCVForUnity;
using Rect = UnityEngine.Rect;

public class Unifier:Photon.MonoBehaviour
{
	private bool PngCompression = true;

	//Optional, VideoChat will create these for you if you do not assign them
	public GameObject VideoObject;
	public AudioSource AudioIn;
	public AudioSource AudioOut;
	public bool AudioOut3D = false;

	//Adjust these as you wish for the custom needs of your particular application and platform
	private int ChunksPerFrame = 8;
	private int PacketsPerFrame = 16;
	private float PixelDeltaMagnitude = 0.002f;
	private int Width = 128;
	private int Height = 128;
	private int ChunkSize = 32;
	private int AudioFrequency = 6000;
	private int AudioPacketSize = 1500;
	private float AudioThreshold = 0.01f; //Remove echo for conversations without headphones
	private bool AcousticEchoCancellation = false;
	private float acousticDelay = 2f;
	private float acousticTimer = 0f;

	//Unity Networking stuff
	private float _clearTime;

	//Two network views for access to audio and video network groups separately
	private PhotonView _audioView;
	private PhotonView _videoView;
	private PhotonView _otherView;
	
	private bool _testMode = true;
	public bool CanTest = true;


	// Use this for initialization
	IEnumerator Start()
	{
		_audioView = gameObject.AddComponent<PhotonView>();
		_audioView.synchronization = ViewSynchronization.Off;
		_audioView.observed = this;
		_audioView.viewID = 3;

		_videoView = gameObject.AddComponent<PhotonView>();
		_videoView.synchronization = ViewSynchronization.Off;
		_videoView.observed = this;
		_videoView.viewID = 4;

		_otherView = gameObject.AddComponent<PhotonView>();
		_otherView.synchronization = ViewSynchronization.Off;
		_otherView.observed = this;
		_otherView.viewID = 5;

		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		//PhotonNetwork.sendRate = (int)(PacketsPerFrame + ((1 / Time.fixedDeltaTime) / 10));

		VideoChat.pngCompression = PngCompression;
		VideoChat.requestedWidth = Width;
		VideoChat.requestedHeight = Height;
		VideoChat.chunksPerFrame = ChunksPerFrame;
		VideoChat.packetsPerFrame = PacketsPerFrame;
		VideoChat.pixelDeltaMagnitude = PixelDeltaMagnitude;
		VideoChat.localView = true;

		VideoChat.audioPacketSize = AudioPacketSize;
		VideoChat.audioOut3D = AudioOut3D;
		VideoChat.audioFrequency = AudioFrequency;
		VideoChat.audioThreshold = AudioThreshold;
		VideoChat.acousticEchoCancellation = AcousticEchoCancellation;
		VideoChat.acousticDelay = acousticDelay;
		VideoChat.acousticTimer = acousticTimer;

		//Initialize to set base parameters such as the actual WebCamTexture height and width
		VideoChat.Init();

		//Add was created in case we need to defer the assignment of a videoObject until after it has been Network instantiated
		//In this example we are not doing network instantiation but if we were, this would come in handy
		VideoChat.Add(VideoObject, AudioIn, AudioOut);

		//Make some adjustments to the default video chat quad object for this demo, this assumes a Main Camera at the origin
		if(VideoObject)
			yield break;

		VideoChat.vcObject.transform.localScale *= 2f;
		VideoChat.vcObject.transform.position = new Vector3(0, -.64f, 5);

		FaceTracking.DrawRectToFaces = false;
		FaceTracking.LocalNeedsConversion = true;
		FaceTracking.PeerNeedsConversion = true;
		FaceTracking.DrawCropedFace = true;
	}

	// Update is called once per frame
	private void Update()
	{
		if(Input.GetKey(KeyCode.Escape))
			Application.Quit();

		//This is new in version 1.004, initializes things early for thumbnail
		VideoChat.PreVideo();

		if(!VideoChat.localViewTexture)
			return;

		if ((PhotonNetwork.room == null) || (PhotonNetwork.room != null && PhotonNetwork.otherPlayers.Length < 1))
			_testMode = CanTest;
		else
			_testMode = false;


		#region AUDIO

		//Collect source audio, this will create a new AudioPacket and add it to the audioPackets list in the VideoChat static class
		VideoChat.FromAudio();

		//Send the latest VideoChat audio packet for a local test or your networking library of choice, in this case Unity Networking
		var numPackets = VideoChat.audioPackets.Count;
		if(numPackets > 1)
		{

			var tempAudioPackets = new AudioPacket[numPackets];
			VideoChat.audioPackets.CopyTo(tempAudioPackets);

			for(var i = 0; i < numPackets; i++)
			{
				var currentPacket = tempAudioPackets[i];
				if(!_testMode)
				{
					_audioView.RPC("ReceiveAudio", PhotonTargets.Others, currentPacket.position, currentPacket.length,
						currentPacket.data); //Unity Networking
				}
				else
				{
					ReceiveAudio(currentPacket.position, currentPacket.length, currentPacket.data); //Test mode just plays back on one machine
				}
				VideoChat.audioPackets.Remove(tempAudioPackets[i]);
			}
		}

		#endregion

		#region Start VIDEO

		//Collect source video, this will create a new VideoPacket(s) and add it(them) to the videoPackets list in the VideoChat static class
		VideoChat.FromVideo();

		#endregion

		#region Face Tracking

		if(VideoChat.localViewTexture)
		{
			FaceTracking.LocalSourceImage = VideoChat.localViewTexture;
			if(FaceTracking.NewLocalFaceFound)
			{

				var face = FaceTracking.LocalFace;
				if(!_testMode)
				{
					_otherView.RPC("ReceiveFaceInformation", PhotonTargets.Others, face.x, face.y, face.width, face.height);
				}
				else
				{
					ReceiveFaceInformation(face.x, face.y, face.width, face.height);
				}
			}
		}


		#endregion

		#region Send Video

		numPackets = VideoChat.videoPackets.Count > VideoChat.packetsPerFrame
			? VideoChat.packetsPerFrame
			: VideoChat.videoPackets.Count;
		var tempVideoPackets = new VideoPacket[VideoChat.videoPackets.Count];
		VideoChat.videoPackets.CopyTo(tempVideoPackets);

		//Send the latest VideoChat video packets for a local test or your networking library of choice, in this case Unity Networking
		for(var i = 0; i < numPackets; i++)
		{
			var currentPacket = tempVideoPackets[i];
			if(!_testMode)
			{
				_videoView.RPC("ReceiveVideo", PhotonTargets.Others, currentPacket.x, currentPacket.y, currentPacket.data);
			}
			else
			{
				ReceiveVideo(currentPacket.x, currentPacket.y, currentPacket.data);
			}

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
	[RPC]
	void ReceiveFaceInformation(float x, float y, float width, float height)
	{
		FaceTracking.PeerFace = new Rect(x,y,width,height);
	}
}


