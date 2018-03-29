using UnityEngine;
using System.Collections;


public class MeshGeneration : MonoBehaviour
{


    Mesh mesh;
    Vector3[] vertices;
    Vector2[] uv;
    int[] triangles;
    Vector3[] normals;
    void Start()
    {
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        Texture img = (Texture)Resources.Load("3124");

        gameObject.GetComponent<Renderer>().material.mainTexture = img;
        mesh = new Mesh();
        int m = 5; //row  
        int n = 10;  //col  
        float width = 8;
        float height = 6;
        vertices = new Vector3[(m + 1) * (n + 1)];//the positions of vertices 
        normals = new Vector3[(m + 1) * (n + 1)];


        uv = new Vector2[(m + 1) * (n + 1)];
        triangles = new int[6 * m * n];
        for (int i = 0; i < vertices.Length; i++)
        {
            float x = i % (n + 1);
            float y = i / (n + 1);
            float x_pos = x / n * width;
            float y_pos = y / m * height;
            vertices[i] = new Vector3(x_pos, y_pos, 0);
            normals[i] = new Vector3(0, 0, -1);
            float u = x / n;
            float v = y / m;
            uv[i] = new Vector2(u, v);




        }
        for (int i = 0; i < 2 * m * n; i++)
        {
            int[] triIndex = new int[3];
            if (i % 2 == 0)
            {
                triIndex[0] = i / 2 + i / (2 * n);
                triIndex[1] = triIndex[0] + 1;
                triIndex[2] = triIndex[0] + (n + 1);
            }
            else
            {
                triIndex[0] = (i + 1) / 2 + i / (2 * n);
                triIndex[1] = triIndex[0] + (n + 1);
                triIndex[2] = triIndex[1] - 1;


            }
            //三角形顶点顺序会影响显示方向
            triangles[i * 3] = triIndex[0];
            triangles[i * 3 + 1] = triIndex[2];
            triangles[i * 3 + 2] = triIndex[1];


        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.normals = normals;
        this.GetComponent<MeshFilter>().mesh = mesh;
    }

}
