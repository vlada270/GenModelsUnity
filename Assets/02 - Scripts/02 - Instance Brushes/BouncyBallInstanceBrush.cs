using UnityEngine;

/// <summary>
/// Brush that throws bouncy balls of random colors onto the ground.
/// Inherit from your existing InstanceBrush.
/// Put this on the same GameObject as CustomTerrain / other brushes.
/// </summary>
public class BouncyBallInstanceBrush : InstanceBrush
{
    [Header("Ball settings")]
    [SerializeField] private GameObject ballPrefab;      // sphere with collider + Rigidbody
    [SerializeField] private int ballsPerClick = 3;      // how many balls per call
    [SerializeField] private float spawnHeight = 3f;     // how high above the ground they appear

    [Header("Throw settings")]
    [SerializeField] private float throwForce = 5f;      // initial velocity strength

    [Header("Raycast")]
    [SerializeField] private LayerMask groundMask = ~0;  // what counts as ground

    // Called by CustomTerrain with world X/Z under the mouse
    public override void callDraw(float worldX, float worldZ)
    {
        if (ballPrefab == null)
        {
            if (terrain != null && terrain.debug != null)
                terrain.debug.text = "Assign a ballPrefab to BouncyBallInstanceBrush!";
            return;
        }

        // Raycast down from high above to find the ground point
        Ray ray = new Ray(new Vector3(worldX, 1000f, worldZ), Vector3.down);
        if (!Physics.Raycast(ray, out RaycastHit hit, 2000f, groundMask))
            return;

        Vector3 centerOnGround = hit.point;
        
        // Spawn several balls inside the brush radius
        for (int i = 0; i < ballsPerClick; i++)
        {
            // random point in a circle (using base Brush.radius)
            Vector2 offset2D = Random.insideUnitCircle * radius;
            Vector3 spawnPos =
                centerOnGround +
                new Vector3(offset2D.x, 0f, offset2D.y) +
                Vector3.up * spawnHeight;

            // create ball
            GameObject ball = Instantiate(ballPrefab, spawnPos, Quaternion.identity);

            // --- RANDOM COLOR PER INSTANCE ---
            // Get Renderer (on root or child)
            Renderer rend = ball.GetComponent<Renderer>();
            if (rend == null)
                rend = ball.GetComponentInChildren<Renderer>();

            if (rend != null)
            {
                // this uses a UNIQUE material instance for this ball
                // nice bright colors: high saturation & value
                rend.material.color = Random.ColorHSV(
                    0f, 1f,   // hue range
                    0.6f, 1f, // saturation
                    0.7f, 1f  // value/brightness
                );
            }

            // --- THROW / BOUNCE WITH PHYSICS ---
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // mostly up, a bit sideways
                Vector3 throwDir = new Vector3(
                    Random.Range(-0.3f, 0.3f),
                    1f,
                    Random.Range(-0.3f, 0.3f)
                ).normalized;

                rb.AddForce(throwDir * throwForce, ForceMode.VelocityChange);
            }
        }
    }

    // Unused but required overrides
    public override void draw(float x, float z) { }
    public override void draw(int x, int z) { }
}
