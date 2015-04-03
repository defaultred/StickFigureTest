using UnityEngine;
using System.Collections;
//using TNet; //Must have TNet!

public class VideoChatExampleTNet : MonoBehaviour {
	public bool   testMode;
	public bool   LAN; //TNet's LAN stuff only works with Universal Plug & Play Devices, i.e. no Apple Airport. LANParty could still be used to setup the remoteIPAddress for a standard TNet connection (see the UnityNet example)
	public string remoteIPAddress;
	public bool   pngCompression;

	//TNet stuff
	public int serverTcpPort = 5127;
	string mAddress = "127.0.0.1";
	string mMessage = "";
	float mAlpha = 0f;
	public GUIStyle button;
	public GUIStyle text;
	public GUIStyle input;

	//Optional, VideoChat will create these for you if you do not assign them
	public GameObject videoObject;
	public AudioSource audioIn;
	public AudioSource audioOut;
	public bool        audioOut3D;
	
	//Adjust these as you wish for the custom needs of your particular application and platform
	public int chunksPerFrame = 10;
	public int packetsPerFrame = 10;
	public float pixelDeltaMagnitude = 0.0015f;
	public float audioThreshold = 0.1f; //Anything louder than this will be picked up by the mid
	public int audioFrequency = 5000;
	public bool acousticEchoCancellation = false; //Make this true to allow echo cancellation, in test mode this will cancel your own voice
	public int width = 64;
	public int height = 64;
	public int chunkSize = 16;
	
	private string stringWidth = "64";
	private string stringHeight = "64";
	private string stringChunkSize = "16";
	
	
	//Comment out this Update and uncomment the rest once TNet is imported, don't forget to uncommment "using TNet" up top!
	void Update() {
		Debug.LogError( "Install TNet and uncomment to use!" );
	}

	/*
	//If you use LANParty() with TNet (not provided by default) then ensure this is included
	void OnApplicationQuit() {
		LANParty.End();
	}
	*/

	/*
	public TNObject tno;

	// Use this for initialization, Remember TNet won't work with the Web Player
	void Start() {
		tno = gameObject.AddComponent( typeof( TNObject ) ) as TNObject;
		tno.id = 1; //make id public in TNObject.cs

		gameObject.AddComponent( typeof( TNManager ) );
		gameObject.AddComponent( typeof( TNUdpLobbyClient ) );

		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		TNManager.StartUDP( Random.Range( 10000, 50000 ) );

		stringWidth = width + "";
		stringHeight = height + "";
		stringChunkSize = chunkSize + "";

		VideoChat.pngCompression  = pngCompression;
		VideoChat.requestedWidth  = width;
		VideoChat.requestedHeight = height;
		VideoChat.audioOut3D      = audioOut3D;	
		VideoChat.acousticEchoCancellation = acousticEchoCancellation;
		VideoChat.audioFrequency = audioFrequency;

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
	}
	
	IEnumerator Restart() {
		testMode = false;
		TNManager.Disconnect();
		
		yield return new WaitForSeconds( 0.1f );
		
		//Resources.UnloadUnusedAssets();
		//Application.LoadLevel( Application.loadedLevel );
	}
	
	void OnGUI () {
		if( !VideoChat.tempImage )
			return;

		GUI.Label( new Rect( 0, 0, Screen.width, Screen.height ), VideoChat.log );
		if( !TNManager.isConnected && !testMode ) {
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

			LAN = GUI.Toggle( new Rect( Screen.width / 3, 20, Screen.width / 3, 40 ), LAN, "LAN" );			

			
			
			GUI.Label( new Rect( 0, 40, Screen.width / 3, 20 ), "Width" );
			GUI.Label( new Rect( Screen.width / 3, 40, Screen.width / 3, 20 ), "Height" );
			GUI.Label( new Rect( 2 * Screen.width / 3, 40, Screen.width / 3, 20 ), "Chunk Size" );
			stringWidth = GUI.TextField( new Rect( 0, 60, Screen.width / 3, 40 ), stringWidth );
			stringHeight = GUI.TextField( new Rect( Screen.width / 3, 60, Screen.width / 3, 40 ), stringHeight );
			stringChunkSize = GUI.TextField( new Rect( 2 * Screen.width / 3, 60, Screen.width / 3, 40 ), stringChunkSize );			
			
			GUI.Label( new Rect( 0, 100, Screen.width / 2, 20 ), "Remote IP Address" );
			remoteIPAddress = GUI.TextField( new Rect( Screen.width / 2, 100, Screen.width / 2, 40 ), remoteIPAddress );

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
				
				int udpPort = Random.Range(10000, 40000);

				// Start a local server, loading the saved data if possible
				// The UDP port of the server doesn't matter much as it's optional,
				// and the clients get notified of it via Packet.ResponseSetUDP.
				TNUdpLobbyClient tNetLan = GetComponent<TNUdpLobbyClient>();
				int lobbyPort = ( tNetLan != null ) ? tNetLan.remotePort : 0;
				TNServerInstance.Start( serverTcpPort, udpPort, "server.dat", lobbyPort );
				TNManager.Connect( "127.0.0.1" );
			}	
			
			if( GUI.Button( new Rect( 0, 180, Screen.width, 40 ), "Join" ) ) {
				VideoChat.chunkSize = ( stringChunkSize == "" ? 0 : System.Convert.ToInt32( stringChunkSize ) );
				if( width >= VideoChat.chunkSize && height >= VideoChat.chunkSize ) {
					VideoChat.requestedWidth = width;
					VideoChat.requestedHeight = height;
				}
				VideoChat.deviceIndex++;
				if( LAN ) {
					List<ServerList.Entry> list = TNLobbyClient.knownServers.list;
					// Server list example script automatically collects servers that have recently announced themselves
					for( int i = 0; i < list.size; ++i ) {
						ServerList.Entry ent = list[i];
						if( GUILayout.Button(ent.externalAddress.ToString(), button ) ) {
							TNManager.Connect( ent.externalAddress, ent.internalAddress );
						}
					}
				} else {
					TNManager.Connect( remoteIPAddress );
				}
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
	

	// Process video in Update
	void Update() {
		if( Input.GetKey( KeyCode.Escape ) )
			Application.Quit();
		
		if( Application.isPlaying )
		{
			float target = ( TNLobbyClient.knownServers.list.size == 0 ) ? 0f : 1f;
			mAlpha = UnityTools.SpringLerp( mAlpha, target, 8f, Time.deltaTime) ;
		}

		VideoChat.PreVideo();		

		if( !TNManager.isConnected && !testMode ) {
			//Debug.Log( "TNManager is not connected" );
			return;	
		}	
		
		//Collect source audio, this will create a new AudioPacket and add it to the audioPackets list in the VideoChat static class
		VideoChat.FromAudio();		

		//Send the latest VideoChat audio packet for a local test or your networking library of choice, in this case TNet Networking
		AudioPacket currentAudioPacket = VideoChat.audioPackets[ VideoChat.audioPackets.Count - 1 ]; 
		if( testMode )
			ReceiveAudio( currentAudioPacket.position, currentAudioPacket.length, currentAudioPacket.data ); //Test mode just plays back on one machine
		else {
			if( currentAudioPacket.data.Length < 10 )
				tno.Send( "ReceiveAudio", Target.Others, currentAudioPacket.position, currentAudioPacket.length, currentAudioPacket.data );
			else
				tno.SendQuickly( "ReceiveAudio", Target.Others, currentAudioPacket.position, currentAudioPacket.length, currentAudioPacket.data );
		}
		
		VideoChat.audioPackets.Clear();

		VideoChat.chunksPerFrame = chunksPerFrame;
		
		VideoChat.packetsPerFrame = packetsPerFrame;

		VideoChat.pixelDeltaMagnitude = pixelDeltaMagnitude / 100000.0f;

		VideoChat.audioThreshold = audioThreshold;
		
		//Collect source video, this will create a new VideoPacket(s) and add it(them) to the videoPackets list in the VideoChat static class
		VideoChat.FromVideo();
	
		int numPackets = VideoChat.videoPackets.Count > VideoChat.packetsPerFrame ? VideoChat.packetsPerFrame : VideoChat.videoPackets.Count;				
		VideoPacket[] tempVideoPackets = new VideoPacket[ VideoChat.videoPackets.Count ];
		VideoChat.videoPackets.CopyTo( tempVideoPackets );		
		
		//Send the latest VideoChat video packets for a local test or your networking library of choice, in this case TNet Networking
		for( int i = 0; i < numPackets; i++ ) {
			VideoPacket currentPacket = tempVideoPackets[ i ];

			if( testMode )
				ReceiveVideo( currentPacket.x, currentPacket.y, currentPacket.data ); //Test mode just displays on one machine
			else {
				if( currentPacket.data.Length < 10 )
					tno.Send( "ReceiveVideo", Target.Others, currentPacket.x, currentPacket.y, currentPacket.data );
				else
					tno.SendQuickly( "ReceiveVideo", Target.Others, currentPacket.x, currentPacket.y, currentPacket.data );
			}
		
			VideoChat.videoPackets.Remove( tempVideoPackets[ i ] );
		}
	}

	[RFC] void ReceiveAudio( int micPosition, int length, byte[] audioData ) {
		VideoChat.ToAudio( micPosition, length, audioData );
	}

	[RFC] void ReceiveVideo( int x, int y, byte[] videoData ) {
		VideoChat.ToVideo( x, y, videoData );
	}

	void OnNetworkConnect( bool success, string message ) {
		Debug.Log( "OnNetworkConnect " + success + " " + message + " " + TNManager.isHosting + " " + TNManager.isTryingToConnect );
        	TNManager.JoinChannel( 0, null );
	}

	void OnNetworkPlayerJoin(Player newPlayer) {
		Debug.Log( "Player joined " + newPlayer.id + " " + newPlayer.name );
		VideoChat.deviceIndex = VideoChat.deviceIndex; //This resets the camera to prepare for a new connection
	}

	void OnNetworkJoinChannel( bool success, string message ) {
    	    Debug.Log("Joining Network Channel: Success = " + success + " Message:" + message);
    }
	*/
}
