using UnityEngine;
using System.Collections;

public class LocalView : MonoBehaviour {

	public bool localView;

	void Update () {
		VideoChat.localView = localView;
		if( VideoChat.localView && renderer.material.GetTexture( "_MainTex" ) != VideoChat.localViewTexture )
			renderer.material.SetTexture( "_MainTex", VideoChat.localViewTexture );
		
		//This requires a shader that enables texture rotation, you can use the supplied CameraView material
		//or use a new material that also uses the UnlitRotatableTexture shader if you're already using the
		//CameraView material for another object
		if( VideoChat.webCamTexture != null ) {
			Quaternion rot = Quaternion.Euler( 0, 0, VideoChat.webCamTexture.videoRotationAngle );
   			Matrix4x4 m = Matrix4x4.TRS( Vector3.zero, rot, new Vector3( 1, 1, 1 ) );
			renderer.material.SetMatrix( "_Rotation", m ); 
		}
	}
}
