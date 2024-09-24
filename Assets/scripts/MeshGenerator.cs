// using System;
// using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshGenerator : MonoBehaviour
{
    int x_offset = 1000, y_offset = 1000; 
    private int grid_verts_per_side = 85;
    private float grid_size = 10.0f;
    
    public enum Terrain {
        Texture2D,
        snowy,
        sandy,
        greyscale,
        greenland
    }
    public Terrain terrain_selection = Terrain.snowy;

    //https://docs.unity3d.com/Manual/InstantiatingPrefabs.html
    
    //Texture2D/snowy biome plants
    public GameObject flower_prefab = null, tree_prefab = null;
    //sany biome plants
    public GameObject cactus_prefab = null, small_cactus_prefab = null;
    //greyscale plants
    public GameObject greyscale_flower_prefab = null, greyscale_tree_prefab = null;
    public int octaves = 3;
    //camera moves in xz plane
    float left_bound, right_bound, top_bound, bottom_bound;

    // Start is called before the first frame update
    void Start()
    {
        left_bound = 0; right_bound = grid_size; top_bound = grid_size; bottom_bound = -grid_size;
        Mesh m = create_plane(grid_size, grid_verts_per_side, 0, 0);
        // perlin_noise(m, grid_verts_per_side, grid_size, 0, 0);
        mesh_to_game_object(m);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cam_pos = Camera.main.transform.position;
        float magic_offset = grid_size / grid_verts_per_side;
        // if the camera has moved far enough, create another plane
		if (cam_pos.z > (top_bound) - grid_size / 2) {  //generate new top row from left_bonud to right bound. use mod grid_size
        //or, put them in 2d array and use int. div and mod to determine which cell u are in.
        //int divide by grid_size, check if generated. if not, generate. (use mod to snap back to grid??) floor(pos/grid_size) * grid_size??
			GameObject temp = mesh_to_game_object(create_plane(grid_size, grid_verts_per_side, 0, top_bound - magic_offset));
            temp.transform.position = new Vector3(0, 0, 0);
            top_bound += grid_size;
		} 
        else if (cam_pos.z < bottom_bound + grid_size / 2) {
            GameObject temp = mesh_to_game_object(create_plane(grid_size, grid_verts_per_side, 0, bottom_bound + magic_offset));
            temp.transform.position = new Vector3(0, 0, 0);
            bottom_bound -= grid_size;
        } else if (cam_pos.x > right_bound - grid_size / 2) { //generate new right col. from bottom to top
            GameObject temp = mesh_to_game_object(create_plane(grid_size, grid_verts_per_side, right_bound - magic_offset, top_bound - magic_offset));
            temp.transform.position = new Vector3(0, 0, 0);
            right_bound += grid_size;
        } else if (cam_pos.x < left_bound + grid_size / 2) {
            GameObject temp = mesh_to_game_object(create_plane(grid_size, grid_verts_per_side, left_bound + magic_offset, top_bound - magic_offset));
            temp.transform.position = new Vector3(0, 0, 0);
            left_bound -= grid_size;
        }
    }

    //method used to place plants based on the selected terrain mode.
    //each terrain has a "high plant" which is placed in high locations of the biome 
    //and a "low plant" placed low in the biome.
    private void place_plant(float x_index, float noise, float y_index) {
        GameObject low_plant = null, high_plant = null;
        float offset = 0.05f;   //offset used to raise the plants to ground level.
        
        // set the low and high plants based on the biome selected
        if (terrain_selection == Terrain.snowy || terrain_selection == Terrain.Texture2D || terrain_selection == Terrain.greenland) {
            low_plant = flower_prefab;
            high_plant = tree_prefab;
        } else if (terrain_selection == Terrain.sandy) {
            low_plant = small_cactus_prefab;
            high_plant = cactus_prefab;
        } else if (terrain_selection == Terrain.greyscale) {
            low_plant = greyscale_flower_prefab;
            high_plant = greyscale_tree_prefab;
        }

        //make sure height is withing low plant bounds, place with certain probability
        //make sure height is within high plant bounds, place with certain probability
        if (noise > 0.1 && noise < 0.5 && Random.value < 0.1) {
            Instantiate(low_plant, new Vector3(x_index, noise + offset, y_index), Quaternion.identity);
        } else if (noise > 1 && Random.value < 0.01) {
           Instantiate(high_plant, new Vector3(x_index, noise + offset, y_index), Quaternion.identity);
        }
    }
    private Mesh create_plane(float grid_size, int grid_verts_per_side, float x_off, float y_off) {
        Vector3[] verts = new Vector3[grid_verts_per_side * grid_verts_per_side];  	// the vertices of the mesh
	    int[] tris = new int[(2 * (grid_verts_per_side - 1) * (grid_verts_per_side - 1)) * 3];      	// the triangles of the mesh (triplets of integer references to vertices)
	    Mesh mesh = new Mesh();
        Color[] colors = null;
        Vector2[] uvs = new Vector2[verts.Length];

        // if (terrain_selction == Terrain.snowy) {
            colors = new Color[verts.Length];
        // }

        //generate the verticies for the plane
        for (int i = 0; i < grid_verts_per_side ; i++) {
            for (int j = 0; j < grid_verts_per_side; j++) {
                int vert_index = i * grid_verts_per_side + j;
                float x_index = grid_size / grid_verts_per_side * i + x_off;
                float y_index = grid_size / grid_verts_per_side * j + y_off;
                // verts[vert_index] = new Vector3(x_index, y_index, 0);
                
                float noise = get_perlin_noise(x_index, y_index, x_offset, y_offset);
                
                place_plant(x_index, noise, y_index);
                verts[vert_index] = new Vector3(x_index, noise, y_index);
                uvs[vert_index] = new Vector2(x_index / grid_size, (grid_size - y_index) / grid_size);
                
                // if (terrain_selction == Terrain.snowy) {
                    colors[vert_index] = get_color(noise);
                // }
                
                //else, colors is handled in the texture generation part. 
            }
        }

        int ntris = 0;
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

        mesh.uv = uvs;

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
        
        Renderer rend = s.GetComponent<Renderer>();

        //get the renderer, attach a material that uses a vertex shader 
        //thus, we can color each vertex and it mixes the colors. 
        //note: this method is an alternative to using a texture 2D and potentially allows for a different gradient of colors to be made
        if (terrain_selection == Terrain.Texture2D) {
            Texture2D texture = make_a_texture(mesh);
            // rend.material.color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
            rend.material.mainTexture = texture;
        } else {
            Material material = new Material(Shader.Find("Particles/Standard Surface"));
            rend.material = material;
        }
       
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
        float total = 0.00f;
        int pow = 1;
        for (int i = 0; i < octaves; i++) {
            total += 1.0f / pow * Mathf.PerlinNoise(pow * x + x_offset, pow * y + y_offset);
            pow *= 2;
        }

        return total;
        // return Mathf.PerlinNoise(x + x_offset , y + y_offset) 
        // + 1.0f / 2 * Mathf.PerlinNoise(2 * x + x_offset, 2 * y + y_offset) 
        // + 1.0f / 4 * Mathf.PerlinNoise( 4 * x + x_offset, 4 * y + y_offset);
    }

    //for some reason, the heights are super high. this is wehre we assign color based on height
    //this allows us to create mountains, rivers, etc. coloring based on height
    Color get_color(float x) {
        
        //spooky coloring below
        if (terrain_selection == Terrain.snowy || terrain_selection == Terrain.Texture2D) {
            if (x > 10.0f / 10) return Color.white;
            else if (x  > 2.5 / 10.0f) return new Color(25, 25, 25) / 255.0f;   //dark grey
            else if (x > 0.5 /10.0f) return Color.red;
            else return Color.green;
        } else if (terrain_selection == Terrain.sandy) {     //sand biome
            Color low = new Color(150, 114, 22) / 255.0f;
            Color high = new Color(228, 214, 172) / 255.0f;
            return Color.Lerp(low, high, x - 0.3f);
        } else if (terrain_selection == Terrain.greyscale) {
            // x -= 0.4f;
            x = 1 - x;
            return new Color(x,x,x);
        } else if (terrain_selection == Terrain.greenland) {
            // Color brown = new Color(168, 81, 3) / 255.0f;
            Color brown = new Color(101, 78, 44) / 255.0f;
            // Color green = new Color(99, 180, 0) / 255.0f;
            Color green = new Color(19,109,21) / 255.0f;
            Color blue  = new Color(62,164,240) / 255.0f;
            // return Color.Lerp(brown, green, x - 0.3f);    

            if (x > 0.7f) return green;
            if (x > 0.45f) return brown;
            else return blue;

        }
        return Color.clear; //error case
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

    Color get_color_new(float x) {
        
        //spooky coloring below
        // if (x > 10.0f / 10) return Color.white;
        // else if (x  > 2.5 / 10.0f) return new Color(25, 25, 25) / 255.0f;   //dark grey
        // else if (x > 0.5 /10.0f) return Color.red;
        // else return Color.green;
        if (x > 0.75) return Color.red;
        else return Color.blue;
    }
    Texture2D make_a_texture(Mesh mesh) {
        int len = grid_verts_per_side;
        // Vector2[] uv = new Vector2[4];
        // uv[0] = new Vector2(len, 0);
        // uv[1] = new Vector2(len, len);
        // uv[2] = new Vector2(0, len);
        // uv[3] = new Vector2(0, 0);
        // uv[0] = new Vector2(grid_size, 0);
        // uv[1] = new Vector2(grid_size, grid_size);
        // uv[2] = new Vector2(0, grid_size);
        // uv[3] = new Vector2(0, 0);
        // mesh.uv = uv;
        // mesh.RecalculateNormals();

;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];
        Texture2D texture = new Texture2D(len, len);
        // Color[] colors = new Color[len * len];

        // for (int i = 0; i < uvs.Length; i++)
        // {
        //     uvs[i] = new Vector2((vertices[i].x) / grid_size, (grid_size - vertices[i].z) / grid_size);
        //     // colors[i] = get_color(vertices[i].y);
        // }
        // mesh.uv = uvs;
        // mesh.RecalculateNormals();

       
    //    for (int i = 0; i < len; i++) {
    //         for (int j = len - 1; j >= 0; j--) {
    //             colors[i * len + (len - 1 - j)] = get_color(mesh.vertices[i * len + j].y);
    //         }
    //     }
        // for (int i = 0; i < len; i++) {
        //     for (int j = 0; j < len; j++) {
        //         float t = mesh.vertices[i * len + j].y;
        //         // colors [i * len + j] = get_color_new(mesh.vertices[i * len + j].y);
        //         // colors[i * len + j] = new Color(t, t, t);
        //         // colors[j * len + i] = get_color(mesh.vertices[j * len + i].y);
        //         colors[i * len + j] = get_color(mesh.vertices[i * len + j].y);
        //     }
        // }

        Color[] colors = new Color[mesh.vertices.Length];

        for (int i = 0; i < colors.Length; i++) {
            // colors[i] = new Color(vertices[i].x, vertices[i].y, vertices[i].z);
            colors[i] = new Color(0.25f, 0.25f, vertices[i].y / 2.5f + 0.25f);
        }
        // texture.SetPixels(mesh.colors);
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }
}
