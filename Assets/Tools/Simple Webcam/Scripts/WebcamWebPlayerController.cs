using UnityEngine;
using System.Collections;

public class WebcamWebPlayerController : MonoBehaviour {

	IEnumerator Start() 
	{
		yield return Application.RequestUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone);
		if (Application.HasUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone)) 
		{
			WebcamWebPlayer[] wcs = GameObject.FindObjectsOfType<WebcamWebPlayer>();
			foreach(WebcamWebPlayer a in wcs)
			{
				a.startWebcam();
			}
		} 
	}
}
