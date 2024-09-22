using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    int x_offset = 1000, y_offset = 1000; 
    private int grid_verts_per_side = 85;
    private float grid_size = 10;
    
    // Start is called before the first frame update
    void Start()
    {
        Mesh m = create_plane(grid_size, grid_verts_per_side);
        // perlin_noise(m, grid_verts_per_side, grid_size, 0, 0);
        mesh_to_game_object(m);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Mesh create_plane(float grid_size, int grid_verts_per_side) {
        Vector3[] verts = new Vector3[grid_verts_per_side * grid_verts_per_side];  	// the vertices of the mesh
	    int[] tris = new int[(2 * (grid_verts_per_side - 1) * (grid_verts_per_side - 1)) * 3];      	// the triangles of the mesh (triplets of integer references to vertices)
	    Mesh mesh = new Mesh();
        Color[] colors = new Color[verts.Length];

        //generate the verticies for the plane
        for (int i = 0; i < grid_verts_per_side ; i++) {
            for (int j = 0; j < grid_verts_per_side; j++) {
                int vert_index = i * grid_verts_per_side + j;
                float x_index = grid_size / grid_verts_per_side * i;
                float y_index = grid_size / grid_verts_per_side * j;
                // verts[vert_index] = new Vector3(x_index, y_index, 0);
                
                float noise = get_perlin_noise(x_index, y_index, x_offset, y_offset);
                verts[vert_index] = new Vector3(x_index, noise, y_index);
                colors[vert_index] = get_color(noise);
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

        // Renderer rend = 
        mesh.colors = colors;
        // mesh.SetColors(colors);
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
        //get the renderer, attach a material that uses a vertex shader 
        //thus, we can color each vertex and it mixes the colors. 

        Renderer rend = s.GetComponent<Renderer>();
        Material material = new Material(Shader.Find("Particles/Standard Surface"));
        rend.material = material;

        // // random color
        // rend.material.color = new Color(Random.value, Random.value, Random.value, 1.0f);
        // rend.material.color = new Color(0,0,1);
        return s;
    }

    // void perlin_noise(Mesh mesh, int grid_verts_per_side, float grid_size, int x_world_offset, int y_world_offset) {
    //     for (int i = 0; i < grid_verts_per_side; i++) {
    //         for (int j = 0; j < grid_verts_per_side; j++) {
    //             float x = grid_size / grid_verts_per_side * i + x_world_offset + x_offset;
    //             float y = grid_size / grid_verts_per_side * j + y_world_offset + y_offset; 
                
    //             mesh.vertices[i * grid_verts_per_side + j].x = 
    //                 Mathf.PerlinNoise(x, y)
    //                 + 1.0f / 2 * Mathf.PerlinNoise(2 * x, 2 * y)
    //                 + 1.0f / 4 * Mathf.PerlinNoise(4 * x, 4 * y);

    //         }
    //     }
    //     mesh.RecalculateNormals();
    // }

    float get_perlin_noise(float x, float y, float x_offset, float y_offset) {
        return Mathf.PerlinNoise(x + x_offset , y + y_offset) 
        + 1.0f / 2 * Mathf.PerlinNoise(2 * x + x_offset, 2 * y + y_offset) 
        + 1.0f / 4 * Mathf.PerlinNoise( 4 * x + x_offset, 4 * y + y_offset);
    }

    //for some reason, the heights are super high. this is wehre we assign color based on height
    //this allows us to create mountains, rivers, etc. coloring based on height
    Color get_color(float x) {
        
        //spooky coloring below
        if (x > 10.0f / 10) return Color.white;
        else if (x  > 2.5 / 10.0f) return new Color(25, 25, 25) / 255.0f;   //dark grey
        else if (x > 0.5 /10.0f) return Color.red;
        else return Color.green;
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
