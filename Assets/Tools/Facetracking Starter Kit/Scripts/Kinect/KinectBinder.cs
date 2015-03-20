using System;
using System.Diagnostics;
using DataConverter;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// The kinect binder creates the necessary setup for you to receive data for the kinect face tracking system directly.
/// 
/// Simply subscribe to the FaceTrackingDataReceived event to receive face tracking data.
/// Check FaceTrackingExample.cs for an example.
/// 
/// VideoFrameDataReceived and DepthFrameDataReceived events will give you the raw rbg/depth data from kinect cameras.
/// Check ImageFeedback.cs for an example.
/// </summary>
public class KinectBinder : MonoBehaviour
{
    public delegate void FaceTrackingDataDelegate(float au0, float au1, float au2, float au3, float au4, float au5, float posX, float posY, float posZ, float rotX, float rotY, float rotZ);
    public event FaceTrackingDataDelegate FaceTrackingDataReceived;

    public delegate void VideoFrameDataDelegate(Color32[] pixels);
    public event VideoFrameDataDelegate VideoFrameDataReceived;

    public delegate void DepthFrameDataDelegate(short[] pixels);
    public event DepthFrameDataDelegate DepthFrameDataReceived;

    public delegate void SkeletonDataDelegate(JointData[] jointsData);
    public event SkeletonDataDelegate SkeletonDataReceived;

    private float _timeOfLastFrame;
    private int _frameNumber = -1;
    private int _processedFrame = -1;
    private Process _otherProcess;

    private int _kinectFps;
    private int _kinectLastFps;
    private float _kinectFpsTimer;
    private bool _hasNewVideoContent;
    private bool _hasNewDepthContent;
    private string _faceTrackingData;
	private string _skeletonData;
    private short[] _depthBuffer;
    private Color32[] _colorBuffer;
    private JointData[] _jointsData;

    // Use this for initialization
    void Start()
    {
        BootProcess();
    }

    private void BootProcess()
    {
        const string dataTransmitterFilename = "KinectDataTransmitter.exe";
        string path = Application.dataPath + @"/../Kinect/";

        _otherProcess = new Process();
        _otherProcess.StartInfo.FileName = path + dataTransmitterFilename;
        _otherProcess.StartInfo.UseShellExecute = false;
        _otherProcess.StartInfo.CreateNoWindow = true;
        _otherProcess.StartInfo.RedirectStandardInput = true;
        _otherProcess.StartInfo.RedirectStandardOutput = true;
        _otherProcess.StartInfo.RedirectStandardError = true;
        _otherProcess.OutputDataReceived += (sender, args) => ParseReceivedData(args.Data);
        _otherProcess.ErrorDataReceived += (sender, args) => Debug.LogError(args.Data);

        try
        {
            _otherProcess.Start();
        }
        catch (Exception)
        {
            Debug.LogWarning(
                "Could not find the kinect data transmitter. Please read the readme.txt for the setup instructions.");
            _otherProcess = null;
            enabled = false;
            return;
        }
        _otherProcess.BeginOutputReadLine();
        _otherProcess.StandardInput.WriteLine("1"); // gets rid of the Byte-order mark in the pipe.
    }

    void ParseReceivedData(string data)
    {
        if (Converter.IsFaceTrackingData(data))
        {
            _faceTrackingData = data;
        }
		else if (Converter.IsSkeletonData(data))
		{
            _skeletonData = data;
        }
        else if (Converter.IsVideoFrameData(data))
        {
            _hasNewVideoContent = true;
        }
        else if (Converter.IsDepthFrameData(data))
        {
            _hasNewDepthContent = true;
        }
        else if (Converter.IsPing(data))
        {
            if (_otherProcess != null && !_otherProcess.HasExited)
            {
                _otherProcess.StandardInput.WriteLine(Converter.EncodePingData());
            }
        }
        else if (Converter.IsError(data))
        {
            Debug.LogError(Converter.GetDataContent(data));
        }
        else if (Converter.IsInformationMessage(data))
        {
            Debug.Log("Kinect (information message): " + Converter.GetDataContent(data));
        }
        else
        {
            Debug.LogWarning("Received this (unknown) message from kinect: " + data);
        }
    }

    void Update()
    {
        if (_otherProcess == null || _otherProcess.HasExited)
        {
            Debug.LogWarning("KinectDataTransmitter has exited. Trying to reboot the process...");
            BootProcess();
        }

        bool hasNewData = (_frameNumber > _processedFrame);

        if (hasNewData)
        {
            _kinectFps += _frameNumber - _processedFrame;
            _processedFrame = _frameNumber;
        }

        if (_hasNewVideoContent)
        {
            _hasNewVideoContent = false;
            ProcessVideoFrame(Converter.GetVideoStreamData());
        }

        if (_hasNewDepthContent)
        {
            _hasNewDepthContent = false;
            ProcessDepthFrame(Converter.GetDepthStreamData());
        }

        if (_faceTrackingData != null)
        {
            string data = _faceTrackingData;
            _faceTrackingData = null;
            ProcessFaceTrackingData(Converter.GetDataContent(data));
        }

        if (_skeletonData != null)
        {
            string data = _skeletonData;
            _skeletonData = null;
            ProcessSkeletonData(Converter.GetDataContent(data));
        }

        UpdateFrameCounter();
    }

    private void ProcessDepthFrame(byte[] bytes)
    {
        if (DepthFrameDataReceived == null || bytes == null)
            return;

        if (_depthBuffer == null || _depthBuffer.Length != bytes.Length/2)
        {
            _depthBuffer = new short[bytes.Length / 2];
        }
        for (int i = 0; i < _depthBuffer.Length; i++)
        {
            int byteIndex = i * 2;
            _depthBuffer[i] = BitConverter.ToInt16(bytes, byteIndex);
        }

        DepthFrameDataReceived(_depthBuffer);
    }

    private void ProcessVideoFrame(byte[] bytes)
    {
        if (VideoFrameDataReceived == null || bytes == null)
            return;

        if (_colorBuffer == null || _colorBuffer.Length != bytes.Length / 4)
        {
            _colorBuffer = new Color32[bytes.Length / 4];
        }

        for (int i = 0; i < _colorBuffer.Length; i++)
        {
            int byteIndex = i*4;
            _colorBuffer[i] = new Color32(bytes[byteIndex+2], bytes[byteIndex+1], bytes[byteIndex], byte.MaxValue);
        }

        VideoFrameDataReceived(_colorBuffer);
    }

    private void ProcessFaceTrackingData(string data)
    {
        if (FaceTrackingDataReceived == null)
            return;

        _frameNumber++;
        float au0, au1, au2, au3, au4, au5, posX, posY, posZ, rotX, rotY, rotZ;
        Converter.DecodeFaceTrackingData(data, out au0, out au1, out au2, out au3, out au4, out au5, out posX,
                                         out posY, out posZ, out rotX, out rotY, out rotZ);

        FaceTrackingDataReceived(au0, au1, au2, au3, au4, au5, posX, posY, posZ, rotX, rotY, rotZ);
    }

    private void ProcessSkeletonData(string data)
    {
        if (SkeletonDataReceived == null)
            return;

        _frameNumber++;
        if (_jointsData == null)
        {
            _jointsData = new JointData[(int)JointType.NumberOfJoints];
        }
        Converter.DecodeSkeletonData(data, _jointsData);
        SkeletonDataReceived(_jointsData);
    }

    private void UpdateFrameCounter()
    {
        _kinectFpsTimer -= Time.deltaTime;
        if (_kinectFpsTimer <= 0f)
        {
            _kinectLastFps = _kinectFps;
            _kinectFps = 0;
            _kinectFpsTimer = 1;
        }
    }

    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint)
            return;

        GUI.color = Color.white;
        GUI.Label(new Rect(5, 5, 250, 30), "Kinect FPS: " + _kinectLastFps);
        if (_kinectLastFps == 0)
        {
            GUI.Label(new Rect(5, 25, 400, 30), "(Kinect is not tracking... please get in range.)");
        }

    }

    void OnApplicationQuit()
    {
        ShutdownKinect();
    }

    private void ShutdownKinect()
    {
        if (_otherProcess == null)
            return;

        try
        {
            Process.GetProcessById(_otherProcess.Id);
        }
        catch (ArgumentException)
        {
            // The other app might have been shut down externally already.
            return;
        }

        try
        {
            _otherProcess.CloseMainWindow();
            _otherProcess.Close();
        }
        catch (InvalidOperationException)
        {
            // The other app might have been shut down externally already.
        }
    }
}

