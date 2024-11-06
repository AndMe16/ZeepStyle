using UnityEngine;

public class Style_GizmoVisualization : MonoBehaviour
{
    // Visualization
    private GameObject xAxisVisual, yAxisVisual, zAxisVisual;
    private const float axisLength = 3.0f;  // Length of the visualized axes
    private GameObject xyPlane, yzPlane, zxPlane;
    private const float planeSize = 3.0f;  // Size of the reference planes


    // Visualization
    public void CreateAxisVisuals(Rigidbody rb)
    {
        // X-axis (Roll)
        xAxisVisual = CreateAxisVisual(Vector3.right, Color.red, rb);
        // Y-axis (Pitch)
        yAxisVisual = CreateAxisVisual(Vector3.up, Color.green, rb);
        // Z-axis (Yaw)
        zAxisVisual = CreateAxisVisual(Vector3.forward, Color.blue, rb);
    }

    GameObject CreateAxisVisual(Vector3 axisDirection, Color color, Rigidbody rb)
    {
        GameObject axisVisual = new GameObject($"AxisVisual_{axisDirection}");
        axisVisual.transform.position = rb.position; // Set initial position to the rigidbody position

        // Add a LineRenderer to visualize the axis
        LineRenderer lineRenderer = axisVisual.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, rb.position); // Start point
        lineRenderer.SetPosition(1, rb.position + axisDirection * axisLength); // End point in the axis direction
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        return axisVisual;
    }

    public void CleanupAxisVisuals()
    {
        if (xAxisVisual != null) Destroy(xAxisVisual);
        if (yAxisVisual != null) Destroy(yAxisVisual);
        if (zAxisVisual != null) Destroy(zAxisVisual);
    }

    private void UpdateAxisVisual(GameObject axisVisual, Vector3 axisDirection, Rigidbody rb)
    {
        if (axisVisual != null)
        {
            axisVisual.transform.position = rb.position; // Keep the position at the rigidbody
            LineRenderer lineRenderer = axisVisual.GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, rb.position);
            lineRenderer.SetPosition(1, rb.position + axisDirection * axisLength); // Update end point based on axis direction
        }
    }

    public void CreateReferencePlanes(Quaternion initialRotation, Rigidbody rb)
    {
        // Create circular planes based on the initial rotation
        xyPlane = CreateCircularPlane(initialRotation * Vector3.up, Color.red, "XY Plane", rb);
        yzPlane = CreateCircularPlane(initialRotation * Vector3.right, Color.green, "YZ Plane", rb);
        zxPlane = CreateCircularPlane(initialRotation * Vector3.forward, Color.blue, "ZX Plane", rb);
    }

    GameObject CreateCircularPlane(Vector3 normal, Color color, string planeName, Rigidbody rb)
    {
        GameObject plane = new GameObject(planeName);
        plane.transform.position = rb.position; // Set the plane's initial position
        plane.transform.up = normal; // Align the plane's normal to the calculated normal
        plane.AddComponent<MeshFilter>().mesh = CreateCircularMesh(); // Assign circular mesh
        plane.AddComponent<MeshRenderer>();

        // Set the plane's material and color
        Renderer planeRenderer = plane.GetComponent<Renderer>();
        planeRenderer.material = new Material(Shader.Find("Standard"));
        planeRenderer.material.color = color;

        // Disable the collider for non-collidable behavior
        Collider collider = plane.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false; // Disable the collider
        }

        return plane;
    }

    // Generate a circular mesh for the planes
    Mesh CreateCircularMesh()
    {
        Mesh mesh = new Mesh();

        int segments = 50;  // Number of segments in the circle
        float angleStep = 360f / segments;

        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero; // Center of the circle

        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleStep);
            vertices[i + 1] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * planeSize;

            // Define the triangles (three points per triangle)
            if (i < segments - 1)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
            else
            {
                // Last triangle (connects last segment back to the first)
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = 1;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    public void UpdatePlanePositions(Rigidbody rb)
    {
        if (xyPlane != null) xyPlane.transform.position = rb.position;
        if (yzPlane != null) yzPlane.transform.position = rb.position;
        if (zxPlane != null) zxPlane.transform.position = rb.position;
    }

    public void CleanupReferencePlanes()
    {
        if (xyPlane != null)
            Destroy(xyPlane);
        if (yzPlane != null)
            Destroy(yzPlane);
        if (zxPlane != null)
            Destroy(zxPlane);
    }

    public void UpdateAllAxisVisuals(Rigidbody rb)
    {
        // Update the position and orientation of the axis visualizations based on the rigidbody's current rotation
        UpdateAxisVisual(xAxisVisual, rb.transform.right, rb);
        UpdateAxisVisual(yAxisVisual, rb.transform.up, rb);
        UpdateAxisVisual(zAxisVisual, rb.transform.forward, rb);
    }
}