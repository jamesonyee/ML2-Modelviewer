![image](https://github.com/user-attachments/assets/b70990c2-46f4-4eb1-8e7a-85cc34d236e0)# ML2-Modelviewer

Augmented Reality construction visualization application for the Magic Leap 2

This project implements marker tracking using a Unity's OpenXR library with the Magic Leap feature group. Specifically it takes advantage of Marker Understanding from the follow link below:
<p> https://developer-docs.magicleap.cloud/docs/guides/unity-openxr/marker-understanding/unity-marker-understanding-example/ </p>

The marker understanding script in Asset > Scripts is an adaptation of the example provided above. This script has a dictionary that allows users to add 3D models to the software and associate it with the ArUco code of thier choosing. 

ArUco codes can be generated at: https://chev.me/arucogen/  

<b> Adding your own models </b>
To add your own models declare a game object with the name of the model in 

![image](https://github.com/user-attachments/assets/7273cccb-a70d-48e1-b9c3-8fc0604a9477)

