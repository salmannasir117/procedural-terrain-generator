// This sample code demonstrates how to create geometry "on demand" based on camera motion.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//simply handle the motion of the camera
public class CameraMotion : MonoBehaviour {
   
    void Start () {
        //tilt camera to interesting angle
        if (Camera.main != null) Camera.main.transform.Rotate(new Vector3(45, 0, 0));
    }
    
    // move the camera
    void Update () {
        
        // get the horizontal and vertical controls (arrows, or WASD keys)
        float dx = Input.GetAxis ("Horizontal");
        float dz = Input.GetAxis ("Vertical");
        
        // sensitivity factors for translate and rotate
        //originally 0.3f and 5.0f
        float translate_factor = 0.03f;
        float rotate_factor = .25f;
        

        Camera cam = Camera.main;

        // move the camera based on keyboard input, account for rotation of camera
        if (cam != null) {
            // translate forward or backwards
            cam.transform.Translate (0, dz * translate_factor, dz * translate_factor);
            
            // rotate left or right
            cam.transform.Rotate(new Vector3(0, 1, 0), dx * rotate_factor, Space.World);
        }
    }
}