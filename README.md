# ML2-Modelviewer

Augmented Reality construction visualization application for the Magic Leap 2

This project implements marker tracking using a Unity's OpenXR library with the Magic Leap feature group. Specifically it takes advantage of Marker Understanding from the follow link below:
<p> https://developer-docs.magicleap.cloud/docs/guides/unity-openxr/marker-understanding/unity-marker-understanding-example/ </p>

The marker understanding script in Asset > Scripts is an adaptation of the example provided above. This script has a dictionary that allows users to add 3D models to the software and associate it with the ArUco code of thier choosing. 

ArUco codes can be generated at: https://chev.me/arucogen/  

<b> Adding your own models </b>
To add your own models declare a game object with the name of the model in the marker class at the top of the code:
![image](https://github.com/user-attachments/assets/7273cccb-a70d-48e1-b9c3-8fc0604a9477)

At the bottom of the start function assign all game object you defined in the class to a ArUco ID (0-999)
![image](https://github.com/user-attachments/assets/dc00a2b7-903a-4691-9e39-102f4ae2ae9f)

In the Unity editor click the ML rig in the hierarchy. You will be able to click and drag prefabs from the project outliner into each game object you defined in the script.
![image](https://github.com/user-attachments/assets/269f0a4c-2e96-4eea-abb6-1a2d8b1d6d68)



