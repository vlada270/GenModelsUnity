using UnityEngine;

// If your other brushes inherit TerrainBrush, do that here too
public class PerlinNoiseBrush : TerrainBrush
{
    [Header("FBM settings")]
    public float baseFrequency = 0.05f;   // how "zoomed in" the noise is
    public float baseAmplitude = 5f;      // how tall the terrain variation is
    public int octaves = 4;               // number of layers of noise
    public float lacunarity = 2f;         // frequency multiplier per octave
    public float gain = 0.5f;             // amplitude multiplier per octave
    public float strength = 3f;

    [Header("Offset so it�s not always the same pattern")]
    public Vector2 noiseOffset = Vector2.zero;
    public float heightOffset = 0f;       // shift everything up/down

    public override void draw(int x, int z)
    {
        for (int zi = -radius; zi <= radius; zi++)
        {
            for (int xi = -radius; xi <= radius; xi++)
            {
                int dx = xi;
                int dz = zi;
                if (dx * dx + dz * dz > radius * radius)
                    continue;

                // World-ish coordinates for noise
                float px = (x + xi) * baseFrequency + noiseOffset.x;
                float pz = (z + zi) * baseFrequency + noiseOffset.y;

                // --- FBM (fractal Brownian motion) ---
                float value = FBM(px, pz);

                // Center noise around 0: [-0.5, 0.5]
                float centered = value - 0.5f;

                // Scale by brush strength
                float delta = centered * strength;

                // Add to existing terrain height
                float current = terrain.get(x + xi, z + zi);
                float result = current + delta;

                terrain.set(x + xi, z + zi, result);
            }
        }
    }

    // Simple FBM using Mathf.PerlinNoise
    float FBM(float x, float z)
    {
        float sum = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float maxAmp = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float n = Mathf.PerlinNoise(x * frequency, z * frequency); // [0..1]
            sum += n * amplitude;
            maxAmp += amplitude;

            amplitude *= gain;       // shrink each layer
            frequency *= lacunarity; // increase frequency each layer
        }

        // Normalize back to [0..1]
        return sum / maxAmp;
    }
}
