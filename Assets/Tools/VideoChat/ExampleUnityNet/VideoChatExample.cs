using UnityEngine;
using System.Collections;

public class VideoChatExample : MonoBehaviour {
	
	public bool LAN;
	public bool testMode;
	public bool pngCompression;
	
	//Optional, VideoChat will create these for you if you do not assign them
	public GameObject videoObject;
	public Material   videoViewMaterial;
	
	public AudioSource audioIn;
	public AudioSource audioOut;
	public bool        audioOut3D;
	
	//Adjust these as you wish for the custom needs of your particular application and platform
	public int chunksPerFrame = 10;
	public int packetsPerFrame = 10;
	public float pixelDeltaMagnitude = 0.002f;
	public float audioThreshold = 0.1f; //Remove echo for conversations without headphones
	public int audioFrequency = 5000;
	public bool acousticEchoCancellation = false;
	public int width = 64;
	public int height = 64;
	public int chunkSize = 16;
	
	private string stringWidth = "";
	private string stringHeight = "";
	private string stringChunkSize = "";	

	//Unity Networking stuff
	private HostData[]    hostData;
	private float         clearTime;
	
	//Two network views for access to audio and video network groups separately
	private NetworkView   audioView; 
	private NetworkView   videoView;
	
	// Use this for initialization
	IEnumerator Start() {

		if( Application.isWebPlayer ) {
			yield return Application.RequestUserAuthorization( UserAuthorization.WebCam | UserAuthorization.Microphone );
			LAN = false;
		}

		Screen.sleepTimeout = SleepTimeout.NeverSleep;		

		stringWidth = width + "";
		stringHeight = height + "";
		stringChunkSize = chunkSize + "";

		VideoChat.pngCompression  = pngCompression;
		VideoChat.requestedWidth  = width;
		VideoChat.requestedHeight = height;
		VideoChat.audioOut3D      = audioOut3D;
		VideoChat.acousticEchoCancellation = acousticEchoCancellation;
		VideoChat.audioFrequency = audioFrequency;	
		VideoChat.cameraView = videoViewMaterial;
		//Initialize to set base parameters such as the actual WebCamTexture height and width
		VideoChat.Init();
		
		//Add was created in case we need to defer the assignment of a videoObject until after it has been Network instantiated
		//In this example we are not doing network instantiation but if we were, this would come in handy
		VideoChat.Add( videoObject, audioIn, audioOut );
		
		//Make some adjustments to the default video chat quad object for this demo, this assumes a Main Camera at the origin
		if( !videoObject ) {
			VideoChat.vcObject.transform.localScale *= 1.5f;
			VideoChat.vcObject.transform.position = new Vector3( 0, -1.4f, 5 );
		}

		if( LAN )
			LANParty.Init( "MidnightVideoChat", 1 );
		else {
			MasterServer.ClearHostList();
			MasterServer.RequestHostList( "MidnightVideoChat" );
			hostData = MasterServer.PollHostList();
		}

		audioView = gameObject.AddComponent< NetworkView >();
		audioView.stateSynchronization = NetworkStateSynchronization.Off;
		audioView.group = VideoChat.audioGroup;
		//audioView.observed = null;

		videoView = gameObject.AddComponent< NetworkView >();
		videoView.stateSynchronization = NetworkStateSynchronization.Off;
		videoView.group = VideoChat.videoGroup;	
		//videoView.observed = null;
	}
	
	IEnumerator DelayedConnection() {
		yield return new WaitForSeconds( 2.0f );
		
		string connectionResult = "";
		if( LAN )
			connectionResult = "" + Network.Connect( LANParty.serverIPAddress, 2301 );
		else { 
			if( hostData.Length > 0 ) {
		 		connectionResult = "" + Network.Connect( hostData[ 0 ] );
				LANParty.log += "\nConnect to Server @ " + hostData[ 0 ].gameName + " " + hostData[ 0 ].gameType + " " + hostData[ 0 ].guid + " " + hostData[ 0 ].ip + " " + hostData[ 0 ].useNat + " " + connectionResult;
			}
			else
				connectionResult = "";
		}
		if( connectionResult == "IncorrectParameters" || connectionResult == "" )
			StartCoroutine( "Restart" );
	}	

	void OnApplicationQuit() {
		LANParty.End();
	}
	
	void OnDisconnectedFromServer() {
		//StartCoroutine( "Restart" );
		DelayedConnection();	
	}
	
	IEnumerator Restart() {
		
		if( LAN )
			LANParty.End();
		else {
			if( Network.peerType == NetworkPeerType.Server )
				MasterServer.UnregisterHost();	
		}

		yield return new WaitForSeconds( 1.0f );

		
		if( Network.peerType != NetworkPeerType.Disconnected ) {
			Network.Disconnect();
			LANParty.log += "\nDisconnected";
		}	
		
		Resources.UnloadUnusedAssets();
		Application.LoadLevel( Application.loadedLevel );
	}
	
	void OnGUI () {
		if( !VideoChat.tempImage )
			return;
		GUI.Label( new Rect( 0, 0, Screen.width, Screen.height ), VideoChat.log );
		if( Network.peerType == NetworkPeerType.Disconnected ) {
			bool oldTestMode = testMode;
			testMode = GUI.Toggle( new Rect( 0, 20, Screen.width / 3, 40 ), testMode, "Test Mode" );
			if( testMode == true && oldTestMode == false ) {
				VideoChat.chunkSize = ( stringChunkSize == "" ? 0 : System.Convert.ToInt32( stringChunkSize ) );
				if( width >= VideoChat.chunkSize && height >= VideoChat.chunkSize ) {
					VideoChat.requestedWidth = width;
					VideoChat.requestedHeight = height;
				}
				VideoChat.deviceIndex++;
			}
			
			if( !Application.isWebPlayer ) {
				bool oldLAN = LAN;
				LAN = GUI.Toggle( new Rect( Screen.width / 3, 20, Screen.width / 3, 40 ), LAN, "LAN" );
				if( LAN != oldLAN ) {
					LANParty.End();
					LANParty.Init( "MidnightVideoChat", 1 );
				}
			}
			
			GUI.Label( new Rect( 0, 40, Screen.width / 3, 20 ), "Width" );
			GUI.Label( new Rect( Screen.width / 3, 40, Screen.width / 3, 20 ), "Height" );
			GUI.Label( new Rect( 2 * Screen.width / 3, 40, Screen.width / 3, 20 ), "Chunk Size" );
			stringWidth = GUI.TextField( new Rect( 0, 60, Screen.width / 3, 40 ), stringWidth );
			stringHeight = GUI.TextField( new Rect( Screen.width / 3, 60, Screen.width / 3, 40 ), stringHeight );
			stringChunkSize = GUI.TextField( new Rect( 2 * Screen.width / 3, 60, Screen.width / 3, 40 ), stringChunkSize );			

			width = ( stringWidth == "" ? 0 : System.Convert.ToInt32( stringWidth ) );
			height = ( stringHeight == "" ? 0 : System.Convert.ToInt32( stringHeight ) );										

			if( testMode ) {
				if( GUI.Button( new Rect( 0, 100, Screen.width, 40 ), "Change Camera" ) ) {
					VideoChat.chunkSize = ( stringChunkSize == "" ? 0 : System.Convert.ToInt32( stringChunkSize ) );
					if( width >= VideoChat.chunkSize && height >= VideoChat.chunkSize ) {
						VideoChat.requestedWidth = width;
						VideoChat.requestedHeight = height;
					}
					VideoChat.deviceIndex++;
				}
				//Added some sliders to toy with performance settings in real time
				GUI.Label( new Rect( 0, 160, Screen.width, 40 ), "Chunks per Frame " + chunksPerFrame );
				chunksPerFrame = System.Convert.ToInt16( GUI.HorizontalSlider( new Rect( 0, 140, Screen.width, 40 ), chunksPerFrame, 0.0f, 512 ) );
				GUI.Label( new Rect( 0, 220, Screen.width, 40 ), "Packets per Frame " + packetsPerFrame );
				packetsPerFrame = System.Convert.ToInt16( GUI.HorizontalSlider( new Rect( 0, 200, Screen.width, 40 ), packetsPerFrame, 0.0f, 512 ) );
				GUI.Label( new Rect( 0, 280, Screen.width, 40 ), "Pixel Delta Magnitude " + pixelDeltaMagnitude );
				pixelDeltaMagnitude = GUI.HorizontalSlider( new Rect( 0, 260, Screen.width, 40 ), pixelDeltaMagnitude, 0.0f, 0.01f );	
				GUI.Label( new Rect( 0, 320, Screen.width, 40 ), "Audio Threshold " + audioThreshold );
				audioThreshold = GUI.HorizontalSlider( new Rect( 0, 340, Screen.width, 40 ), audioThreshold, 0.0f, 1.0f );
				VideoChat.acousticEchoCancellation = GUI.Toggle( new Rect( 0, 380, Screen.width / 3, 40 ), VideoChat.acousticEchoCancellation, "Echo Cancellation" );
				return;
			}
			if( GUI.Button( new Rect( 0, 140, Screen.width, 40 ), "Start" ) ) {
				VideoChat.chunkSize = ( stringChunkSize == "" ? 0 : System.Convert.ToInt32( stringChunkSize ) );
				if( width >= VideoChat.chunkSize && height >= VideoChat.chunkSize ) {
					VideoChat.requestedWidth = width;
					VideoChat.requestedHeight = height;
				}
				VideoChat.deviceIndex++;
				if( LAN ) {
					LANParty.peerType = "server";
					LANParty.possibleConnections = 1;
					LANParty.log += "\n" + Network.InitializeServer( LANParty.possibleConnections, 2301, false );
				} else {
					Network.InitializeServer( 1, 2301, true );
					MasterServer.RegisterHost( "MidnightVideoChat", "Test" );
				}
			}	
			
			if( GUI.Button( new Rect( 0, 180, Screen.width, 40 ), "Join" ) && LANParty.peerType == "" ) {
				VideoChat.chunkSize = ( stringChunkSize == "" ? 0 : System.Convert.ToInt32( stringChunkSize ) );
				if( width >= VideoChat.chunkSize && height >= VideoChat.chunkSize ) {
					VideoChat.requestedWidth = width;
					VideoChat.requestedHeight = height;
				}
				VideoChat.deviceIndex++;
				if( LAN ) {
					LANParty.peerType = "client";
					LANParty.Broadcast( LANParty.ipRequestString + LANParty.gameName );
				} else {
					if( hostData.Length == 0 )
						hostData = MasterServer.PollHostList();
				}
				StartCoroutine( "DelayedConnection" );
			}
			if( GUI.Button( new Rect( 0, 220, Screen.width, 40 ), "Refresh Master Server" ) ) {
				MasterServer.ClearHostList();
    				MasterServer.RequestHostList( "MidnightVideoChat" );
				hostData = MasterServer.PollHostList();
			}
		} else {
			if( GUI.Button( new Rect( 0, 20, Screen.width, 40 ), "Disconnect" ) )
				StartCoroutine( "Restart" );
			if( GUI.Button( new Rect( 0, 60, Screen.width, 40 ), "Change Camera" ) ) {
				VideoChat.chunkSize = ( stringChunkSize == "" ? 0 : System.Convert.ToInt32( stringChunkSize ) );
				if( width >= VideoChat.chunkSize && height >= VideoChat.chunkSize ) {
					VideoChat.requestedWidth = width;
					VideoChat.requestedHeight = height;
				}
				VideoChat.deviceIndex++;
			}
			//Added some sliders to toy with performance settings in real time
			GUI.Label( new Rect( 0, 120, Screen.width, 40 ), "Chunks per Frame " + chunksPerFrame );
			chunksPerFrame = System.Convert.ToInt16( GUI.HorizontalSlider( new Rect( 0, 140, Screen.width, 40 ), chunksPerFrame, 0.0f, 100 ) );
			GUI.Label( new Rect( 0, 160, Screen.width, 40 ), "Packets per Frame " + packetsPerFrame );
			packetsPerFrame = System.Convert.ToInt16( GUI.HorizontalSlider( new Rect( 0, 180, Screen.width, 40 ), packetsPerFrame, 0.0f, 100 ) );
			GUI.Label( new Rect( 0, 200, Screen.width, 40 ), "Pixel Delta Magnitude " + pixelDeltaMagnitude );
			pixelDeltaMagnitude = GUI.HorizontalSlider( new Rect( 0, 220, Screen.width, 40 ), pixelDeltaMagnitude, 0.0f, 0.01f );	
			GUI.Label( new Rect( 0, 240, Screen.width, 40 ), "Audio Threshold " + audioThreshold );
			audioThreshold = GUI.HorizontalSlider( new Rect( 0, 260, Screen.width, 40 ), audioThreshold, 0.0f, 1.0f );
			VideoChat.acousticEchoCancellation = GUI.Toggle( new Rect( 0, 300, Screen.width / 3, 40 ), VideoChat.acousticEchoCancellation, "Echo Cancellation" );
		}		
	}
	
	void Update() {
		if( Input.GetKey( KeyCode.Escape ) )
			Application.Quit();
		
		//This is new in version 1.004, initializes things early for thumbnail
		VideoChat.PreVideo();

		if( ( !testMode && Network.peerType == NetworkPeerType.Disconnected ) || ( Network.peerType != NetworkPeerType.Disconnected && Network.connections.Length < 1 ) )
			return;
		
		#region AUDIO
		VideoChat.audioThreshold = audioThreshold;

		//Collect source audio, this will create a new AudioPacket and add it to the audioPackets list in the VideoChat static class
		VideoChat.FromAudio();		

		//Send the latest VideoChat audio packet for a local test or your networking library of choice, in this case Unity Networking
		int numPackets = VideoChat.audioPackets.Count;				
		AudioPacket[] tempAudioPackets = new AudioPacket[ numPackets ];
		VideoChat.audioPackets.CopyTo( tempAudioPackets );
		
		for( int i = 0; i < numPackets; i++ ) {
			AudioPacket currentPacket = tempAudioPackets[ i ]; 
			
			if( testMode )
				ReceiveAudio( currentPacket.position, currentPacket.length, currentPacket.data ); //Test mode just plays back on one machine
			else
				audioView.RPC( "ReceiveAudio", RPCMode.Others, currentPacket.position, currentPacket.length, currentPacket.data ); //Unity Networking
			
			VideoChat.audioPackets.Remove( tempAudioPackets[ i ] );
		}
		#endregion
		

		#region VIDEO
		VideoChat.chunksPerFrame = chunksPerFrame;
		
		Network.sendRate = packetsPerFrame + ( ( 1 / Time.fixedDeltaTime ) / 10 );
		VideoChat.packetsPerFrame = packetsPerFrame;

		VideoChat.pixelDeltaMagnitude = pixelDeltaMagnitude;
		
		//Collect source video, this will create a new VideoPacket(s) and add it(them) to the videoPackets list in the VideoChat static class
		VideoChat.FromVideo();
	
		numPackets = VideoChat.videoPackets.Count > VideoChat.packetsPerFrame ? VideoChat.packetsPerFrame : VideoChat.videoPackets.Count;				
		VideoPacket[] tempVideoPackets = new VideoPacket[ VideoChat.videoPackets.Count ];
		VideoChat.videoPackets.CopyTo( tempVideoPackets );		
		
		//Send the latest VideoChat video packets for a local test or your networking library of choice, in this case Unity Networking
		for( int i = 0; i < numPackets; i++ ) {
			VideoPacket currentPacket = tempVideoPackets[ i ];

			if( testMode )
				ReceiveVideo( currentPacket.x, currentPacket.y, currentPacket.data ); //Test mode just displays on one machine
			else
				videoView.RPC( "ReceiveVideo", RPCMode.Others, currentPacket.x, currentPacket.y, currentPacket.data ); //Unity Networking
		
			VideoChat.videoPackets.Remove( tempVideoPackets[ i ] );
		}
		#endregion
	}
	
	[RPC]
	void ReceiveVideo( int x, int y, byte[] videoData ) {
		if( videoData.Length == 1 && Network.connections.Length > 0 ) {
			for( int i = 0; i < Network.connections.Length; i++ )
				Network.RemoveRPCs( Network.connections[ 0 ], VideoChat.videoGroup );
		}
		VideoChat.ToVideo( x, y, videoData );
	}
	
	[RPC]
	void ReceiveAudio( int micPosition, int length, byte[] audioData ) {
		if( micPosition == 0 && Network.connections.Length > 0 ) {
			for( int i = 0; i < Network.connections.Length; i++ )
				Network.RemoveRPCs( Network.connections[ i ], VideoChat.audioGroup );
		}
		VideoChat.ToAudio( micPosition, length, audioData );
	}

	void OnPlayerDisconnected( NetworkPlayer player ) {
        	Network.RemoveRPCsInGroup( VideoChat.videoGroup );
		Network.RemoveRPCsInGroup( VideoChat.audioGroup );
		Network.RemoveRPCs( player );
		VideoChat.deviceIndex = VideoChat.deviceIndex; //This resets the camera to prepare for a new connection
    	}
	
	void OnPlayerConnected( NetworkPlayer player ) {
		Network.RemoveRPCsInGroup( VideoChat.videoGroup );
		Network.RemoveRPCsInGroup( VideoChat.audioGroup );
		VideoChat.deviceIndex = VideoChat.deviceIndex; //This resets the camera to prepare for a new connection

	}
}
