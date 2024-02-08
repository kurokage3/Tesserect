using System.Collections.Generic;
using UnityEngine;

// This class represents a rotating Tesseract (4D hypercube) in 3D space using Unity's GameObjects.
public class RotatingTesseract : MonoBehaviour
{
    public float edgeLength = 1f; // The length of each edge in the Tesseract.
    public float rotationSpeed = 20f; // The speed at which the Tesseract rotates.
    public Material lineMaterial; // The material used for the lines representing the edges.

    private Vector4[] vertices; // An array to store the vertices of the Tesseract.
    private List<(int, int)> edges; // A list to store pairs of vertices that form the edges.
    private LineRenderer[] lineRenderers; // An array to cache the LineRenderer components.

    // Start is called before the first frame update to initialize the Tesseract.
    private void Start()
    {
        vertices = GenerateVertices(); // Generate the vertices of the Tesseract.
        edges = GenerateEdges(); // Generate the edges by pairing vertices.
        InitializeEdges(); // Initialize the GameObjects and LineRenderers for each edge.
    }

    // Update is called once per frame to rotate the Tesseract and update the edge positions.
    private void Update()
    {
        RotateVertices(); // Rotate the vertices of the Tesseract.
        UpdateEdgePositions(); // Update the positions of the edges based on the rotated vertices.
    }

    // Initializes the LineRenderers for each edge of the Tesseract.
    private void InitializeEdges()
    {
        lineRenderers = new LineRenderer[edges.Count]; // Initialize the array based on the number of edges.

        for (int i = 0; i < edges.Count; i++)
        {
            var (start, end) = edges[i]; // Get the start and end vertex indices for each edge.
            GameObject edgeObj = new GameObject($"Edge_{start}_{end}"); // Create a new GameObject for the edge.
            edgeObj.transform.SetParent(transform); // Set the parent of the edge GameObject to this transform.

            LineRenderer lineRenderer = edgeObj.AddComponent<LineRenderer>(); // Add a LineRenderer component to the edge GameObject.
            lineRenderer.material = lineMaterial; // Set the material of the LineRenderer.
            lineRenderer.widthMultiplier = 0.05f; // Set the width of the LineRenderer.
            lineRenderer.positionCount = 2; // Set the number of positions to 2, since an edge is a line between two vertices.

            lineRenderers[i] = lineRenderer; // Cache the LineRenderer component.
        }
    }

    // Rotates the vertices of the Tesseract by applying 4D rotations.
    private void RotateVertices()
    {
        float angle = rotationSpeed * Time.deltaTime; // Calculate the rotation angle based on the rotation speed and time elapsed since the last frame.
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = RotateXW(vertices[i], angle); // Rotate around the XW plane.
            vertices[i] = RotateYZ(vertices[i], angle); // Rotate around the YZ plane.
        }
    }

    // Updates the positions of the edges based on the rotated vertices.
    private void UpdateEdgePositions()
    {
        for (int i = 0; i < edges.Count; i++)
        {
            var (start, end) = edges[i]; // Get the start and end vertex indices for each edge.
            Vector3 startPos = StereographicProjection(vertices[start]); // Project the start vertex from 4D to 3D.
            Vector3 endPos = StereographicProjection(vertices[end]); // Project the end vertex from 4D to 3D.

            lineRenderers[i].SetPosition(0, startPos); // Set the start position of the LineRenderer.
            lineRenderers[i].SetPosition(1, endPos); // Set the end position of the LineRenderer.
        }
    }

    // Generates the vertices of the Tesseract in 4D space.
    private Vector4[] GenerateVertices()
    {
        Vector4[] vertices = new Vector4[16]; // Initialize an array to store 16 vertices.
        int index = 0; // A counter to keep track of the current index in the array.

        // Nested loops to generate all combinations of vertices in 4D space.
        for (int w = 0; w < 2; w++)
        {
            for (int z = 0; z < 2; z++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        vertices[index++] = new Vector4(x * edgeLength, y * edgeLength, z * edgeLength, w * edgeLength); // Assign the vertex position based on the edge length.
                    }
                }
            }
        }

        return vertices; // Return the array of vertices.
    }

    // Generates the edges of the Tesseract by pairing vertices.
    private List<(int, int)> GenerateEdges()
    {
        List<(int, int)> edges = new List<(int, int)>(); // Initialize a list to store the edges.

        // Double loop to compare each vertex with every other vertex.
        for (int i = 0; i < 16; i++)
        {
            for (int j = i + 1; j < 16; j++)
            {
                Vector4 difference = vertices[i] - vertices[j]; // Calculate the difference between two vertices to determine if they form an edge.

                int nonZeroComponents = 0; // A counter to track the number of non-zero components in the difference vector.
                if (difference.x != 0) nonZeroComponents++;
                if (difference.y != 0) nonZeroComponents++;
                if (difference.z != 0) nonZeroComponents++;
                if (difference.w != 0) nonZeroComponents++;

                if (nonZeroComponents == 1) // If there is exactly one non-zero component, then the vertices form an edge.
                {
                    edges.Add((i, j)); // Add the pair of vertices to the list of edges.
                }
            }
        }

        return edges; // Return the list of edges.
    }

    // Rotates a vertex around the XW plane in 4D space.
    private Vector4 RotateXW(Vector4 v, float angle)
    {
        float sinAngle = Mathf.Sin(angle * Mathf.Deg2Rad); // Calculate the sine of the angle.
        float cosAngle = Mathf.Cos(angle * Mathf.Deg2Rad); // Calculate the cosine of the angle.

        float newX = v.x * cosAngle + v.w * sinAngle; // Calculate the new X component.
        float newW = -v.x * sinAngle + v.w * cosAngle; // Calculate the new W component.

        return new Vector4(newX, v.y, v.z, newW); // Return the rotated vertex.
    }

    // Rotates a vertex around the YZ plane in 4D space.
    private Vector4 RotateYZ(Vector4 v, float angle)
    {
        float sinAngle = Mathf.Sin(angle * Mathf.Deg2Rad); // Calculate the sine of the angle.
        float cosAngle = Mathf.Cos(angle * Mathf.Deg2Rad); // Calculate the cosine of the angle.

        float newY = v.y * cosAngle + v.z * sinAngle; // Calculate the new Y component.
        float newZ = -v.y * sinAngle + v.z * cosAngle; // Calculate the new Z component.

        return new Vector4(v.x, newY, newZ, v.w); // Return the rotated vertex.
    }

    // Projects a 4D vertex to 3D space using stereographic projection.
    private Vector3 StereographicProjection(Vector4 v)
    {
        float factor = 1f / (1f + v.w); // Calculate the projection factor.
        return new Vector3(v.x * factor, v.y * factor, v.z * factor) * edgeLength; // Return the projected 3D position scaled by the edge length.
    }
}
