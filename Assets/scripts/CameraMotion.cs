// This sample code demonstrates how to create geometry "on demand" based on camera motion.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion : MonoBehaviour {
    int max_plane = -1; // the number of planes that we have made
    int min_plane = 1;
    // int right_plane = 0;
    // int left_plane = 0;
    float plane_size = 3.0f; // the size of the planes

    void Start () {
        // start with one plane
        // create_new_plane(0); 
    }
    
    // move the camera, and perhaps create a new plane
    void Update () {
        
        // get the horizontal and vertical controls (arrows, or WASD keys)
        float dx = Input.GetAxis ("Horizontal");
        float dz = Input.GetAxis ("Vertical");
        
        // sensitivity factors for translate and rotate
        //originally 0.3f and 5.0f
        float translate_factor = 0.03f;
        float rotate_factor = .50f;
        
        // translate_factor *= 10;
        // rotate_factor *= 10;

        Camera temp = Camera.main;
        // Camera temp = Camera.current;
        // originally, temp = Camera.current;
        // Camera.main works but is super sensitive 

        // move the camera based on keyboard input
        if (temp != null) {
            // translate forward or backwards
            temp.transform.Translate (0, 0, dz * translate_factor);
            // rotate left or right
            temp.transform.Rotate (0, dx * rotate_factor, 0);
        }
        
        // get the main camera position
        Vector3 cam_pos = Camera.main.transform.position;
        
        //Debug.LogFormat ("x z: {0} {1}", cam_pos.x, cam_pos.z);
        
        // if the camera has moved far enough, create another plane
        // if (cam_pos.z > (max_plane + 0.5) * plane_size * 2) {
        //     // create_new_plane(0);
        //     mesh_to_game_object(create_plane(grid_size, grid_verts_per_side));
        // }
        // if (cam_pos.z < (min_plane - 0.5) * plane_size * 2) {
        //     // create_new_plane(1);
        //     mesh_to_game_object(create_plane(grid_size, grid_verts_per_side));
        // }

        // if (cam_pos.x > (right_plane + 0.5) * plane_size * 2) {
        //     create_new_plane(2);
        // }
        // if (cam_pos.x < (left_plane - 0.5) * plane_size * 2) {
        //     create_new_plane(3);
        // }

    }
    
    // create a new plane
    //int dir:
    // 0 -> up
    // 1 -> down
    // 2 -> left
    // 3 -> right
    void create_new_plane(int dir) {
        int index = 0;
        if (dir == 0) index = max_plane + 1;
        else if (dir == 1) index = min_plane - 1;

        // if (dir == 2) index = right_plane + 1;
        // else if (dir == 3) index = left_plane - 1;

        float plane_scale = plane_size / 5.0f;
        
        // make a new plane
        GameObject s = GameObject.CreatePrimitive(PrimitiveType.Plane);
        s.name = index.ToString("Plane 0"); // give this plane a name
        
        // modify the size and position of the plane
        s.transform.localScale = new Vector3 (plane_scale, plane_scale,
        plane_scale);
        
        // move plane to proper location in z
        s.transform.position = new Vector3 (0.0f, 0.0f, 2 * plane_size * (index
        + 0.0f));
        
        // change the plane's color
        Renderer rend = s.GetComponent<Renderer>();
        
        // alternate between two colors
        int temp = 0;
        if (dir == 0) temp = max_plane;
        else if (dir == 1) temp = min_plane;
        
        // else if (dir == 2) temp = left_plane;
        // else if (dir == 3) temp = right_plane;

        if (temp % 2 == 0)
            rend.material.color = new Color (0.2f, 0.2f, 0.7f, 1.0f);
        else
            rend.material.color = new Color (0.7f, 0.1f, 0.1f, 1.0f);
        
        // increment the number of planes that we've created
        if (dir == 0) max_plane++;
        else if (dir == 1) min_plane--;

        // else if (dir == 2) left_plane--;
        // else if (dir == 3) right_plane++;
    }

    
}