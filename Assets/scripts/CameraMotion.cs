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

    int x_offset = 1000, y_offset = 1000; 
    private int grid_verts_per_side = 85;
    private float grid_size = 10;

    
    void Start () {
        // start with one plane
        // create_new_plane(0);

        Mesh m = create_plane(grid_size, grid_verts_per_side);
        mesh_to_game_object(m);
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

    private Mesh create_plane(float grid_size, int grid_verts_per_side) {
        Vector3[] verts = new Vector3[grid_verts_per_side * grid_verts_per_side];  	// the vertices of the mesh
	    int[] tris = new int[(2 * (grid_verts_per_side - 1) * (grid_verts_per_side - 1)) * 3];      	// the triangles of the mesh (triplets of integer references to vertices)
	    Mesh mesh = new Mesh();

        //generate the verticies for the plane
        for (int i = 0; i < grid_verts_per_side ; i++) {
            for (int j = 0; j < grid_verts_per_side; j++) {
                int vert_index = i * grid_verts_per_side + j;
                float x_index = grid_size / grid_verts_per_side * i;
                float y_index = grid_size / grid_verts_per_side * j;
                // verts[vert_index] = new Vector3(x_index, y_index, 0);

                verts[vert_index] = new Vector3(x_index, 0, y_index);
            }
        }

        int ntris = 0;
        // for (int i = 0; i < grid_verts_per_side - 1; i++) {
        //     for (int j = 0; j < grid_verts_per_side - 1; j++) {
        //         MakeQuad(i, j, i + grid_verts_per_side, j + grid_verts_per_side, ntris, tris);
        //         ntris += 2;
        //     }
        // }
        for (int i = 0; i < verts.Length - 1 - grid_verts_per_side - 1; i++) {
            if (i % (grid_verts_per_side) != grid_verts_per_side - 1 || i == 0) {
                int tl, tr, bl, br;
                tl = i;
                tr = i + 1;
                bl = i + grid_verts_per_side;
                br = i + grid_verts_per_side + 1;
                MakeQuad(tl, tr, br, bl, ntris, tris);
                // MakeQuad(bl, br, tr, tl, ntris, tris);
                
                ntris += 2;
            }
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        return mesh;
    }

    GameObject mesh_to_game_object(Mesh mesh) {
        
        GameObject s = new GameObject("temp name");
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();

        // associate the mesh with this object
        s.GetComponent<MeshFilter>().mesh = mesh;

        // change the color of the object
        Renderer rend = s.GetComponent<Renderer>();
        // random color
        // rend.material.color = new Color(Random.value, Random.value, Random.value, 1.0f);
        rend.material.color = new Color(0,0,1);
        return s;
    }

    // make a triangle from three vertex indices (clockwise order)
	void MakeTri(int i1, int i2, int i3, int ntris, int [] tris) {
		int index = ntris * 3;  // figure out the base index for storing triangle indices
		// ntris++;

		tris[index]     = i1;
		tris[index + 1] = i2;
		tris[index + 2] = i3;
	}

	// make a quadrilateral from four vertex indices (clockwise order)
	void MakeQuad(int i1, int i2, int i3, int i4, int ntris, int[] tris) {
		MakeTri (i1, i2, i3, ntris, tris);
		MakeTri (i1, i3, i4, ntris + 1, tris);
	}
}