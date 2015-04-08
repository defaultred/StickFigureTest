--- Simple Webcam ---

--Set-up:

1. Simply apply the "Webcam" (or "WebcamWebPlayer" if building for web player) component to the 
   object you wish to display it on.

2. Enter the device Index (leave at 0 if you only have one device), or enter the device name.

3. If building for webplayer, make sure you add in a WebcamWebPlayerController componant anywhere in
   the scene. This will handle the camera permissions.

That's it! Click play!

The texture of the main material of the object the component is on, will be changed to the webcam texture.



--Tweaking:

As only the texture of the material is changed, set-up the material on the object beforehand, how you would 
like. For example, tint colour or shader (Recommended 'Unlit' shader for a screen effect)

To change the resolution the webcam outputs, check the Manual Resolution tickbox and enter your
preferred resolution.*

To change the FPS (frames per second), Check the  manual FPS tickbox and enter your preferred fps.*

*These settings should only need to be tweaked if your scene is running slow, however some webcam's won't 
output their maximum resolution automatically.



--Notes:

Building for webplayer requires a prompt for permission from the user. So make sure you use the webplayer 
specific components if building for webplayer.

The same camera can be displayed on multiple objects at the same time.

Multiple cameras can be displayed at the same time.




Thank you for using Simple Webcam!