using System;
using UnityEngine;
using System.Collections;
using OpenCVForUnity;
using Rect = UnityEngine.Rect;

public class PeerFace:MonoBehaviour
{
	private bool _init = false;
	private Mat _imgMat;
	private int _offset = 10;

	private void Init(Texture2D texture)
	{
		_init = true;
		_imgMat = new Mat(texture.height, texture.width, CvType.CV_8UC4);
	}
	private void Update()
	{
		if(!VideoChat.networkTexture)
			return;

		FaceTracking.PeerSourceImage = VideoChat.networkTexture;

		var tmpTexture = FaceTracking.DrawCropedFace ?
			FaceTracking.PeerCropedImage :
			FaceTracking.PeerSourceImage;

		if(FaceTracking.DrawRectToFaces)
		{
			if(!_init)
			{
				Init(tmpTexture);
			}
			Utils.texture2DToMat(tmpTexture, _imgMat);
			var face = FaceTracking.PeerFace;

			Core.rectangle(_imgMat, new Point(face.x, face.y),
				new Point(face.x + face.width, face.y + face.height),
				new Scalar(255, 0, 0, 255), 1);
			Core.rectangle(_imgMat, new Point(face.x - _offset, face.y - _offset),
				new Point(face.x + face.width + _offset, face.y + face.height + _offset),
				new Scalar(0, 255, 0, 255), 1);

			Utils.matToTexture2D(_imgMat, tmpTexture);
		}


		renderer.material.SetTexture("_MainTex", tmpTexture);
	}
}
