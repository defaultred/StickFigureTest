using System;
using System.Linq.Expressions;
using UnityEngine;
using OpenCVForUnity;
using Rect = UnityEngine.Rect;


public static class FaceTracking
{

	private const int FaceNotfoundLimit = 200;

	private static bool _drawRectToFaces;

	private static bool _localFaceFound;
	private static bool _newLocalFaceFound;
	private static bool _localNeedsConversion;
	private static Texture2D _localSourceImage;
	private static Texture2D _localCropedImage;
	private static Rect _localFace;
	private static Rect _lastSentLocalFace;
	private static int _faceNotFoundTracker;

	private static bool _peerFaceFound;
	private static bool _peerNeedsConversion;
	private static Texture2D _peerSourceImage;
	private static Texture2D _peerCropedImage;
	private static Rect _peerFace;

	private static Mat _imgMat;
	private static Mat _grayMat;
	private static MatOfRect _matFaces;
	private static bool _init = false;
	private static CascadeClassifier _cascade;

	private const int MaximumCenterDistance = 5;
	private const int MaximumSizeDelta = 8;

	private static int _rowCounter;
	private static int _colorType;

	public static bool DrawRectToFaces
	{
		set;
		get;
	}
	public static bool DrawCropedFace
	{
		get;
		set;
	}
	public static bool LocalFaceFound
	{
		get
		{
			return _localFaceFound;
		}
	}
	public static bool NewLocalFaceFound
	{
		get
		{
			if(!_newLocalFaceFound)
				return _newLocalFaceFound;

			_newLocalFaceFound = false;
			return true;
		}
	}
	public static bool LocalNeedsConversion
	{
		set
		{
			_localNeedsConversion = value;
		}
	}
	public static Texture2D LocalSourceImage
	{
		get
		{
			return _localSourceImage;
		}
		set
		{
			_localSourceImage = value;
			_localFaceFound = false;

			if(!_init)
			{
				_init = true;
				_imgMat = new Mat(_localSourceImage.height, _localSourceImage.width, CvType.CV_8UC4);
				_grayMat = new Mat(_localSourceImage.height, _localSourceImage.width, CvType.CV_8UC1);
				_matFaces = new MatOfRect();
				_cascade = new CascadeClassifier(Utils.getFilePath("haarcascade_frontalface_alt.xml"));
			}

			if(_localNeedsConversion)
			{
				_localSourceImage = ConvertImage(_localSourceImage);
			}

			_localFaceFound = FindFace(_localSourceImage, out _localFace);

			if(!_localFaceFound)
			{
				_faceNotFoundTracker++;
			}
			else
			{
				_faceNotFoundTracker = 0;
				_newLocalFaceFound = !FaceDeltaAcceptable(_localFace, ref _lastSentLocalFace);

				if(DrawCropedFace)
				{
					_localCropedImage = CropFace(_localSourceImage, LocalFace);
				}
			}

			if(_faceNotFoundTracker > FaceNotfoundLimit)
				_localFaceFound = false;

		}
	}
	public static Texture2D LocalCropedImage
	{
		get
		{
			return _localCropedImage;
		}
	}
	public static Rect LocalFace
	{
		get
		{
			return _localFace;
		}
	}
	public static bool PeerFaceFound
	{
		get
		{
			return _peerFaceFound;
		}
	}
	public static Texture2D PeerSourceImage
	{
		get
		{
			return _peerSourceImage;
		}
		set
		{
			_peerSourceImage = _peerNeedsConversion ? ConvertImage(value) : value;

			if(DrawCropedFace)
			{
				_peerCropedImage = CropFace(_peerSourceImage, PeerFace);
			}
		}
	}
	public static Texture2D PeerCropedImage
	{
		get
		{
			return _peerCropedImage;
		}
	}
	public static Rect PeerFace
	{
		get
		{
			return _peerFace;
		}
		set
		{
			_peerFaceFound = true;
			_peerFace = value;
		}
	}
	public static bool PeerNeedsConversion
	{
		set
		{
			_peerNeedsConversion = value;
		}
	}
	private static bool FindFace(Texture2D sourceImage, out Rect face)
	{
		Utils.texture2DToMat(sourceImage, _imgMat);

		Imgproc.cvtColor(_imgMat, _grayMat, Imgproc.COLOR_RGBA2GRAY);
		Imgproc.equalizeHist(_grayMat, _grayMat);

		if(_cascade != null)
			_cascade.detectMultiScale(_grayMat, _matFaces, 1.1, 2, 2,
					   new Size(20, 20), new Size());

		var faces = _matFaces.toArray();

		var mainFace = new OpenCVForUnity.Rect();
		foreach(var f in faces)
		{
			if(mainFace.area() < f.area())
				mainFace = f;
		}

		face = new Rect(mainFace.x, mainFace.y, mainFace.width, mainFace.height);

		return faces.Length > 0;
	}
	private static bool FaceDeltaAcceptable(Rect localFace, ref Rect lastSentLocalFace)
	{
		if(Math.Abs(Vector2.Distance(localFace.center, lastSentLocalFace.center)) >= MaximumCenterDistance)
		{
			if(Math.Abs(localFace.size.magnitude - lastSentLocalFace.size.magnitude) > MaximumSizeDelta)
			{
				lastSentLocalFace = localFace;
				return false;
			}
		}
		return true;
	}
	private static Texture2D ConvertImage(Texture2D sourceImage)
	{
		var tmpTexture = sourceImage;

		var rt = new RenderTexture(tmpTexture.width, tmpTexture.height, 24);

		Graphics.Blit(tmpTexture, rt);

		var returnTexture = new Texture2D(tmpTexture.width, tmpTexture.height);

		RenderTexture.active = rt;
		returnTexture.ReadPixels(new Rect(0, 0, tmpTexture.width, tmpTexture.height), 0, 0);
		returnTexture.Apply();

		return returnTexture;
	}
	private static Texture2D CropFace(Texture2D sourceImage, Rect face)
	{
		var sourceArray = sourceImage.GetPixels();

		face = ScaleFace(face);

		var fw = Convert.ToInt32(face.width);
		var fh = Convert.ToInt32(face.height);

		var cropArray = new Color[fw * fh];

		var cropedTexture = new Texture2D(fw, fh, sourceImage.format, true);

		var oTi = 0;
		var cropI = 0;
		var yMax = Convert.ToInt32(face.yMax);
		var xMax = Convert.ToInt32(face.xMax);
		var y = Convert.ToInt32(face.y);
		var x = Convert.ToInt32(face.x);
		var tmpColor = new Color();

		_rowCounter = 0;
		_colorType = 0;

		for(var yi = y; yi < yMax; yi++)
		{
			oTi = yi * sourceImage.height;
			tmpColor = GetRowColor();
			for(var xi = x; xi < xMax; xi++)
			{
				cropArray[cropI] = sourceArray[oTi + xi];
				cropI++;
			}
		}

		cropedTexture.SetPixels(cropArray);
		cropedTexture.Apply();


		return cropedTexture;
	}

	private static Rect ScaleFace(Rect face)
	{
		const int change = 10;
		var newFace = new Rect()
		{
			x = face.x - change > 0 ? face.x - change : 0,
			y = face.y - change > 0 ? face.y - change : 0,
			xMax = face.xMax + change,
			yMax = face.yMax + change
		};

		return newFace;
	}

	private static Color GetRowColor()
	{
		Color returnColor = Color.white;
		_rowCounter++;
		if(_rowCounter > 20)
		{
			_rowCounter = 0;
			_colorType++;
		}
		switch(_colorType % 3)
		{
			case 0:
				returnColor = Color.red;
				break;
			case 1:
				returnColor = Color.blue;
				break;
			case 2:
				returnColor = Color.green;
				break;
		}
		return returnColor;
	}
}
