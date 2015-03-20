
using System.IO;
using UnityEngine;

public class ImageFeedback : MonoBehaviour
{
    public KinectBinder Kinect;
    public Renderer ColorRenderer;
    public Renderer DepthRenderer;

    private Texture2D _colorTex;
    private Texture2D _depthTex;
    private bool _colorUpdated;
    private bool _depthUpdated;
    private Color32[] _colorPixels;
    private Color32[] _depthPixels;

    void Start()
    {
        _colorTex = new Texture2D(640, 480, TextureFormat.ARGB32, false);
        _depthTex = new Texture2D(640, 480, TextureFormat.ARGB32, false);
        _colorPixels = new Color32[640 * 480];
        _depthPixels = new Color32[640 * 480];
        for (int i = 0; i < _colorPixels.Length; i++)
        {
            _colorPixels[i] = new Color32(0, 0, 0, 0);
        }
        for (int i = 0; i < _depthPixels.Length; i++)
        {
            _depthPixels[i] = new Color32(0, 0, 0, 0);
        }

        _colorTex.SetPixels32(_colorPixels);
        _colorTex.Apply(false);
        _depthTex.SetPixels32(_depthPixels);
        _depthTex.Apply(false);

        ColorRenderer.material.mainTexture = _colorTex;
        DepthRenderer.material.mainTexture = _depthTex;
        Kinect.VideoFrameDataReceived += ProcessVideoFrame;
        Kinect.DepthFrameDataReceived += ProcessDepthFrame;
    }

    private void ProcessVideoFrame(Color32[] pixels)
    {
        _colorPixels = pixels;
        _colorUpdated = true;
    }

    private void ProcessDepthFrame(short[] depth)
    {
        for (int i = 0; i < _depthPixels.Length; i++)
        {
            //_depthPixels[i] = new Color32(depth[i], depth[i], depth[i], byte.MaxValue);
            _depthPixels[i] = new Color32((byte)(depth[i] >> 8), (byte)(depth[i] >> 8),(byte)(depth[i] >> 8), byte.MaxValue);
        }
        _depthUpdated = true;
    }

    void Update()
    {
        if (_colorUpdated)
        {
            _colorUpdated = false;
            _colorTex.SetPixels32(_colorPixels);
            _colorTex.Apply(false);
        }
        if (_depthUpdated)
        {
            _depthUpdated = false;
            _depthTex.SetPixels32(_depthPixels);
            _depthTex.Apply(false);
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Saving...");
            SaveTexture(_colorTex, "color.png");
            SaveTexture(_depthTex, "depth.png");
        }
    }

    private void SaveTexture(Texture2D tex, string filename)
    {
        byte[] pngContent = tex.EncodeToPNG();
        var file = File.Create(filename, pngContent.Length);
        file.Write(pngContent, 0, pngContent.Length);
    }
}
