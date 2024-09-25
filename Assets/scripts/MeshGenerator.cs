using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    int x_offset = 1000, y_offset = 1000; 
    private int grid_verts_per_side = 85;
    private float grid_size = 8.5f;
    
    public enum Terrain {
        Texture2D,
        snowy,
        sandy,
        greyscale,
        greenland
    }
    //parameter for terrain selection. Options are from the above enum. 
    public Terrain terrain_selection = Terrain.snowy;

    //number of octaves to use for Perlin noise. 
    //Note that number of octaves above 3 seem to look similar. 
    //octaves 1 and 2 look drastically different.
    public int octaves = 3;

    //seed influences the generation of plants on the terrain.
    public int seed = 0;

    //https://docs.unity3d.com/Manual/InstantiatingPrefabs.html
    
    //Texture2D/snowy biome plants
    public GameObject flower_prefab = null, tree_prefab = null;

    //sany biome plants
    public GameObject cactus_prefab = null, small_cactus_prefab = null;

    //greyscale plants
    public GameObject greyscale_flower_prefab = null, greyscale_tree_prefab = null;
    
    //maximum number of rows and columns of terrain you can generate. This is relatively arbitrary
    int max_rows = 51, max_cols = 51;
    
    //keep track of the terrains generated. so we can move and generate only where terrain is not seen. 
    bool[][] generated;
    
    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(seed);
        
        //instantiate generated array
        generated = new bool[max_rows][];
        
        for (int i = 0; i < max_rows; i++) {
            generated[i] = new bool[max_cols];
            for (int j = 0; j < max_cols; j++) {
                generated[i][j] = false;
            }
        }

        generate_chunk(grid_size, grid_verts_per_side, 0, 0);
   
        generated[max_rows/2][max_cols/2] = true;   //first chunk in center
       
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cam_pos = Camera.main.transform.position;
        
        
        //generate the coords of the cell in "generated" the camera is in based on the camera position. 
        //use integer division and offset by the first chunk being at the center (max_rows/2)
        int x = (int) Mathf.Floor(cam_pos.x / grid_size) + max_rows / 2;
        int z = (int) Mathf.Floor(cam_pos.z / grid_size) + max_cols / 2;


        //make sure we are in bounds and there is not a chunk there already. if so, generate new chunk
        if (x >= 0 && z >= 0 && x < max_rows && z < max_cols && generated[x][z] == false) {
            int x_coord = (int) Mathf.Floor(cam_pos.x / grid_size);
            int z_coord = (int) Mathf.Floor(cam_pos.z / grid_size);
           
            GameObject temp = generate_chunk(grid_size, grid_verts_per_side, x_coord * grid_size, z_coord * grid_size);
            generated[x][z] = true;
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

        GameObject plant;
        Vector3 angle = new Vector3(0, Random.value * 360, 0);
        
        //make sure height is withing low plant bounds, place with certain probability
        //make sure height is within high plant bounds, place with certain probability
        if (noise > 0.1 && noise < 0.5 && Random.value < 0.1) {
            plant = Instantiate(low_plant, new Vector3(x_index, noise + offset, y_index), Quaternion.identity);
            plant.transform.Rotate(angle);
        } else if (noise > 1 && Random.value < 0.01) {
           plant = Instantiate(high_plant, new Vector3(x_index, noise + offset, y_index), Quaternion.identity);
           plant.transform.Rotate(angle);
        }
    }

    GameObject generate_chunk(float grid_size, int grid_verts_per_side, float x_off, float y_off) {
        return mesh_to_game_object(create_plane(grid_size, grid_verts_per_side, x_off, y_off));
    }
    //create a new chunk of terrain as a mesh
    private Mesh create_plane(float grid_size, int grid_verts_per_side, float x_off, float y_off) {
        Vector3[] verts = new Vector3[grid_verts_per_side * grid_verts_per_side];  	                // the vertices of the mesh
	    int[] tris = new int[2 * (grid_verts_per_side - 1) * (grid_verts_per_side - 1) * 3];      	// the triangles of the mesh (triplets of integer references to vertices)
	    Mesh mesh = new Mesh();
        Color[] colors = new Color[verts.Length];;
        Vector2[] uvs = new Vector2[verts.Length];

        //generate the verticies for the plane
        for (int i = 0; i < grid_verts_per_side ; i++) {
            for (int j = 0; j < grid_verts_per_side; j++) {
                int vert_index = i * grid_verts_per_side + j;
                float x_index = grid_size / grid_verts_per_side * i + x_off;
                float y_index = grid_size / grid_verts_per_side * j + y_off;
                
                float noise = get_perlin_noise(x_index, y_index, x_offset, y_offset);
                
                place_plant(x_index, noise, y_index);
                verts[vert_index] = new Vector3(x_index, noise, y_index);
                uvs[vert_index] = new Vector2(x_index / grid_size, (grid_size - y_index) / grid_size);
                colors[vert_index] = get_color(noise);
            }
        }

        //generate triangles
        int ntris = 0;
        for (int i = 0; i < verts.Length - 1 - grid_verts_per_side - 1; i++) {
            if (i % grid_verts_per_side != grid_verts_per_side - 1 || i == 0) {
                int tl, tr, bl, br;
                tl = i;
                tr = i + 1;
                bl = i + grid_verts_per_side;
                br = i + grid_verts_per_side + 1;
                MakeQuad(tl, tr, br, bl, ntris, tris);
                // MakeQuad(bl, br, tr, tl, ntris, tris);   //direction check
                
                ntris += 2;
            }
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.RecalculateNormals();

        return mesh;
    }

    //convert mesh of chunk to game object
    GameObject mesh_to_game_object(Mesh mesh) {
        
        GameObject s = new GameObject("terrain chunk");
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();
        
        // associate the mesh with this object
        s.GetComponent<MeshFilter>().mesh = mesh;

        // change the color of the object
        Renderer rend = s.GetComponent<Renderer>();

        //color using Texture2D
        if (terrain_selection == Terrain.Texture2D) {
            Texture2D texture = make_a_texture(mesh);
            rend.material.mainTexture = texture;
        } else {
            //get the renderer, attach a material that uses a vertex shader 
            //thus, we can color each vertex and it mixes the colors. 
            //note: this method is an alternative to using a texture 2D and potentially allows for a different gradient of colors to be made
            Material material = new Material(Shader.Find("Particles/Standard Surface"));
            rend.material = material;
        }
       
        return s;
    }

    //perlin noise on variable number of octaves set by public variable. Note the offset variables to add offsets to account for unity replication at (0,0)
    float get_perlin_noise(float x, float y, float x_offset, float y_offset) {
        float total = 0.00f;
        int pow = 1;
        for (int i = 0; i < octaves; i++) {
            total += 1.0f / pow * Mathf.PerlinNoise(pow * x + x_offset, pow * y + y_offset);
            pow *= 2;
        }

        return total;
    }

    //for some reason, the heights are super high. this is wehre we assign color based on height or other attributes
    //this allows us to create mountains, rivers, etc. coloring based on height
    Color get_color(float x) {
        if (terrain_selection == Terrain.snowy || terrain_selection == Terrain.Texture2D) {     
            if (x > 10.0f / 10) return Color.white;
            else if (x  > 2.5 / 10.0f) return new Color(25, 25, 25) / 255.0f;   //dark grey
            else if (x > 0.5 /10.0f) return Color.blue;
            else return Color.green;
        } else if (terrain_selection == Terrain.sandy) {     //sand biome
            Color low = new Color(150, 114, 22) / 255.0f;
            Color high = new Color(228, 214, 172) / 255.0f;
            return Color.Lerp(low, high, x - 0.3f);
        } else if (terrain_selection == Terrain.greyscale) {    //spooky biome
            x = 1 - x;
            return new Color(x,x,x);
        } else if (terrain_selection == Terrain.greenland) {    //grassy biome
            Color brown = new Color(101, 78, 44) / 255.0f;
            Color green = new Color(19,109,21) / 255.0f;
            Color blue  = new Color(62,164,240) / 255.0f;
            
            if (x > 0.7f) return green;
            if (x > 0.45f) return brown;
            else return blue;

        }
        return Color.clear; //error case
    }

    // make a triangle from three vertex indices (clockwise order)
	void MakeTri(int i1, int i2, int i3, int ntris, int [] tris) {
		int index = ntris * 3;  // figure out the base index for storing triangle indices

		tris[index]     = i1;
		tris[index + 1] = i2;
		tris[index + 2] = i3;
	}

	// make a quadrilateral from four vertex indices (clockwise order)
	void MakeQuad(int i1, int i2, int i3, int i4, int ntris, int[] tris) {
		MakeTri (i1, i2, i3, ntris, tris);
		MakeTri (i1, i3, i4, ntris + 1, tris);
	}

    //create a texture2D 
    Texture2D make_a_texture(Mesh mesh) {
        int len = grid_verts_per_side;
       
        Vector3[] vertices = mesh.vertices;
        Texture2D texture = new Texture2D(len, len);
        

        Color[] colors = new Color[mesh.vertices.Length];

        //coloring scheme
        for (int i = 0; i < colors.Length; i++) {
            colors[i] = new Color(0.25f, 0.25f, vertices[i].y / 2.5f + 0.25f);
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }
}
