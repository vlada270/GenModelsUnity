using UnityEngine;

/// <summary>
/// Random paint brush:
/// - On each mouse *press*, picks a random terrain texture layer (color).
/// - While you drag, it paints that same color under the brush.
/// - Adds a bit of roughness to the height under the brush.
/// - Height and color use the same coordinates, so they line up perfectly.
/// </summary>
public class CrayonBrush : TerrainBrush
{
    [Header("Height Roughness")]
    [Tooltip("How strong the vertical bumps are under the brush.")]
    public float heightRoughness = 0.3f;

    [Tooltip("Softness of the brush edge (1 = normal, >1 = softer edge).")]
    [Range(0.1f, 3f)]
    public float edgeSoftness = 1.0f;
    [SerializeField] private int radius = 10;

    private int currentTextureIndex = 0;
    private bool hasStrokeColor = false;

    // This is called from CustomTerrain.Update() with the world X,Z under the mouse
    public override void callDraw(float worldX, float worldZ)
    {
        // Get alphamaps to know how many texture layers we have
        float[,,] alpha = terrain.getTextures();
        int layers = alpha.GetLength(2);
        if (layers == 0)
            return;

        // New mouse drag -> pick a new random color
        // Input.GetMouseButtonDown(0) is true only on the first frame of the click
        if (Input.GetMouseButtonDown(0) || !hasStrokeColor)
        {
            currentTextureIndex = Random.Range(0, layers);
            hasStrokeColor = true;
        }

        // Convert world position to heightmap grid coordinates
        Vector3 grid = terrain.world2grid(worldX, worldZ);

        // Do the actual painting around this grid point
        draw((int)grid.x, (int)grid.z);

        // Save height + textures back to the Unity Terrain
        terrain.save();
        terrain.saveTextures();
    }

    public override void draw(float x, float z)
    {
        draw((int)x, (int)z);
    }

    public override void draw(int gx, int gz)
    {
        // ----- sizes -----
        Vector3 gridSize = terrain.gridSize();   // (heightmapWidth, 0, heightmapHeight)
        int heightWidth = (int)gridSize.x;
        int heightHeight = (int)gridSize.z;

        Vector2 texSize = terrain.textureSize(); // (alphamapWidth, alphamapHeight)
        int alphaWidth = (int)texSize.x;
        int alphaHeight = (int)texSize.y;

        float[,,] alpha = terrain.getTextures();
        int layers = alpha.GetLength(2);
        if (layers == 0) return;

        int r = radius;

        // Loop on a disk of radius r around (gx,gz)
        for (int dz = -r; dz <= r; dz++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                int hx = gx + dx;  // heightmap x index (can be out of bounds, but get/set wrap)
                int hz = gz + dz;  // heightmap z index

                float dist = Mathf.Sqrt(dx * dx + dz * dz);
                if (dist > r)
                    continue; // outside the circular brush

                // Soft falloff from center (1) to edge (0)
                float t = 1f - (dist / Mathf.Max(0.001f, r));
                t = Mathf.Pow(t, edgeSoftness);

                // ----- HEIGHT ROUGHNESS -----
                float currentHeight = terrain.get(hx, hz); // get() already wraps indices
                float delta = (Random.value * 2f - 1f) * heightRoughness * t;
                terrain.set(hx, hz, currentHeight + delta);

                // ----- COLOR PAINTING -----
                // Wrap indices to be inside [0, width-1] for mapping
                int wx = (hx % heightWidth + heightWidth) % heightWidth;
                int wz = (hz % heightHeight + heightHeight) % heightHeight;

                // Map heightmap grid -> alphamap grid using the same relative position
                float v = wx / (float)(heightWidth - 1);   // 0..1 across heightmap X
                float u = wz / (float)(heightHeight - 1);   // 0..1 across heightmap Z

                int ax = Mathf.Clamp(Mathf.RoundToInt(u * (alphaWidth - 1)), 0, alphaWidth - 1);
                int az = Mathf.Clamp(Mathf.RoundToInt(v * (alphaHeight - 1)), 0, alphaHeight - 1);

                // Strength of painting at this point
                float strength = t;

                // Boost chosen layer, fade the others
                float sum = 0f;
                for (int l = 0; l < layers; l++)
                {
                    if (l == currentTextureIndex)
                    {
                        // Increase this texture’s weight
                        alpha[ax, az, l] = Mathf.Clamp01(alpha[ax, az, l] + strength);
                    }
                    else
                    {
                        // Fade others a bit so the chosen color dominates
                        alpha[ax, az, l] *= (1f - 0.5f * strength);
                    }

                    sum += alpha[ax, az, l];
                }

                // Normalize so all texture weights sum to 1
                if (sum > 0.0001f)
                {
                    for (int l = 0; l < layers; l++)
                        alpha[ax, az, l] /= sum;
                }
            }
        }
    }
}
