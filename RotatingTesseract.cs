using System.Collections.Generic;
using UnityEngine;

public class RotatingTesseract : MonoBehaviour
{
    public float edgeLength = 1f;
    public float rotationSpeed = 20f;
    public Material lineMaterial;

    private Vector4[] vertices;
    private List<(int, int)> edges;

    private void Start()
    {
        vertices = GenerateVertices();
        edges = GenerateEdges();

        foreach (var edge in edges)
        {
            GameObject edgeObj = new GameObject($"Edge_{edge.Item1}_{edge.Item2}");
            edgeObj.transform.SetParent(transform);

            LineRenderer lineRenderer = edgeObj.AddComponent<LineRenderer>();
            lineRenderer.material = lineMaterial;
            lineRenderer.widthMultiplier = 0.05f;
            lineRenderer.positionCount = 2;
        }
    }

    private void Update()
    {
        float angle = rotationSpeed * Time.deltaTime;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = RotateXW(vertices[i], angle);
            vertices[i] = RotateYZ(vertices[i], angle);
        }

        int edgeIndex = 0;
        foreach (var edge in edges)
        {
            LineRenderer lineRenderer = transform.GetChild(edgeIndex).GetComponent<LineRenderer>();

            Vector3 startPos = StereographicProjection(vertices[edge.Item1]);
            Vector3 endPos = StereographicProjection(vertices[edge.Item2]);

            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);

            edgeIndex++;
        }
    }

    private Vector4[] GenerateVertices()
    {
        Vector4[] vertices = new Vector4[16];
        int index = 0;

        for (int w = 0; w < 2; w++)
        {
            for (int z = 0; z < 2; z++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        vertices[index++] = new Vector4(x * edgeLength, y * edgeLength, z * edgeLength, w * edgeLength);
                    }
                }
            }
        }

        return vertices;
    }

    private List<(int, int)> GenerateEdges()
    {
        List<(int, int)> edges = new List<(int, int)>();

        for (int i = 0; i < 16; i++)
        {
            for (int j = i + 1; j < 16; j++)
            {
                Vector4 difference = GenerateVertices()[i] - GenerateVertices()[j];

                int nonZeroComponents = 0;
                if (difference.x != 0) nonZeroComponents++;
                if (difference.y != 0) nonZeroComponents++;
                if (difference.z != 0) nonZeroComponents++;
                if (difference.w != 0) nonZeroComponents++;

                if (nonZeroComponents == 1)
                {
                    edges.Add((i, j));
                }
            }
        }

        return edges;
    }

    private Vector4 RotateXW(Vector4 v, float angle)
    {
        float sinAngle = Mathf.Sin(angle * Mathf.Deg2Rad);
        float cosAngle = Mathf.Cos(angle * Mathf.Deg2Rad);

        float newX = v.x * cosAngle + v.w * sinAngle;
        float newW = -v.x * sinAngle + v.w * cosAngle;

        return new Vector4(newX, v.y, v.z, newW);
    }

    private Vector4 RotateYZ(Vector4 v, float angle)
    {
        float sinAngle = Mathf.Sin(angle * Mathf.Deg2Rad);
        float cosAngle = Mathf.Cos(angle * Mathf.Deg2Rad);

        float newY = v.y * cosAngle + v.z * sinAngle;
        float newZ = -v.y * sinAngle + v.z * cosAngle;

        return new Vector4(v.x, newY, newZ, v.w);
    }

    private Vector3 StereographicProjection(Vector4 v)
    {
        float factor = 1f / (1f + v.w);
        return new Vector3(v.x * factor, v.y * factor, v.z * factor) * edgeLength;
    }
}
