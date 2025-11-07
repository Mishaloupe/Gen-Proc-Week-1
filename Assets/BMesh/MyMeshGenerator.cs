using UnityEngine;
using static BMesh;

public class MyMeshGenerator : MonoBehaviour
{
    public int width = 15;
    public int height = 15;

    BMesh mesh;

    BMesh GenerateGrid()
    {
        BMesh bm = new BMesh();
        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                bm.AddVertex(i, 0, j); // vertex # i + j * w
                if (i > 0 && j > 0) bm.AddFace(i + j * width, i - 1 + j * width, i - 1 + (j - 1) * width, i + (j - 1) * width);
            }
        }
        return bm;
    }

    void Start()
    {
        mesh = GenerateGrid();
        foreach (Vertex v in mesh.vertices)
        {
            v.point.y = Mathf.Sin(v.point.x + v.point.z); // Vertical displacement
        }
        BMeshUnity.SetInMeshFilter(mesh, GetComponent<MeshFilter>());
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        if (mesh != null) BMeshUnity.DrawGizmos(mesh);
    }
}