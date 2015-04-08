using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Webcam))]
public class WebcamEditor : Editor  
{

	public override void OnInspectorGUI()
	{
		Webcam myTarget = (Webcam)target;
		EditorGUILayout.Separator();
		myTarget.getDeviceByName = EditorGUILayout.Toggle("Device Index/Name: ",myTarget.getDeviceByName);
		if(myTarget.getDeviceByName)
		{
			myTarget.deviceName = EditorGUILayout.TextField("Device Name: ",myTarget.deviceName);
		}
		else
		{
			myTarget.deviceIndex = EditorGUILayout.IntField("Device Index: ",myTarget.deviceIndex);
		}
		EditorGUILayout.Separator();
		myTarget.setRequestResolution = EditorGUILayout.Toggle("Manual Resolution: ",myTarget.setRequestResolution);
		if(myTarget.setRequestResolution)
		{
			myTarget.resolutionWidth = EditorGUILayout.IntField("Width: ",myTarget.resolutionWidth);
			myTarget.resolutionHeight = EditorGUILayout.IntField("Height: ",myTarget.resolutionHeight);
		}
		EditorGUILayout.Separator();
		myTarget.setFPS = EditorGUILayout.Toggle("Manual FPS: ",myTarget.setFPS);
		if(myTarget.setFPS)
		{
			myTarget.requestedFPS = EditorGUILayout.IntField("Requested FPS: ",myTarget.requestedFPS);
		}
		EditorGUILayout.Separator();
		myTarget.mirrorHorizontal = EditorGUILayout.Toggle("Mirror Horizontal: ",myTarget.mirrorHorizontal);
	}
}
