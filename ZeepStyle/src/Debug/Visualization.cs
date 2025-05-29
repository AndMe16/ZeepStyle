using UnityEngine;

namespace ZeepStyle.Debug;

public class StyleGizmoVisualization : MonoBehaviour
{
    private const float AxisLength = 3.0f; // Length of the visualized axes

    private const float PlaneSize = 3.0f; // Size of the reference planes

    // Visualization
    private GameObject xAxisVisual, yAxisVisual, zAxisVisual;
    private GameObject xyPlane, yzPlane, zxPlane;


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

    private static GameObject CreateAxisVisual(Vector3 axisDirection, Color color, Rigidbody rb)
    {
        var axisVisual = new GameObject($"AxisVisual_{axisDirection}")
        {
            transform =
            {
                position = rb.position // Set initial position to the rigidbody position
            }
        };

        // Add a LineRenderer to visualize the axis
        var lineRenderer = axisVisual.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, rb.position); // Start point
        lineRenderer.SetPosition(1, rb.position + axisDirection * AxisLength); // End point in the axis direction
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        return axisVisual;
    }

    public void CleanupAxisVisuals()
    {
        if (xAxisVisual) Destroy(xAxisVisual);
        if (yAxisVisual) Destroy(yAxisVisual);
        if (zAxisVisual) Destroy(zAxisVisual);
    }

    private static void UpdateAxisVisual(GameObject axisVisual, Vector3 axisDirection, Rigidbody rb)
    {
        if (!axisVisual) return;
        axisVisual.transform.position = rb.position; // Keep the position at the rigidbody
        var lineRenderer = axisVisual.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, rb.position);
        lineRenderer.SetPosition(1,
            rb.position + axisDirection * AxisLength); // Update end point based on an axis direction
    }

    public void CreateReferencePlanes(Quaternion initialRotation, Rigidbody rb)
    {
        // Create circular planes based on the initial rotation
        xyPlane = CreateCircularPlane(initialRotation * Vector3.up, Color.red, "XY Plane", rb);
        yzPlane = CreateCircularPlane(initialRotation * Vector3.right, Color.green, "YZ Plane", rb);
        zxPlane = CreateCircularPlane(initialRotation * Vector3.forward, Color.blue, "ZX Plane", rb);
    }

    private static GameObject CreateCircularPlane(Vector3 normal, Color color, string planeName, Rigidbody rb)
    {
        var plane = new GameObject(planeName)
        {
            transform =
            {
                position = rb.position, // Set the plane's initial position
                up = normal // Align the plane's normal to the calculated normal
            }
        };
        plane.AddComponent<MeshFilter>().mesh = CreateCircularMesh(); // Assign circular mesh
        plane.AddComponent<MeshRenderer>();

        // Set the plane's material and color
        var planeRenderer = plane.GetComponent<Renderer>();
        planeRenderer.material = new Material(Shader.Find("Standard"))
        {
            color = color
        };

        // Disable the collider for non-collidable behavior
        var collider = plane.GetComponent<Collider>();
        if (collider) collider.enabled = false; // Disable the collider

        return plane;
    }

    // Generate a circular mesh for the planes
    private static Mesh CreateCircularMesh()
    {
        var mesh = new Mesh();

        const int segments = 50; // Number of segments in the circle
        const float angleStep = 360f / segments;

        var vertices = new Vector3[segments + 1];
        var triangles = new int[segments * 3];

        vertices[0] = Vector3.zero; // Center of the circle

        for (var i = 0; i < segments; i++)
        {
            var angle = Mathf.Deg2Rad * (i * angleStep);
            vertices[i + 1] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * PlaneSize;

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
        if (xyPlane) xyPlane.transform.position = rb.position;
        if (yzPlane) yzPlane.transform.position = rb.position;
        if (zxPlane) zxPlane.transform.position = rb.position;
    }

    public void CleanupReferencePlanes()
    {
        if (xyPlane)
            Destroy(xyPlane);
        if (yzPlane)
            Destroy(yzPlane);
        if (zxPlane)
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