using UnityEngine;
using System.Collections;

public class DEMOSceneSelecter : MonoBehaviour {

	void OnGUI()
	{
		if(GUI.Button(new Rect(10,10,200,60),"Scene 1"))
		{
			Application.LoadLevel("DemoSceneWebPlayer 2");
		}
		if(GUI.Button(new Rect(10,70,200,60),"Scene 2"))
		{
			Application.LoadLevel("DemoSceneWebPlayer");
		}
	}
}
