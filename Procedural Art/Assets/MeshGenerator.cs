using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour {

    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public int xSize = 100;
    public int zSize = 100;
 
    public float offsetX = 100f;
    public float offsetY = 100f;
    public float scale = 20f;
    private float y = 0;
    public int randomHeight = 10;
    private int colorCount;

    Color[] colors;
    public Gradient gradientOne;
    public Gradient gradientTwo;


    float minTerrainHeight;
    float maxTerrainHeight; 

    Material m_Material; 

	// Use this for initialization
	void Start () {
        
        colorCount = 0;
    }

    public void Pink()
    {
        colorCount = 1;

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        offsetX = Random.Range(0f, 99999f);
        offsetY = Random.Range(0f, 99999f);
        randomHeight = Random.Range(4, 11);
        StartCoroutine(CreateShape());
        m_Material = GetComponent<Renderer>().material;

    }

    public void Blue()
    {
        colorCount = 2;

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        offsetX = Random.Range(0f, 99999f);
        offsetY = Random.Range(0f, 99999f);
        randomHeight = Random.Range(5, 10);
        StartCoroutine(CreateShape());
        m_Material = GetComponent<Renderer>().material;

    }

    private void Update()
    {
        
        UpdateMesh();

    }

    
    IEnumerator CreateShape ()
    {
        if (colorCount >= 1)
        {

            vertices = new Vector3[(xSize + 1) * (zSize + 1)];


            for (int i = 0, z = 0; z <= zSize; z++)
            {
                for (int x = 0; x <= xSize; x++)
                {
                    float xCoord = (float)x * scale + offsetX;
                    float yCoord = (float)z * scale + offsetY;
                    float y = Mathf.PerlinNoise(xCoord, yCoord) * randomHeight - 1;

                    vertices[i] = new Vector3(x, y, z);

                    if (y > maxTerrainHeight)
                        maxTerrainHeight = y;

                    if (y < minTerrainHeight)
                        minTerrainHeight = y;

                    i++;
                }
            }

            triangles = new int[xSize * zSize * 6];

            int vert = 0;
            int tris = 0;

            for (int z = 0; z < zSize; z++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    triangles[tris + 0] = vert + 0;
                    triangles[tris + 1] = vert + xSize + 1;
                    triangles[tris + 2] = vert + 1;
                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 4] = vert + xSize + 1;
                    triangles[tris + 5] = vert + xSize + 2;

                    vert++;
                    tris += 6;

                    yield return new WaitForSeconds(.01f);
                }
                vert++;
            }

            if (colorCount == 1)
            {
                colors = new Color[vertices.Length];

                for (int i = 0, z = 0; z <= zSize; z++)

                {
                    for (int x = 0; x <= xSize; x++)
                    {
                        float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[i].y);
                        colors[i] = gradientOne.Evaluate(height);
                        i++;

                    }
                }
            }

            if (colorCount == 2)
            {
                colors = new Color[vertices.Length];

                for (int i = 0, z = 0; z <= zSize; z++)

                {
                    for (int x = 0; x <= xSize; x++)
                    {
                        float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[i].y);
                        colors[i] = gradientTwo.Evaluate(height);
                        i++;

                    }
                }
            }
            
        }
        
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();
      

     

    }

    private void OnDrawGizmos()
    {

        if (vertices == null)
            return;


        for(int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], .1f);
        }
    }
}
