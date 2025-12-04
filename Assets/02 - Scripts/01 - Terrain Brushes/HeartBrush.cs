using UnityEngine;

public class HeartBrush : TerrainBrush
{
    public float targetHeight = 8f;   // make this clearly different from current terrain

    // Scale factor makes the heart fill the brush radius better.
    // 1.3f is a good starting point (classic heart curve needs a bit more room than [-1,1]).
    public float shapeScale = 1.3f;

    public override void draw(int x, int z)
    {
        for (int zi = -radius; zi <= radius; zi++)
        {
            for (int xi = -radius; xi <= radius; xi++)
            {
                // Normalize the brush-local coordinates to roughly [-1,1]
                float nx = (xi / (float)radius) * shapeScale;
                float nz = (zi / (float)radius) * shapeScale;

                // Implicit heart: (x^2 + y^2 - 1)^3 - x^2 * y^3 <= 0
                float x2 = nx * nx;
                float z2 = nz * nz;
                float inside = Mathf.Pow(x2 + z2 - 1f, 3f) - x2 * (nz * nz * nz);

                if (inside <= 0f) // point is inside the heart
                {
                    terrain.set(x + xi, z + zi, targetHeight);
                }
            }
        }
    }
}
