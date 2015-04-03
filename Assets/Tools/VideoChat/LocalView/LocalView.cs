using UnityEngine;
using System.Collections;
using OpenCVForUnity;
using Rect = UnityEngine.Rect;

public class LocalView:MonoBehaviour
{

	private bool _init = false;
	private CascadeClassifier _cascade;

	private void Init()
	{
		_init = true;
	}
	private void Update()
	{
		VideoChat.localView = true;
		if(VideoChat.localView && renderer.material.GetTexture("_MainTex") != VideoChat.localViewTexture)
		{
			var tmpTexture = VideoChat.localViewTexture;

//			renderer.material.SetTexture("_MainTex", tmpTexture);
			var rt = new RenderTexture(tmpTexture.width, tmpTexture.height,24);

			Graphics.Blit(tmpTexture,rt);

			var camTexture = new Texture2D(tmpTexture.width, tmpTexture.height);

			RenderTexture.active = rt;
			camTexture.ReadPixels(new Rect(0,0,tmpTexture.width,tmpTexture.height), 0,0);
			camTexture.Apply();


			Mat imgMat = new Mat(camTexture.height, camTexture.width, CvType.CV_8UC4);

			Utils.texture2DToMat(camTexture, imgMat);
//			Debug.Log("imgMat dst ToString " + imgMat.ToString());

			Mat grayMat = new Mat();
			Imgproc.cvtColor(imgMat, grayMat, Imgproc.COLOR_RGBA2GRAY);
			Imgproc.equalizeHist(grayMat, grayMat);


			MatOfRect faces = new MatOfRect();

			if(_cascade != null)
				_cascade.detectMultiScale(grayMat, faces, 1.1, 2, 2,
						   new Size(20, 20), new Size());

			OpenCVForUnity.Rect[] rects = faces.toArray();
			foreach(var rect in rects)
			{
				Debug.Log("detect faces " + rect);

				Core.rectangle(imgMat, new Point(rect.x, rect.y), new Point(rect.x + rect.width, rect.y + rect.height), new Scalar(255, 0, 0, 255), 1);
				Core.rectangle(imgMat, new Point(rect.x-10, rect.y-10), new Point(rect.x + rect.width+10, rect.y + rect.height+10), new Scalar(0, 255, 0, 255), 1);
			}



			var texture = new Texture2D(imgMat.cols(), imgMat.rows(), TextureFormat.RGBA32, false);

			Utils.matToTexture2D(imgMat, texture);

//			gameObject.GetComponent<Renderer>().material.mainTexture = texture;


			renderer.material.SetTexture("_MainTex", texture);
		}

		//This requires a shader that enables texture rotation, you can use the supplied CameraView material
		//or use a new material that also uses the UnlitRotatableTexture shader if you're already using the
		//CameraView material for another object
		//if(VideoChat.webCamTexture != null)
		//{
		//	Quaternion rot = Quaternion.Euler(0, 0, VideoChat.webCamTexture.videoRotationAngle);
		//	Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, rot, new Vector3(1, 1, 1));
		//	renderer.material.SetMatrix("_Rotation", m);
		//}
	}
}
