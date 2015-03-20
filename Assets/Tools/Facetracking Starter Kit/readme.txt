============
Introduction
============
First of all, thank you for purchasing the Facetracking Starter Kit.
This kit is an easy way to start building application that require the new face tracking technology recently developed with Kinect for Windows.
It takes care of setting the kinect device and make the data available to your scripts in Unity.

This kit provides two main components:
- KinectDataTransmitter: a .net4 project that reads the data from the kinect device (full source code included)
- Unity scripts that launch the KinectDataTransmitter as a separate process, manage its life-cycle and retrieve the available kinect data.

The data we are exposing through the scripts are the Facetracking data namely:
- the user head position
- the user head rotation
- a set of 6 animation units [you can read more here: http://msdn.microsoft.com/en-us/library/jj130970.aspx]
- rbg camera frames
- depth camera frames


============
Installation
============
1. 
To use the Facetracking Starter Kit you will need to install the Kinect SDK 1.7.0. or the Kinect redistributables.
Please note that for the Kinect XBOX 360 you will need the SDK as it is not supported in by the end-user redistributables.

Downloads:
- Kinect SDK 1.7.0 (support both the Kinect for Windows and Kinect XBOX 360) is available at: http://go.microsoft.com/fwlink/?LinkId=275588
- or Kinect Redistributables (only supports Kinect for Windows) at: http://go.microsoft.com/fwlink/?LinkID=275590


2.
You will need to unzip the KinectDataTransmitter into a root project folder (the folder above the Assets folder).
We are deeply sorry for this inconvinence but Unity does not deal well with having this inside the Assets folder.

The end setup will be:
your-root-project
|- Assets
|- Library
|- ProjectSettings
|- Kinect
   |- DataConverter.dll
   |- FaceTrackData.dll
   |- FaceTrackLib.dll
   |- KinectDataTransmitter.dll
   |- Microsoft.Kinect.Toolkit.dll
   |- Microsoft.Kinect.Toolkit.FaceTracking.dll
   
   

=====
Usage
=====
To use the Facetracking Starter Kit you simply need to:
- Add the KinectBinder on one of your game objects
- Add an event handler to the KinectBinder.FaceTrackingDataReceived
- Use the received data to manipulate your animations!


===========
Oni Example
===========
The Facetracking Starter Kit comes with a simple example of how to use the Kinect face tracking data.
We use the mask of a demon (oni) to express the six animation units as well as positional information retrieved from the kinect.
The logic is contained in a single script, FaceTrackingExample.cs, and consists of 3 phases:
- Use the received data to animate the mask.
- Return to a neutral state when we detect that no more data is being receibed.
- Play automatical animations whenever there is no kinect data being received.

In the project there are two scenes. A simple facetracking scene and one with both the rgb and depth video feedback.
Both scene are almost identical except that second one is significantly slower to render due to the massive amount of data being pushed to the textures in real time.

====================
How to go from here?
====================
Included in this package is the full source of the KinectDataTransmitter.
Should you need more data from the kinect you could start by modifying this project.

Quite a lot of documentation and resources are available at the Kinect for Windows website: http://www.microsoft.com/en-us/kinectforwindows/.


================
Additional Notes
================
Whenever the Kinect FPS drops to 0, the mask will play automatic animations.
A FPS of 0 means that the kinect fracetracking library did not find any face to track.
Most often, this will happen if the user is not in a tracking range.
Kinect starts to track users that are at (approximately) 1m up to several (5+) meters.

For the best results try to stay in range between 1.5 meters and 2 meters.
The room illumination also has an impact on the quality of the tracking.
Please note that those limitations come from the kinect device and software itself.


=======
Contact
=======
For questions and feeback, you can contact me directly at eurico@doirado.net.